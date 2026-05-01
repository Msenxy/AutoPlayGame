module Domain.Config

open Domain.Types


// 配置
module Config =
    let windowTitle = "智商不够别点"

    let roiModifier: RoiModifier = {
        TopRatio = 0.297
        LeftRatio = 0.097
        WidthRatio = 0.807
        HeightRatio = 0.443
    }

    let columnGroupThreshold = 5
    let binarizationThreshold = 200.0
