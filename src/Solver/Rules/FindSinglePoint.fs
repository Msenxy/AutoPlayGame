namespace AutoPlayGame.Solver.Rules

open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module FindSinglePoint =

    // 寻找行列中的唯一点
    let apply state =
        let newRealsFromRows =
            Helpers.unknowns state
            |> Array.groupBy (fun cell -> cell.Rank.Row)
            |> Array.choose (fun (_, cells) -> if cells.Length = 1 then Some cells[0].Rank else None)

        let newRealsFromColumns =
            Helpers.unknowns state
            |> Array.groupBy (fun cell -> cell.Rank.Column)
            |> Array.choose (fun (_, cells) -> if cells.Length = 1 then Some cells[0].Rank else None)

        let newReals = Array.append newRealsFromRows newRealsFromColumns |> Set.ofArray

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
