namespace AutoPlayGame.Win32

open System
open System.Runtime.InteropServices


[<StructLayout(LayoutKind.Sequential)>]
type RECT =
    struct
        val mutable Left: int
        val mutable Top: int
        val mutable Right: int
        val mutable Bottom: int
    end


// ShowWindow 命令常量
module WindowState =
    [<Literal>]
    let SwHide = 0

    [<Literal>]
    let SwShowNormal = 1

    [<Literal>]
    let SwShowMinimized = 2

    [<Literal>]
    let SwShowMaximized = 3

    [<Literal>]
    let SwShow = 5

    [<Literal>]
    let SwRestore = 9


[<AutoOpen>]
module private NativeApi =

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
