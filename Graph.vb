Option Strict On
Option Explicit On

Public Class Graph

    Private maxY As Integer = 10
    Private viewerHistory As New List(Of ViewerHistoryStop)
    Dim iC As System.Globalization.CultureInfo = System.Globalization.CultureInfo.InvariantCulture
    Private vdWidth As Integer

    Private lbl0, lbl20, lbl40, lbl60, lbl80 As TextBlock
    Private linePath, fillPath As Path

    Public Sub New(ByRef _lbl0 As TextBlock, ByRef _lbl20 As TextBlock, ByRef _lbl40 As TextBlock, ByRef _lbl60 As TextBlock, ByRef _lbl80 As TextBlock,
                   ByRef _linePath As Path, ByRef _fillPath As Path, ByVal _vdWidth As Integer)

        lbl0 = _lbl0
        lbl20 = _lbl20
        lbl40 = _lbl40
        lbl60 = _lbl60
        lbl80 = _lbl80

        linePath = _linePath
        fillPath = _fillPath

        vdWidth = _vdWidth

    End Sub

    Public Async Function addStop(viewerCount As Integer) As Task

        viewerHistory.Add(New ViewerHistoryStop(viewerCount))
        If viewerCount > maxY Then maxY = viewerCount + CInt(Math.Round(viewerCount * 0.1))

        Await setPath()

    End Function

    Private Async Function setPath() As Task

        Dim pData As String = Await getPath()
        linePath.Data = System.Windows.Media.Geometry.Parse(pData)
        fillPath.Data = System.Windows.Media.Geometry.Parse("M20,100L" + pData.Substring(1) + "V100Z")

        Dim lbls As String() = calcLabels()
        setYAxisLbl(lbl0, lbls(0))
        setYAxisLbl(lbl20, lbls(1))
        setYAxisLbl(lbl40, lbls(2))
        setYAxisLbl(lbl60, lbls(3))
        setYAxisLbl(lbl80, lbls(4))

    End Function

    Private Sub setYAxisLbl(ByVal label As System.Windows.Controls.TextBlock, ByVal s As String)

        label.Text = s

        If s.Length = 1 Then
            Canvas.SetLeft(label, 9)
        ElseIf s.Length = 2 Then
            Canvas.SetLeft(label, 5)
        ElseIf s.Length = 3 Then
            Canvas.SetLeft(label, 3)
        Else
            Canvas.SetLeft(label, 0)
        End If

    End Sub

    Private Function getY(i As Integer) As String

        If i >= viewerHistory.ToArray.Length Then i = viewerHistory.ToArray.Length - 1
        If i < 0 Then i = 0

        If viewerHistory.ToArray.Length = 0 Then Return "100"

        Return (100 - (100 * (viewerHistory(i).count / maxY))).ToString(iC)

    End Function

    Private Async Function getPath() As Task(Of String)

        Dim pData As String = String.Format("M{0},{1}", 20, getY(0))

        Await Task.Run(Sub()
                           For x As Integer = 20 To vdWidth

                               pData += "L" + x.ToString + "," + getY(CInt(Math.Round(((x - 20) / (vdWidth - 20)) * (viewerHistory.ToArray.Length - 1))))

                           Next
                       End Sub)

        Return pData

    End Function

    Private Function calcLabels() As String()

        Dim stepSize As Double = maxY / 5
        Dim ret(5) As String

        For i As Integer = 1 To 4

            ret(i) = Math.Round(stepSize * (5 - i)).ToString()

        Next

        ret(0) = maxY.ToString()

        Return ret

    End Function

    Public Async Function setVdWidth(width As Integer) As Task

        vdWidth = width

        If viewerHistory.ToArray.Length = 0 Then Return

        Await setPath()

    End Function

    Public Function getViewerHistoryStopByX(ByVal x As Integer) As ViewerHistoryStop

        If viewerHistory.ToArray.Length = 0 Then Return New ViewerHistoryStop(0)

        Return viewerHistory(CInt(Math.Round(((x - 20) / (vdWidth - 20)) * (viewerHistory.ToArray.Length - 1))))

    End Function

    Public Function getYbyX(ByVal x As Integer) As Double

        Return Val(getY(CInt(Math.Round(((x - 20) / (vdWidth - 20)) * (viewerHistory.ToArray.Length - 1)))))

    End Function

    Public Sub reset()

        viewerHistory = New List(Of ViewerHistoryStop)
        maxY = 10

    End Sub

End Class

Public Class GraphData

    Public lblText(5) As String
    Public pathLine As String
    Public pahtBG As String

End Class

Public Class ViewerHistoryStop

    Public count As Integer
    Public timestamp As Date

    Public Sub New(ByVal vC As Integer)
        count = vC
        timestamp = Date.Now
    End Sub

End Class