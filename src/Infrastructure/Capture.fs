module Infrastructure.Capture

open System.Drawing
open Domain.Config
open Domain.Types
open OpenCvSharp.Extensions


// 获取偏移后的尺寸
let getModifiedRect (rect: RECT) =
    let winW, winH = rect.Right - rect.Left, rect.Bottom - rect.Top

    {
        Top = rect.Top + int (float winH * Config.roiModifier.TopRatio)
        Left = rect.Left + int (float winW * Config.roiModifier.LeftRatio)
        Width = int (float winW * Config.roiModifier.WidthRatio)
        Height = int (float winH * Config.roiModifier.HeightRatio)
    }

// 截屏
let captureScreen roi =
    use bmp = new Bitmap(roi.Width, roi.Height)
    use g = Graphics.FromImage(bmp)
    g.CopyFromScreen(roi.Left, roi.Top, 0, 0, bmp.Size)

    let mat = BitmapConverter.ToMat(bmp)

    { Src = mat }
