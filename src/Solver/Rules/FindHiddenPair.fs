namespace AutoPlayGame.Solver.Rules

open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module FindHiddenPair =

    let private pairs (arr: 'a[]) = [|
        for i in 0 .. arr.Length - 2 do
            for j in i + 1 .. arr.Length - 1 do
                yield arr[i], arr[j]
    |]

    let apply ctx state =
        let unknownCells = Helpers.unknowns ctx.Grid state

        let newFakes =
            [|
                let rowPairs = unknownCells |> Array.map _.Rank.Row |> Array.distinct |> pairs

                for r1, r2 in rowPairs do
                    let cellsInRows =
                        unknownCells |> Array.filter (fun c -> c.Rank.Row = r1 || c.Rank.Row = r2)

                    let colorsInRows = cellsInRows |> Array.map _.Color |> Array.distinct

                    if colorsInRows.Length = 2 then
                        let colorA = colorsInRows[0]
                        let colorB = colorsInRows[1]

                        yield!
                            unknownCells
                            |> Array.filter (fun c ->
                                c.Rank.Row <> r1 && c.Rank.Row <> r2 && (c.Color = colorA || c.Color = colorB))
                            |> Array.map _.Rank

                let colPairs = unknownCells |> Array.map _.Rank.Column |> Array.distinct |> pairs

                for c1, c2 in colPairs do
                    let cellsInCols =
                        unknownCells |> Array.filter (fun c -> c.Rank.Column = c1 || c.Rank.Column = c2)

                    let colorsInCols = cellsInCols |> Array.map _.Color |> Array.distinct

                    if colorsInCols.Length = 2 then
                        let colorA = colorsInCols[0]
                        let colorB = colorsInCols[1]

                        yield!
                            unknownCells
                            |> Array.filter (fun c ->
                                c.Rank.Column <> c1
                                && c.Rank.Column <> c2
                                && (c.Color = colorA || c.Color = colorB))
                            |> Array.map _.Rank
            |]
            |> Set.ofArray
            |> fun fakes -> Set.difference fakes state.RealPoints

        if Set.isEmpty newFakes then
            state
        else
            {
                state with
                    FakePoints = Set.union state.FakePoints newFakes
            }
