module Infrastructure.Win32

open System
open System.Runtime.InteropServices
open Domain.Types


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
