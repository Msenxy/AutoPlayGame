namespace AutoPlayGame.Solver.Rules

open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module FindCrossConfinement =

    let private pairs (arr: 'a[]) = [|
        for i in 0 .. arr.Length - 2 do
            for j in i + 1 .. arr.Length - 1 do
                yield arr[i], arr[j]
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
            pairs colorGroups
            |> Array.collect (fun ((colorA, rowsA, colsA), (colorB, rowsB, colsB)) ->

                if rowsA = rowsB && rowsA.Count = 2 && colsA = colsB && colsA.Count = 2 then

                    let lookedRows = rowsA
                    let lookedCols = colsA

                    unknowCells
                    |> Array.filter (fun c ->
                        let inLockedRow = Set.contains c.Rank.Row lookedRows
                        let inLockedCol = Set.contains c.Rank.Column lookedCols
                        let isAorB = c.Color = colorA || c.Color = colorB

                        (inLockedRow || inLockedCol) && not isAorB)
                    |> Array.map _.Rank
                else
                    Array.empty)

            |> Set.ofArray
            |> fun fakes -> Set.difference fakes state.RealPoints

        if Set.isEmpty newFakes then
            state
        else
            {
                state with
                    FakePoints = Set.union state.FakePoints newFakes
            }
