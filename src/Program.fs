module Program

open AutoPlayGame.Vision
open AutoPlayGame.Win32
open AutoPlayGame.Solver


let run () =
    Wrappers.acquireWindow ()
    |> Result.bind Pipeline.processImage
    |> Result.map Solver.initialState
    |> Result.map Solver.solve


// Conductor
[<EntryPoint>]
let main _ =
    match run () with
    | Ok _ -> 0
    | Error msg ->
        eprintfn $"错误: %s{msg}"
        1
