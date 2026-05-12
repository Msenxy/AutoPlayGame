namespace AutoPlayGame.Domain


// 窗口尺寸
type WindowRect = {
    Left: int
    Top: int
    Right: int
    Bottom: int
}


// 截屏尺寸偏移量
type RoiModifier = {
    TopRatio: float
    LeftRatio: float
    WidthRatio: float
    HeightRatio: float
}


// 偏移后位置
type Roi = {
    Top: int
    Left: int
    Width: int
    Height: int
}


// 色块
[<Struct>]
type Point2D = { X: int; Y: int }

[<Struct>]
type Rank = { Column: int; Row: int }

[<Struct>]
type BgrColor = { B: byte; G: byte; R: byte }

type Cell = {
    Rank: Rank
    Point: Point2D
    Color: BgrColor
}


type SolverContext = {
    Grid: Cell[][]
    PeerMap: Map<Rank, Set<Rank>>
}


// 棋盘状态
type SolverState = {
    RealPoints: Set<Rank>
    FakePoints: Set<Rank>
}
