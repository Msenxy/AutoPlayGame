namespace AutoPlayGame.Solver

open AutoPlayGame.Solver.Rules
open AutoPlayGame.Domain
open AutoPlayGame.Solver.Shared


module Solver =

    let buildContext grid = {
        Grid = grid
        PeerMap = Helpers.buildPeerMap grid
    }


    // 构建数据处理的初始状态
    let initialState = {
        RealPoints = Set.empty
        FakePoints = Set.empty
    }


    // 工具函数
    let private isComplete ctx state =
        state.RealPoints.Count >= ctx.Grid.Length

    let private hasChanged before after =
        after.RealPoints.Count > before.RealPoints.Count
        || after.FakePoints.Count > before.FakePoints.Count


    // 应用所有推理规则
    let private applyRules ctx state =
        state
        |> FindSingleColor.apply ctx
        |> FindSinglePoint.apply ctx
        |> FindColorConfinement.apply ctx
        |> FindNakedPair.apply ctx
        |> FindNakedTriple.apply ctx
        |> FindHiddenPair.apply ctx
        |> FindCrossConfinement.apply ctx


    // 寻找正确的点
    let rec solve ctx state =
        let state' = applyRules ctx state

        if isComplete ctx state' then
            printfn $"完成: %d{state'.RealPoints.Count}/%d{ctx.Grid.Length}"
            state'
        elif hasChanged state state' then
            printfn $"进度: Real=%d{state'.RealPoints.Count}/%d{ctx.Grid.Length}  Fake=%d{state'.FakePoints.Count}"
            solve ctx state'
        else
            printfn $"推理终止 (Real=%d{state'.RealPoints.Count}/%d{ctx.Grid.Length})"
            state'

    let execute ctx = solve ctx initialState
