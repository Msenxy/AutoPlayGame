module Solver.Rules

open Domain.Types
open Domain.State
open Solver.GridHelpers


// 寻找只有一个颜色的点
let findSingleColor =
    fun state ->
        let newReals =
            unknowns state
            |> Array.groupBy _.Color
            |> Array.choose (fun (_, grid) -> if grid.Length = 1 then Some grid[0].Rank else None)
            |> Set.ofArray

        if Set.isEmpty newReals then
            state
        else
            let confirmed = Set.union state.RealPoints newReals

            let newFakes =
                (Set.empty, newReals)
                ||> Set.fold (fun acc r -> Set.union acc (peers state.Grid r))
                |> fun allPeers -> Set.difference allPeers confirmed

            {
                state with
                    RealPoints = confirmed
                    FakePoints = Set.union state.FakePoints newFakes
            }

// 寻找行列中的唯一点
let findSinglePoint state = state
