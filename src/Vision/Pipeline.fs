namespace AutoPlayGame.Vision


module Pipeline =

    let processImage rect =
        rect
        |> Capture.getModifiedRect
        |> Capture.captureScreen
        |> Processing.toGrayScale
        |> Processing.binarize
        |> Processing.findExternalContours
        |> Processing.extractCenters
        |> Processing.groupByColumn
        |> Result.map Processing.buildGrid
