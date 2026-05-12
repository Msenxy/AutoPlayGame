namespace AutoPlayGame.Domain


// 配置
module Config =

    [<Literal>]
    let WindowTitle = "智商不够别点"

    [<Literal>]
    let ColumnGroupThreshold = 5

    [<Literal>]
    let BinarizationThreshold = 200.0

    let roiModifier = {
        TopRatio = 0.3
        LeftRatio = 0.04
        WidthRatio = 0.92
        HeightRatio = 0.5
    }
