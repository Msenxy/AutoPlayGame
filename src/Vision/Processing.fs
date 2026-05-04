namespace AutoPlayGame.Vision

open AutoPlayGame.Domain
open OpenCvSharp


module Processing =

    // 灰度处理
    let toGrayScale (ctx: Captured) =
        use gray = new Mat()
        Cv2.CvtColor(ctx.Src, gray, ColorConversionCodes.BGR2GRAY)

        { Src = ctx.Src; Gray = gray.Clone() }


    // 二值化处理
    let binarize ctx =
        use binary = new Mat()

        Cv2.Threshold(ctx.Gray, binary, Config.BinarizationThreshold, 255.0, ThresholdTypes.BinaryInv)
        |> ignore

        ctx.Gray.Dispose()

        {
            Src = ctx.Src
            Binary = binary.Clone()
        }


    // 提取轮廓
    let findExternalContours ctx =
        let mutable contours = Array.empty<Point[]>
        let mutable hierarchy = Array.empty<HierarchyIndex>

        Cv2.FindContours(
            ctx.Binary,
            &contours,
            &hierarchy,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple
        )

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


    // 分组
    let groupByColumn (ctx: Centered) =
        let columns =
            ctx.Centers
            |> Array.groupBy (fun p -> p.X / Config.ColumnGroupThreshold)
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
        let grid = [|
            for row in 0 .. ctx.Columns.Length - 1 ->
                [|
                    for col in 0 .. ctx.Columns.Length - 1 ->
                        let p = ctx.Columns[col][row]

                        {
                            Rank = { Column = col + 1; Row = row + 1 }
                            Point = Conversions.toPoint2D p
                            Color = Conversions.toBgrColor (ctx.Src.At<Vec3b>(p.Y, p.X))
                        }
                |]
        |]

        ctx.Src.Dispose()
        grid
