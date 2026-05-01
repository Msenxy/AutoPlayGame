module Domain.Types

open System.Runtime.InteropServices
open OpenCvSharp


// 窗口尺寸
[<StructLayout(LayoutKind.Sequential)>]
type RECT =
    struct
        val mutable Left: int
        val mutable Top: int
        val mutable Right: int
        val mutable Bottom: int
    end

// 窗口的显示状态
module WindowState =
    [<Literal>]
    let swHide = 0

    [<Literal>]
    let swShownormal = 1

    [<Literal>]
    let swShowminimized = 2

    [<Literal>]
    let swShowmaximized = 3

    [<Literal>]
    let swShow = 5

    [<Literal>]
    let swRestore = 9

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
type Cell = {
    Rank: {| Column: int; Row: int |}
    Point: Point
    Color: Vec3b
}

// 管道上下文
type Captured = { Src: Mat }
type Grayed = { Src: Mat; Gray: Mat }
type Thresholded = { Src: Mat; Binary: Mat }
type Contoured = { Src: Mat; Contours: Point[][] }
type Centered = { Src: Mat; Centers: Point[] }

type Grouped = {
    Src: Mat
    Centers: Point[]
    Columns: Point[][]
}
