Option Strict On
Option Explicit On

Imports Microsoft.Expression.Shapes
Imports System.Collections.ObjectModel
Imports System.Windows.Media.Animation
Imports System.ComponentModel

Public Class followerNonfollowerGraph

    Private followerArc As Arc
    Private nonfollowerArc As Arc
    Private unknownArc As Arc
    Private infoPanel As Border
    Private lblInfoPanel As Label

    Private WithEvents viewerList As ObservableCollection(Of User)

    Private currentF As Integer
    Private currentU As Integer
    Private currentO As mouseEnterObject

    Private unknownCount As Integer
    Private followerCount As Integer

    Private isInfoPanelVisible As Boolean = False

    Public Enum mouseEnterObject As Integer
        UNKNOWN = 0
        FOLLOWER = 1
        NONFOLLOWER = 2
    End Enum

    Sub New(ByRef pFollowerArc As Arc,
            ByRef pNonfollowerArc As Arc,
            ByRef pUnknownArc As Arc,
            ByRef pInfoPanel As Border,
            ByRef pLblInfoPanel As Label,
            ByRef pViewerList As ObservableCollection(Of User))

        followerArc = pFollowerArc
        nonfollowerArc = pNonfollowerArc
        unknownArc = pUnknownArc
        viewerList = pViewerList
        infoPanel = pInfoPanel
        lblInfoPanel = pLblInfoPanel

        setPie(0, 0)

    End Sub

    Private Sub setPie(ByVal f As Integer, ByVal u As Integer)

        currentF = f
        currentU = u

        Dim followerAnim As New DoubleAnimation(f, TimeSpan.FromSeconds(0.5))
        followerAnim.AccelerationRatio = 0.4
        followerAnim.DecelerationRatio = 0.4

        Dim nonfollowerAnimSA As New DoubleAnimation(f, TimeSpan.FromSeconds(0.5))
        nonfollowerAnimSA.AccelerationRatio = 0.4
        nonfollowerAnimSA.DecelerationRatio = 0.4
        Dim nonfollowerAnimEA As New DoubleAnimation(u, TimeSpan.FromSeconds(0.5))
        nonfollowerAnimEA.AccelerationRatio = 0.4
        nonfollowerAnimEA.DecelerationRatio = 0.4

        Dim unknownAnim As New DoubleAnimation(u, TimeSpan.FromSeconds(0.5))
        unknownAnim.AccelerationRatio = 0.4
        unknownAnim.DecelerationRatio = 0.4

        followerArc.BeginAnimation(Arc.EndAngleProperty, followerAnim)
        nonfollowerArc.BeginAnimation(Arc.StartAngleProperty, nonfollowerAnimSA)
        nonfollowerArc.BeginAnimation(Arc.EndAngleProperty, nonfollowerAnimEA)
        unknownArc.BeginAnimation(Arc.StartAngleProperty, unknownAnim)

    End Sub

    Private Sub update()

        If viewerList.Count = 0 Then
            setPie(0, 0)
            Return
        End If

        followerCount = 0
        unknownCount = 0

        For Each viewer As User In viewerList

            If viewer.isFollower Then
                followerCount += 1
            ElseIf viewer.updating Then
                unknownCount += 1
            End If

        Next

        Dim fAngle As Integer = CInt((1.0 * followerCount / viewerList.Count) * 360)
        Dim uAngle As Integer = 360 - CInt((1.0 * unknownCount / viewerList.Count) * 360)

        setPie(fAngle, uAngle)

        If isInfoPanelVisible Then MouseEnter(currentO)

    End Sub

    Private Sub viewerList_CollectionChanged(ByVal sender As Object,
                       ByVal args As Specialized.NotifyCollectionChangedEventArgs) _
                       Handles viewerList.CollectionChanged

        If args.Action = Specialized.NotifyCollectionChangedAction.Add Then
            For Each user As Object In args.NewItems
                AddHandler CType(user, User).PropertyChanged, AddressOf User_PropertyChanged
            Next
        End If

        update()

    End Sub

    Private Sub User_PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)

        If e.PropertyName.Equals("isFollower") Then Windows.Application.Current.Dispatcher.Invoke(Sub() update())

    End Sub

    Public Sub MouseEnter(ByVal o As mouseEnterObject)

        Dim p As Point

        Select Case o
            Case mouseEnterObject.UNKNOWN
                p = calcPoint(currentU, 360)
                lblInfoPanel.Content = unknownCount.ToString() + " unknown"
                lblInfoPanel.Foreground = Brushes.White
            Case mouseEnterObject.FOLLOWER
                p = calcPoint(0, currentF)
                lblInfoPanel.Content = followerCount.ToString() + " followers"
                lblInfoPanel.Foreground = New SolidColorBrush(Color.FromRgb(0, 255, 0))
            Case mouseEnterObject.NONFOLLOWER
                p = calcPoint(currentF, currentU)
                lblInfoPanel.Content = (viewerList.Count - followerCount - unknownCount).ToString() + " non-followers"
                lblInfoPanel.Foreground = Brushes.Red
        End Select

        infoPanel.Margin = New Thickness(0, p.Y, p.X, 0)

        If Not isInfoPanelVisible Then

            Dim blendInAmin As New DoubleAnimation(1, TimeSpan.FromSeconds(0.5))
            infoPanel.BeginAnimation(UIElement.OpacityProperty, blendInAmin)

            isInfoPanelVisible = True

        End If

    End Sub

    Public Sub MouseLeave()

        If isInfoPanelVisible Then

            Dim blendOutAnim As New DoubleAnimation(0, TimeSpan.FromSeconds(0.5))
            infoPanel.BeginAnimation(UIElement.OpacityProperty, blendOutAnim)

            isInfoPanelVisible = False

        End If

    End Sub

    Private Function calcPoint(ByVal sa As Integer, ByVal ea As Integer) As Point

        Dim a As Integer = CInt(sa + (ea - sa) / 2)
        Dim x As Integer = -CInt(Math.Sin(toRad(a)) * 25)
        Dim y As Integer = 75 - CInt(Math.Cos(toRad(a)) * 25)
        Return New Point(x, y)

    End Function

    Private Function toRad(ByVal a As Integer) As Double
        Return (a / 180) * Math.PI
    End Function

End Class