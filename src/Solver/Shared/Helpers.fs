namespace AutoPlayGame.Solver.Shared

open AutoPlayGame.Domain


module Helpers =

    // 提取未知格
    let unknowns grid state =
        grid
        |> Array.collect id
        |> Array.filter (fun cell ->
            not (Set.contains cell.Rank state.RealPoints)
            && not (Set.contains cell.Rank state.FakePoints))


    // 寻找同行同列以及邻居节点
    let private computePeers grid rank =
        grid
        |> Array.collect id
        |> Array.choose (fun cell ->
            let sameRow = cell.Rank.Row = rank.Row
            let sameColumn = cell.Rank.Column = rank.Column

            let adjacent =
                abs (cell.Rank.Row - rank.Row) <= 1 && abs (cell.Rank.Column - rank.Column) <= 1

            let isSelf = cell.Rank = rank

            if (sameRow || sameColumn || adjacent || isSelf) && not isSelf then
                Some cell.Rank
            else
                None)
        |> Set.ofArray


    let buildPeerMap grid =
        grid
        |> Array.collect id
        |> Array.map (fun cell -> cell.Rank, computePeers grid cell.Rank)
        |> Map.ofArray
