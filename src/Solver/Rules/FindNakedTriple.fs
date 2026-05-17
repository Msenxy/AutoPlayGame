namespace AutoPlayGame.Solver.Rules

open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module FindNakedTriple =

    let private triples (arr: 'a[]) = [|
        for i in 0 .. arr.Length - 3 do
            for j in i + 1 .. arr.Length - 2 do
                for k in j + 1 .. arr.Length - 1 do
                    yield arr[i], arr[j], arr[k]
    |]

    let apply ctx state =
        let unknowCells = Helpers.unknowns ctx.Grid state

        let colorGroups =
            unknowCells
            |> Array.groupBy _.Color
            |> Array.map (fun (color, cells) ->
                let rows = cells |> Array.map _.Rank.Row |> Array.distinct |> Set.ofArray
                let cols = cells |> Array.map _.Rank.Column |> Array.distinct |> Set.ofArray
                color, rows, cols)

        let newFakes =
            triples colorGroups
            |> Array.collect (fun ((colorA, rowsA, colsA), (colorB, rowsB, colsB), (colorC, rowsC, colsC)) ->

                let rowFakes =
                    let unionRows = rowsA |> Set.union rowsB |> Set.union rowsC

                    if unionRows.Count = 3 then
                        unknowCells
                        |> Array.filter (fun c ->
                            Set.contains c.Rank.Row unionRows
                            && c.Color <> colorA
                            && c.Color <> colorB
                            && c.Color <> colorC)
                        |> Array.map _.Rank
                    else
                        Array.empty

                let colFakes =
                    let unionCols = colsA |> Set.union colsB |> Set.union colsC

                    if unionCols.Count = 3 then
                        unknowCells
                        |> Array.filter (fun c ->
                            Set.contains c.Rank.Column unionCols
                            && c.Color <> colorA
                            && c.Color <> colorB
                            && c.Color <> colorC)
                        |> Array.map _.Rank
                    else
                        Array.empty

                Array.append rowFakes colFakes)

            |> Set.ofArray
            |> fun fakes -> Set.difference fakes state.RealPoints

        if Set.isEmpty newFakes then
            state
        else
            {
                state with
                    FakePoints = Set.union state.FakePoints newFakes
            }
