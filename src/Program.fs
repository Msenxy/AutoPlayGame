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
module WindowShowStyle =
    let SW_HIDE = 0
    let SW_SHOWNORMAL = 1
    let SW_SHOWMINIMIZED = 2
    let SW_SHOWMAXIMIZED = 3
    let SW_SHOW = 5
    let SW_RESTORE = 9

// 截屏尺寸偏移量
type RoiModifier = {
    Top: float
    Left: float
    Width: float
    Height: float
}

// 偏移后位置
type Roi = {
    Top: int
    Left: int
    Width: int
    Height: int
}

// 色块
type GridPosition = { Column: int; Row: int }

type Cell = {
    Rank: GridPosition
    Point: Point
    Color: Vec3b
    HasCow: bool option
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


// ======================
// Config
// ======================

// 配置
module Config =
    let windowTitle = "智商不够别点"

    let roiModifier: RoiModifier = {
        Top = 0.297
        Left = 0.097
        Width = 0.807
        Height = 0.443
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
    match FindWindow(null, windowTitle) with
    | h when h = IntPtr.Zero -> Error "窗口没找到"
    | h -> Ok h


// 如果窗口最小化则恢复
let restoreIfMinimized hwnd =
    if IsIconic(hwnd) then
        ShowWindow(hwnd, WindowShowStyle.SW_RESTORE) |> ignore

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
        Top = rect.Top + int (float winH * Config.roiModifier.Top)
        Left = rect.Left + int (float winW * Config.roiModifier.Left)
        Width = int (float winW * Config.roiModifier.Width)
        Height = int (float winH * Config.roiModifier.Height)
    }

// 截屏
let captureScreen roi =
    use bmp = new Bitmap(roi.Width, roi.Height)
    use g = Graphics.FromImage(bmp)
    g.CopyFromScreen(roi.Left, roi.Top, 0, 0, bmp.Size)

    let mat = BitmapConverter.ToMat(bmp)

    { Src = mat }

// 灰度处理
let toGray (ctx: Captured) =
    use dst = new Mat()
    Cv2.CvtColor(ctx.Src, dst, ColorConversionCodes.BGR2GRAY)

    let gray = dst.Clone()

    { Src = ctx.Src; Gray = gray }


// 二值化处理
let threshold ctx =
    use dst = new Mat()

    Cv2.Threshold(ctx.Gray, dst, Config.binarizationThreshold, 255.0, ThresholdTypes.BinaryInv)
    |> ignore

    let binary = dst.Clone()

    ctx.Gray.Dispose()

    { Src = ctx.Src; Binary = binary }


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
    let grid =
        ctx.Columns
        |> Array.map (Array.sortBy _.Y)
        |> fun cols -> [|
            for row in 0 .. ctx.Columns.Length - 1 do
                [|
                    for col in 0 .. ctx.Columns.Length - 1 do
                        let p = cols[col][row]

                        {
                            Rank = { Column = col + 1; Row = row + 1 }
                            Point = p
                            Color = ctx.Src.At<Vec3b>(p.Y, p.X)
                            HasCow = None
                        }
                |]
        |]

    ctx.Src.Dispose()
    grid


// ======================
// 数据处理
// ======================

// 寻找单一点


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
    |> Result.map toGray
    |> Result.map threshold
    |> Result.map findExternalContours

    |> Result.map extractCenters
    |> Result.bind groupByColumn
    |> Result.map buildGrid

    |> printfn "%A"

    // >>=
    // >>-
    // <*>


    0
