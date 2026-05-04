namespace AutoPlayGame.Domain


// 配置
module Config =

    [<Literal>]
    let WindowTitle = "智商不够别点"

    [<Literal>]
    let ColumnGroupThreshold = 5

    [<Literal>]
    let BinarizationThreshold = 200.0

    let roiModifier: RoiModifier = {
        TopRatio = 0.297
        LeftRatio = 0.097
        WidthRatio = 0.807
        HeightRatio = 0.443
    }
