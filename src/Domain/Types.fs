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
type Point2D = { X: int; Y: int }
type Rank2D = { Column: int; Row: int }
type BgrColor = { B: byte; G: byte; R: byte }

type Cell = {
    Rank: Rank2D
    Point: Point2D
    Color: BgrColor
}


// 棋盘状态
type SolverState = {
    Grid: Cell[][]
    RealPoints: Set<Rank2D>
    FakePoints: Set<Rank2D>
}
