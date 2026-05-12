module Program

open System.Diagnostics
open AutoPlayGame.Vision
open AutoPlayGame.Win32
open AutoPlayGame.Solver


let run () =
    Wrappers.acquireWindow ()
    |> Result.bind Pipeline.processImage
    |> Result.map Solver.buildContext
    |> Result.map Solver.execute


[<EntryPoint>]
let main _ =
    let stopwatch = Stopwatch.StartNew()

    let exitCode =
        match run () with
        | Ok _ ->
            stopwatch.Stop()
            printfn $"总耗时: %.2f{stopwatch.Elapsed.TotalMilliseconds} ms"
            0
        | Error msg ->
            stopwatch.Stop()
            eprintfn $"错误: %s{msg}"
            printfn $"总耗时: %.2f{stopwatch.Elapsed.TotalMilliseconds} ms"
            1

    exitCode
