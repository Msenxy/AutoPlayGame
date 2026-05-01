module Imaging.BoardDetection

open Domain.Config
open Domain.Types
open OpenCvSharp


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
