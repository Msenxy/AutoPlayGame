module Domain.State

open Domain.Types


// 棋盘状态
type SolverState = {
    Grid: Cell[][]
    RealPoints: Set<{| Column: int; Row: int |}>
    FakePoints: Set<{| Column: int; Row: int |}>
}

// 构建数据处理的初始状态
let initialState grid = {
    Grid = grid
    RealPoints = Set.empty
    FakePoints = Set.empty
}
