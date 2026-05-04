namespace AutoPlayGame.Vision

open OpenCvSharp


// 管道上下文
type Captured = { Src: Mat }
type Grayed = { Src: Mat; Gray: Mat }
type Thresholded = { Src: Mat; Binary: Mat }
type Contoured = { Src: Mat; Contours: Point[][] }
type Centered = { Src: Mat; Centers: Point[] }

type Grouped = {
    Src: Mat
    Centers: Point[]
    Columns: Point[][]
}
