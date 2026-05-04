namespace AutoPlayGame.Vision

open OpenCvSharp
open AutoPlayGame.Domain


module Conversions =

    let toPoint2D (p: Point) = { X = p.X; Y = p.Y }


    let toBgrColor (v: Vec3b) = {
        B = v.Item0
        G = v.Item1
        R = v.Item2
    }
