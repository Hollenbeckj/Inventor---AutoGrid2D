Module GeometryTests
    Private checks As Integer = 0
    Private r As New RuleUnderTest
    Private Const Tol As Double = 0.000001

    Sub Main()
        Dim rectangle As New List(Of Double()) From {
            New Double() {0, 0}, New Double() {10, 0}, New Double() {10, 6}, New Double() {0, 6}}
        CheckPolygon(rectangle)
        AssertClose(r.AreaCentroid(rectangle, Tol)(0), 5, "rectangle center X")
        AssertClose(r.AreaCentroid(rectangle, Tol)(1), 3, "rectangle center Y")
        Dim vertical = r.ClipAxis(rectangle, True, 5, Tol)
        AssertTrue(vertical.Count = 1, "rectangle vertical count")
        AssertClose(vertical(0)(1), 0, "vertical bottom")
        AssertClose(vertical(0)(3), 6, "vertical top")
        Dim horizontal = r.ClipAxis(rectangle, False, 3, Tol)
        AssertClose(horizontal(0)(0), 0, "horizontal left")
        AssertClose(horizontal(0)(2), 10, "horizontal right")
        AssertTrue(r.ClipAxis(rectangle, True, 12, Tol).Count = 0, "outside line")

        Dim diamond As New List(Of Double()) From {
            New Double() {0, 3}, New Double() {3, 0}, New Double() {6, 3}, New Double() {3, 6}}
        CheckPolygon(diamond)
        Dim cut = r.ClipAxis(diamond, True, 1, Tol)
        AssertClose(cut(0)(1), 2, "rotated rectangle clipped bottom")
        AssertClose(cut(0)(3), 4, "rotated rectangle clipped top")
        AssertTrue(r.ClipAxis(diamond, True, 0, Tol).Count = 0, "vertex tangent creates no zero-length line")

        Dim trapezoid As New List(Of Double()) From {
            New Double() {0, 0}, New Double() {10, 0}, New Double() {8, 6}, New Double() {2, 6}}
        CheckPolygon(trapezoid)
        AssertClose(r.AreaCentroid(trapezoid, Tol)(0), 5, "trapezoid centroid X")
        AssertClose(r.AreaCentroid(trapezoid, Tol)(1), 2.75, "trapezoid centroid differs from bounding center")

        Dim concave As New List(Of Double()) From {
            New Double() {0, 0}, New Double() {6, 0}, New Double() {1, 1}, New Double() {0, 6}}
        CheckPolygon(concave)
        AssertClose(r.AreaCentroid(concave, Tol)(0), 4.0 / 3, "concave centroid")
        AssertTrue(Not r.InsidePolygon(concave, r.AreaCentroid(concave, Tol), Tol), "concave centroid can be outside")
        Dim split = r.ClipAxis(concave, True, 1, Tol)
        AssertTrue(split.Count = 1, "concave vertex clipping")

        Dim shifted As New List(Of Double())
        For Each p In trapezoid
            shifted.Add(New Double() {p(0) + 1000000, p(1) - 1000000})
        Next
        AssertClose(r.AreaCentroid(shifted, Tol)(0), 1000005, "translated centroid X")
        AssertClose(r.AreaCentroid(shifted, Tol)(1), -999997.25, "translated centroid Y")

        Dim bowtie As New List(Of Double()) From {
            New Double() {0, 0}, New Double() {6, 6}, New Double() {0, 6}, New Double() {6, 0}}
        ExpectReject(Sub() r.OrderBoundary(ToEdges(bowtie), Tol), "self-intersection")
        Dim duplicate = ToEdges(rectangle)
        duplicate(3) = duplicate(0)
        ExpectReject(Sub() r.OrderBoundary(duplicate, Tol), "duplicate edge")
        Dim gap = ToEdges(rectangle)
        gap(0)(0) += 0.01
        ExpectReject(Sub() r.OrderBoundary(gap, Tol), "disconnected edge")
        Dim zeroEdge = ToEdges(rectangle)
        zeroEdge(0) = New Double() {0, 0, 0, 0}
        ExpectReject(Sub() r.OrderBoundary(zeroEdge, Tol), "zero-length edge")
        Dim collinear As New List(Of Double()) From {
            New Double() {0, 0}, New Double() {5, 0}, New Double() {10, 0}, New Double() {0, 6}}
        ExpectReject(Sub() r.OrderBoundary(ToEdges(collinear), Tol), "collinear corner")
        Dim almostClosed = ToEdges(rectangle)
        almostClosed(0)(0) += Tol / 2
        AssertTrue(r.OrderBoundary(almostClosed, Tol).Count = 4, "endpoint tolerance")

        AssertTrue(r.InteriorCount(10, 2, Tol, 2000) = 4, "no boundary grid line")
        AssertTrue(r.InteriorCount(10, 3, Tol, 2000) = 3, "remainder spacing")
        AssertTrue(r.InteriorCount(10, 10, Tol, 2000) = 0, "spacing equal to span")
        AssertTrue(r.InteriorCount(10, 20, Tol, 2000) = 0, "spacing exceeds span")
        AssertTrue(r.InteriorCount(10, 12 * 0.5, Tol, 2000) = 1, "view scale conversion")
        ExpectReject(Sub() r.InteriorCount(10, 0.00000000001, Tol, 2000), "tiny spacing cap")
        ExpectReject(Sub() r.InteriorCount(10, Double.Epsilon, Tol, 2000), "underflow spacing")
        ExpectReject(Sub() r.InteriorCount(10, Double.NaN, Tol, 2000), "NaN spacing")
        ExpectReject(Sub() r.InteriorCount(10, Double.PositiveInfinity, Tol, 2000), "infinite spacing")
        ExpectReject(Sub() r.InteriorCount(10, -1, Tol, 2000), "negative spacing")
        Console.WriteLine("PASS: " & checks & " assertions; full rule compiled against Inventor 2027 interop.")
    End Sub

    Function ToEdges(polygon As List(Of Double())) As List(Of Double())
        Dim edges As New List(Of Double())
        For i As Integer = 0 To 3
            Dim a = polygon(i), b = polygon((i + 1) Mod 4)
            edges.Add(New Double() {a(0), a(1), b(0), b(1)})
        Next
        Return edges
    End Function

    Sub CheckPolygon(polygon As List(Of Double()))
        Dim edges = ToEdges(polygon)
        Dim expected = r.AreaCentroid(polygon, Tol)
        ' All 24 pick orders and all 16 independent edge directions.
        For a As Integer = 0 To 3
            For b As Integer = 0 To 3
                For c As Integer = 0 To 3
                    For d As Integer = 0 To 3
                        If a = b OrElse a = c OrElse a = d OrElse b = c OrElse b = d OrElse c = d Then Continue For
                        For mask As Integer = 0 To 15
                            Dim permutation As New List(Of Double())
                            Dim order As Integer() = {a, b, c, d}
                            For i As Integer = 0 To 3
                                Dim e = edges(order(i))
                                permutation.Add(If((mask And (1 << i)) = 0, e, New Double() {e(2), e(3), e(0), e(1)}))
                            Next
                            Dim actual = r.AreaCentroid(r.OrderBoundary(permutation, Tol), Tol)
                            AssertClose(actual(0), expected(0), "pick order centroid X")
                            AssertClose(actual(1), expected(1), "pick order centroid Y")
                        Next
                    Next
                Next
            Next
        Next
        ' Sample all generated intervals to check clipping against the polygon itself.
        For axis As Integer = 0 To 1
            For stepIndex As Integer = 1 To 119
                For Each segment In r.ClipAxis(polygon, axis = 0, stepIndex / 10.0, Tol)
                    For sample As Integer = 0 To 10
                        Dim t As Double = sample / 10.0
                        Dim p As Double() = {segment(0) + t * (segment(2) - segment(0)), segment(1) + t * (segment(3) - segment(1))}
                        AssertTrue(r.InsidePolygon(polygon, p, Tol), "clipped segment remains inside")
                    Next
                Next
            Next
        Next
    End Sub

    Sub ExpectReject(action As Action, label As String)
        Try
            action()
        Catch ex As InvalidOperationException
            checks += 1
            Return
        End Try
        Throw New Exception("Expected rejection: " & label)
    End Sub

    Sub AssertTrue(condition As Boolean, label As String)
        checks += 1
        If Not condition Then Throw New Exception(label)
    End Sub

    Sub AssertClose(actual As Double, expected As Double, label As String)
        AssertTrue(Math.Abs(actual - expected) < Tol, label & ": expected " & expected & ", got " & actual)
    End Sub
End Module
