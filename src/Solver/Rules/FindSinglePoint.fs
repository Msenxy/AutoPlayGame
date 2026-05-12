namespace AutoPlayGame.Solver.Rules

open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module FindSinglePoint =

    let apply ctx state =
        let unknowCells = Helpers.unknowns ctx.Grid state

        let singleInRow =
            unknowCells
            |> Array.groupBy _.Rank.Row
            |> Array.choose (fun (_, cells) -> if cells.Length = 1 then Some cells[0].Rank else None)
            |> Set.ofArray

        let singleColumn =
            unknowCells
            |> Array.groupBy _.Rank.Column
            |> Array.choose (fun (_, cells) -> if cells.Length = 1 then Some cells[0].Rank else None)
            |> Set.ofArray

        let newReals = Set.union singleInRow singleColumn

        if Set.isEmpty newReals then
            state
        else
            let confirmed = Set.union state.RealPoints newReals

            let newFakes =
                (Set.empty, newReals)
                ||> Set.fold (fun acc rank ->
                    let peers = ctx.PeerMap |> Map.tryFind rank |> Option.defaultValue Set.empty
                    Set.union acc peers)
                |> fun allPeers -> Set.difference allPeers confirmed

            {
                state with
                    RealPoints = confirmed
                    FakePoints = Set.union state.FakePoints newFakes
            }
