namespace AutoPlayGame.Solver.Rules

open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module FindColorConfinement =

    let apply ctx state =
        let unknowCells = Helpers.unknowns ctx.Grid state

        let newFakes =
            unknowCells
            |> Array.groupBy _.Color
            |> Array.collect (fun (color, cells) ->
                let rowFakes =
                    let rows = cells |> Array.map _.Rank.Row |> Array.distinct

                    if rows.Length = 1 then
                        unknowCells
                        |> Array.filter (fun c -> c.Rank.Row = rows[0] && c.Color <> color)
                        |> Array.map _.Rank
                    else
                        Array.empty

                let colFakes =
                    let cols = cells |> Array.map _.Rank.Column |> Array.distinct

                    if cols.Length = 1 then
                        unknowCells
                        |> Array.filter (fun c -> c.Rank.Column = cols[0] && c.Color <> color)
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
