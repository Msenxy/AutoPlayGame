module Imaging.Preprocess

open Domain.Config
open Domain.Types
open OpenCvSharp


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

// 获取中心点
let extractCenters ctx =
    let centers =
        ctx.Contours
        |> Array.map (fun cnt ->
            let r = Cv2.BoundingRect(cnt)
            Point(r.X + r.Width / 2, r.Y + r.Height / 2))

    { Src = ctx.Src; Centers = centers }
