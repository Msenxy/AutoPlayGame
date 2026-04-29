module Program

open System
open System.Drawing
open System.Runtime.InteropServices
open OpenCvSharp
open OpenCvSharp.Extensions


// ======================
// Domain
// ======================

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

// 棋盘状态
type SolverState = {
    Grid: Cell[][]
    RealPoints: Set<{| Column: int; Row: int |}>
    FakePoints: Set<{| Column: int; Row: int |}>
}

// ======================
// Config
// ======================

// 配置
module Config =
    let windowTitle = "智商不够别点"

    let roiModifier: RoiModifier = {
        TopRatio = 0.297
        LeftRatio = 0.097
        WidthRatio = 0.807
        HeightRatio = 0.443
    }

    let columnGroupThreshold = 5
    let binarizationThreshold = 200.0


// ======================
// Win32 API
// ======================

// 获取窗口句柄
[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr FindWindow(string lpClassName, string lpWindowName)

// 判断窗口是否最小化
[<DllImport("user32.dll", SetLastError = true)>]
extern bool IsIconic(IntPtr hwnd)

// 显示窗口
[<DllImport("user32.dll", SetLastError = true)>]
extern bool ShowWindow(IntPtr hwnd, int mCmdShow)

// 获取窗口尺寸
[<DllImport("user32.dll", SetLastError = true)>]
extern bool GetWindowRect(IntPtr hwnd, RECT& lpRect)


// ======================
// Win32 API 包装
// ======================

// 获取窗口句柄
let findWindow windowTitle =
    FindWindow(null, windowTitle)
    |> function
        | h when h = IntPtr.Zero -> Error "窗口没找到"
        | h -> Ok h


// 如果窗口最小化则恢复
let restoreIfMinimized hwnd =
    if IsIconic(hwnd) then
        ShowWindow(hwnd, WindowState.swRestore) |> ignore

    hwnd

// 获取窗口尺寸
let getWindowRect hwnd =
    let mutable rect = RECT()

    match GetWindowRect(hwnd, &rect) with
    | true -> Ok rect
    | false -> Error "无法获取窗口尺寸"


// ======================
// OpenCV
// ======================

// 获取偏移后的尺寸
let getModifiedRect (rect: RECT) =
    let winW, winH = rect.Right - rect.Left, rect.Bottom - rect.Top

    {
        Top = rect.Top + int (float winH * Config.roiModifier.TopRatio)
        Left = rect.Left + int (float winW * Config.roiModifier.LeftRatio)
        Width = int (float winW * Config.roiModifier.WidthRatio)
        Height = int (float winH * Config.roiModifier.HeightRatio)
    }

// 截屏
let captureScreen roi =
    use bmp = new Bitmap(roi.Width, roi.Height)
    use g = Graphics.FromImage(bmp)
    g.CopyFromScreen(roi.Left, roi.Top, 0, 0, bmp.Size)

    let mat = BitmapConverter.ToMat(bmp)

    { Src = mat }

// 灰度处理
let toGrayScale (ctx: Captured) =
    use gray = new Mat()
    Cv2.CvtColor(ctx.Src, gray, ColorConversionCodes.BGR2GRAY)

    let gray' = gray.Clone()

    { Src = ctx.Src; Gray = gray' }


// 二值化处理
let binarize ctx =
    use binary = new Mat()

    Cv2.Threshold(ctx.Gray, binary, Config.binarizationThreshold, 255.0, ThresholdTypes.BinaryInv)
    |> ignore

    ctx.Gray.Dispose()

    let binary' = binary.Clone()

    { Src = ctx.Src; Binary = binary' }


// 提取轮廓
let findExternalContours ctx =
    let mutable contours = Array.empty<Point[]>
    let mutable hierarchy = Array.empty<HierarchyIndex>

    Cv2.FindContours(ctx.Binary, &contours, &hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple)

    ctx.Binary.Dispose()

    { Src = ctx.Src; Contours = contours }


// ======================
// 数据预处理
// ======================

// 获取中心点
let extractCenters ctx =
    let centers =
        ctx.Contours
        |> Array.map (fun cnt ->
            let r = Cv2.BoundingRect(cnt)
            Point(r.X + r.Width / 2, r.Y + r.Height / 2))

    { Src = ctx.Src; Centers = centers }

// 分组
let groupByColumn (ctx: Centered) =
    let columns =
        ctx.Centers
        |> Array.groupBy (fun p -> p.X / Config.columnGroupThreshold)
        |> Array.map snd
        |> Array.map (Array.sortBy _.Y)
        |> Array.sortBy (fun col -> (col[0]).X)

    let colCount = columns.Length
    let isValid = columns |> Array.forall (fun col -> col.Length = colCount)

    if isValid then
        Ok {
            Src = ctx.Src
            Centers = ctx.Centers
            Columns = columns
        }
    else
        Error "行列匹配错误"


// 构建 Cell 结构
let buildGrid ctx =
    [|
        for row in 0 .. ctx.Columns.Length - 1 ->
            [|
                for col in 0 .. ctx.Columns.Length - 1 ->
                    let p = ctx.Columns[col][row]

                    {
                        Rank = {| Column = col + 1; Row = row + 1 |}
                        Point = p
                        Color = ctx.Src.At<Vec3b>(p.Y, p.X)
                    }
            |]
    |]
    |> fun grid ->
        ctx.Src.Dispose()
        grid

// 构建数据处理的初始状态
let initialState grid = {
    Grid = grid
    RealPoints = Set.empty
    FakePoints = Set.empty
}


// ======================
// 数据处理
// ======================

// 提取未知格
let unknowns state =
    state.Grid
    |> Array.collect id
    |> Array.filter (fun cell ->
        not (Set.contains cell.Rank state.RealPoints)
        && not (Set.contains cell.Rank state.FakePoints))

// 寻找同行同列以及邻居节点
let peers grid (rank: {| Column: int; Row: int |}) =
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

// 寻找行列中的单一点
// let findSinglePoint state = None

// 工具函数
let private isComplete state =
    state.RealPoints.Count >= state.Grid.Length

let private hasChanged before after =
    after.RealPoints.Count > before.RealPoints.Count
    || after.FakePoints.Count > before.FakePoints.Count

// 寻找正确的点
let rec solve state =
    let state' = state |> findSingleColor

    if isComplete state' then
        printfn $"完成: %d{state'.RealPoints.Count}/%d{state'.Grid.Length}"
        state'
    elif hasChanged state state' then
        printfn $"进度: Real=%d{state'.RealPoints.Count}/%d{state'.Grid.Length}  Fake=%d{state'.FakePoints.Count}"
        solve state'
    else
        printfn $"推理终止 (Real=%d{state.RealPoints.Count}/%d{state.Grid.Length})"
        state


// ======================
// 入口
// ======================

[<EntryPoint>]
let main _ =
    findWindow Config.windowTitle
    |> Result.map restoreIfMinimized
    |> Result.bind getWindowRect
    |> Result.map getModifiedRect

    |> Result.map captureScreen
    |> Result.map toGrayScale
    |> Result.map binarize
    |> Result.map findExternalContours

    |> Result.map extractCenters
    |> Result.bind groupByColumn
    |> Result.map buildGrid
    |> Result.map initialState

    |> Result.map solve

    |> ignore

    0
