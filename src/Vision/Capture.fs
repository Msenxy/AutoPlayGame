namespace AutoPlayGame.Vision

open System.Drawing
open AutoPlayGame.Domain
open OpenCvSharp.Extensions


module Capture =

    // 获取偏移后的尺寸
    let getModifiedRect rect =
        let winW = rect.Right - rect.Left
        let winH = rect.Bottom - rect.Top

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
        { Src = BitmapConverter.ToMat(bmp) }
