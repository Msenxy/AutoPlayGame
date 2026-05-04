namespace AutoPlayGame.Solver.Rules

open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module FindSingleColor =

    let apply state =
        let newReals =
            Helpers.unknowns state
            |> Array.groupBy _.Color
            |> Array.choose (fun (_, cells) -> if cells.Length = 1 then Some cells[0].Rank else None)
            |> Set.ofArray

        if Set.isEmpty newReals then
            state
        else
            let confirmed = Set.union state.RealPoints newReals

            let newFakes =
                (Set.empty, newReals)
                ||> Set.fold (fun acc rank -> Set.union acc (Helpers.peers state.Grid rank))
                |> fun allPeers -> Set.difference allPeers confirmed

            {
                state with
                    RealPoints = confirmed
                    FakePoints = Set.union state.FakePoints newFakes
            }
