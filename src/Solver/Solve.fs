namespace AutoPlayGame.Solver

open AutoPlayGame.Solver.Rules
open AutoPlayGame.Domain


module Solver =
    // 构建数据处理的初始状态
    let initialState grid = {
        Grid = grid
        RealPoints = Set.empty
        FakePoints = Set.empty
    }


    // 工具函数
    let private isComplete state =
        state.RealPoints.Count >= state.Grid.Length

    let private hasChanged before after =
        after.RealPoints.Count > before.RealPoints.Count
        || after.FakePoints.Count > before.FakePoints.Count


    // 应用所有推理规则
    let private applyRules state =
        state |> FindSingleColor.apply |> FindSinglePoint.apply


    // 寻找正确的点
    let rec solve state =
        let state' = applyRules state

        if isComplete state' then
            printfn $"完成: %d{state'.RealPoints.Count}/%d{state'.Grid.Length}"
            state'
        elif hasChanged state state' then
            printfn $"进度: Real=%d{state'.RealPoints.Count}/%d{state'.Grid.Length}  Fake=%d{state'.FakePoints.Count}"
            solve state'
        else
            printfn $"推理终止 (Real=%d{state.RealPoints.Count}/%d{state'.Grid.Length})"
            state'
