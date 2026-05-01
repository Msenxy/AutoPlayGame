module Solver.Solve

open Domain.State
open Solver.Rules


// 工具函数
let isComplete state =
    state.RealPoints.Count >= state.Grid.Length

let hasChanged before after =
    after.RealPoints.Count > before.RealPoints.Count
    || after.FakePoints.Count > before.FakePoints.Count

// 寻找正确的点
let rec solve state =
    let state' = state |> findSingleColor |> findSinglePoint

    if isComplete state' then
        printfn $"完成: %d{state'.RealPoints.Count}/%d{state'.Grid.Length}"
        state'
    elif hasChanged state state' then
        printfn $"进度: Real=%d{state'.RealPoints.Count}/%d{state'.Grid.Length}  Fake=%d{state'.FakePoints.Count}"
        solve state'
    else
        printfn $"推理终止 (Real=%d{state.RealPoints.Count}/%d{state.Grid.Length})"
        state
