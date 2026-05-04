namespace AutoPlayGame.Win32

open System
open AutoPlayGame.Domain

module Wrappers =

    // 转换
    let private toWindowRect (r: RECT) = {
        Left = r.Left
        Top = r.Top
        Right = r.Right
        Bottom = r.Bottom
    }


    // 获取窗口句柄
    let findWindow title =
        FindWindow(null, title)
        |> function
            | h when h = IntPtr.Zero -> Error "窗口未找到"
            | h -> Ok h


    // 如果窗口最小化则恢复
    let restoreIfMinimized hwnd =
        if IsIconic(hwnd) then
            ShowWindow(hwnd, WindowState.SwRestore) |> ignore

        hwnd


    // 获取窗口尺寸
    let getWindowRect hwnd =
        let mutable rect = RECT()

        match GetWindowRect(hwnd, &rect) with
        | true -> Ok(toWindowRect rect)
        | false -> Error "无法获取窗口尺寸"


    // 窗口获取流程
    let acquireWindow () =
        findWindow Config.WindowTitle
        |> Result.map restoreIfMinimized
        |> Result.bind getWindowRect
