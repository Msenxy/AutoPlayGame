module Program

open Domain.Config
open Domain.State
open Imaging.BoardDetection
open Imaging.Preprocess
open Infrastructure.Capture
open Infrastructure.Win32
open Solver.Solve


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
