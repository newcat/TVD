'Option Strict On
Option Explicit On

Imports System.Windows.Threading
Imports System.Text.RegularExpressions
Imports System.Collections.ObjectModel
Imports System.Windows.Media.Animation
Imports System.Web.Script.Serialization
Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Class MainWindow

    Dim viewerCount As Integer = 0
    Dim suppressUpdate As Boolean = False

    Dim viewerList As New ObservableCollection(Of User)
    Dim listEvents As New ObservableCollection(Of cEvent)

    WithEvents backgroundWorker As New System.ComponentModel.BackgroundWorker
    WithEvents followerTimer As New DispatcherTimer
    WithEvents graphTimer As New DispatcherTimer

    Dim autoupdater As New autoupdater
    WithEvents ircclient As New ircclient
    Dim twitchColors As TwitchColors
    Dim spamProtector As New SpamProtector
    Dim graph As Graph

    Private Sub Window_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded

        If Not My.Application.Info.DirectoryPath.Contains("Debug") Then
            autoupdater.check()
        End If

        If My.Settings.nick = "" Then
            Dim settings As New WpfSettings
            settings.btnCancel.IsEnabled = False
            settings.ShowDialog()
        Else
            My.Settings.Reload()
        End If

        If My.Settings.nick = Nothing Then
            Me.Close()
            End
        End If

        twitchColors = New TwitchColors
        twitchColors.initializeColors()

        graph = New Graph(lbl0, lbl20, lbl40, lbl60, lbl80, LinePath, FillPath, ViewerDiagramm.ActualWidth)

        followerTimer.Interval = New System.TimeSpan(0, 0, 20)
        followerTimer.Start()

        graphTimer.Interval = New System.TimeSpan(0, 0, 1)
        graphTimer.Start()

        DataContext = listEvents

        addEventLineSub(cEvent.IRC, "Connecting to TwitchIRC")

        ircclient.connect()
        backgroundWorker.RunWorkerAsync()

        loadPhrasesHelperSub()

    End Sub

    Private Async Sub loadPhrasesHelperSub()

        Dim ellipseFillBrush As New RadialGradientBrush(Color.FromRgb(255, 230, 0), Color.FromRgb(220, 200, 0))
        spEllipse.Fill = ellipseFillBrush

        spLabel.Content = "Initializing SpamProtector..."
        spLabel.Foreground = New SolidColorBrush(Color.FromRgb(255, 232, 0))

        Await spamProtector.loadPhrases()

        If spamProtector.isInitialized Then

            ellipseFillBrush = New RadialGradientBrush(Color.FromRgb(102, 204, 102), Color.FromRgb(0, 170, 0))
            spEllipse.Fill = ellipseFillBrush

            spLabel.Content = "SpamProtector active"
            spLabel.Foreground = New SolidColorBrush(Color.FromRgb(70, 255, 0))

        Else

            ellipseFillBrush = New RadialGradientBrush(Color.FromRgb(204, 102, 102), Color.FromRgb(170, 0, 0))
            spEllipse.Fill = ellipseFillBrush

            spLabel.Content = "SpamProtector inactive"
            spLabel.Foreground = New SolidColorBrush(Color.FromRgb(255, 0, 0))

        End If


    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles followerTimer.Tick

        If backgroundWorker.IsBusy = False Then
            backgroundWorker.RunWorkerAsync()
        End If

    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles backgroundWorker.DoWork

        getFollowers()

    End Sub

    Sub getFollowers()

        Dim webClient As New System.Net.WebClient
        Dim result As String

        Try
            result = webClient.DownloadString("https://api.twitch.tv/kraken/channels/" & My.Settings.channel & "/follows")
        Catch ex As System.Net.WebException
            addEventLineSub(cEvent.err, "Couldn't connect to Twitch-API")
            Return
        Catch ex As Exception
            addEventLineSub(cEvent.err, ex.ToString())
            Return
        End Try

        Dim followerCount As String = Regex.Match(Regex.Match(result, """_total"":[0-9]*").Value, "[0-9]+").Value

        If followerCount <> "0," + Chr(34) Then
            setFollowersSub(CInt(Val(followerCount)))
        End If

    End Sub

    Private Delegate Sub addEventLine(ByVal eventType As Integer, ByVal text As String, ByVal username As String)
    Private Sub addEventLineSub(ByVal eventType As Integer, ByVal text As String, Optional ByVal username As String = "")
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.Invoke(New addEventLine(AddressOf addEventLineSub), eventType, text, username)
        Else
            addEvent(eventType, text, username)
        End If
    End Sub

    Private Delegate Sub setViewers(ByVal text As String)
    Private Sub setViewersSub(ByVal text As String)
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.Invoke(New setViewers(AddressOf setViewersSub), text)
        Else

            If vc.Content.ToString <> text.ToString Then

                Dim colorAnim As New ColorAnimation(Colors.Green, Colors.White, New System.Windows.Duration(TimeSpan.FromSeconds(2)))

                Dim storyboard As New Storyboard
                storyboard.Children.Add(colorAnim)
                storyboard.SetTargetName(colorAnim, vc.Name)
                storyboard.SetTargetProperty(colorAnim, New System.Windows.PropertyPath("(Label.Foreground).(SolidColorBrush.Color)"))

                storyboard.Begin(Me)

            End If

            vc.Content = text
            viewersLB.ItemsSource = viewerList

        End If
    End Sub

    Private Delegate Sub setFollowers(ByVal num As Integer)
    Private Sub setFollowersSub(ByVal num As Integer)
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.Invoke(New setFollowers(AddressOf setFollowersSub), num)
        Else

            If fc.Content.ToString <> num.ToString Then

                Dim colorAnim As New ColorAnimation(Colors.Red, Colors.White, New System.Windows.Duration(TimeSpan.FromSeconds(5)))

                Dim storyboard As New Storyboard
                storyboard.Children.Add(colorAnim)
                storyboard.SetTargetName(colorAnim, fc.Name)
                storyboard.SetTargetProperty(colorAnim, New System.Windows.PropertyPath("(Label.Foreground).(SolidColorBrush.Color)"))

                storyboard.Begin(Me)

            End If

            fc.Content = num
        End If
    End Sub

    Private Delegate Sub addUser(ByVal user As User)
    Private Sub addUserSub(ByVal user As User)
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.Invoke(New addUser(AddressOf addUserSub), user)
        Else

            If getUserByName(user.name).name = "404" Then
                viewerList.Add(user)
            End If

            If user.isMod Then
                viewerList.Move(viewerList.IndexOf(user), 0)
            End If

            viewersLB.ItemsSource = viewerList
        End If
    End Sub

    Private Delegate Sub removeUser(ByVal user As User)
    Private Sub removeUserSub(ByVal user As User)
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.Invoke(New removeUser(AddressOf removeUserSub), user)
        Else
            viewerList.Remove(user)
            viewersLB.ItemsSource = viewerList
        End If
    End Sub

    Private Sub addEvent(ByVal eventType As Integer, ByVal text As String, Optional ByVal username As String = "")

        Dim displayName As String = getUserByName(username).displayName
        Dim color As String = getUserByName(username).color.ToString

        If displayName = "404" Then
            displayName = username
            color = getColorByUsername(username)
        End If

        listEvents.Add(New cEvent With {.EventType = eventType, .text = text, .username = displayName, .color = color, .isMod = getUserByName(username).isMod, .eventID = listEvents.ToArray.Length + 1})

        eventList.ItemsSource = listEvents
        eventList.ScrollIntoView(eventList.Items.GetItemAt(eventList.Items.Count - 1))

    End Sub

#Region "IRChandles"
    Private Sub connectedToIRC() Handles ircclient.connected
        addEventLineSub(cEvent.IRC, "Connected to IRC")
    End Sub

    Private Sub connectionToIRCFailed() Handles ircclient.connectionError
        addEventLineSub(cEvent.err, "Connection failed")
        MsgBox("Connection failed!")
    End Sub

    Private Sub privmsgReceived(ByVal username As String, ByVal msg As String) Handles ircclient.privmsgReceived

        addEventLineSub(cEvent.Chat, msg, username)

        If spamProtector.checkMsg(msg) Then
            ircclient.send("/timeout " + username)
            addEventLineSub(cEvent.IRC, "Timed out " + username + " because of spam")
        End If

    End Sub

    Private Sub cmdmsgReceived(ByVal msg As String) Handles ircclient.cmdmsgReceived

        If My.Settings.debug = True Then
            addEventLineSub(cEvent.IRC, msg)
        End If

        If msg.Contains(My.Settings.nick) And msg.Contains("353") Then
            Dim viewers() As String = msg.Split(Chr(32))
            For i As Integer = 5 To viewers.Length - 1
                If i = 5 Then
                    If getUserByName(viewers(i).Substring(1)).name = "404" And viewerList.Item(0).name <> viewers(i).Substring(1) Then
                        addUserSub(New User(viewers(i).Substring(1), False))
                    End If
                Else
                    If getUserByName(viewers(i)).name = "404" And viewerList.Item(0).name <> viewers(i) Then
                        addUserSub(New User(viewers(i), False))
                    End If
                End If
            Next
            setViewersSub("Viewers: " & viewerList.ToArray.Length)
        End If

    End Sub

    Private Sub joined(ByVal username As String) Handles ircclient.join

        addUserSub(New User(username, False))

        If My.Settings.showJoinLeaveEvents Then addEventLineSub(cEvent.Join, "joined", username)

        setViewersSub(viewerList.ToArray.Length.ToString)

    End Sub

    Private Sub part(ByVal username As String) Handles ircclient.part

        If My.Settings.showJoinLeaveEvents Then addEventLineSub(cEvent.Part, "left", username)

        Try
            removeUserSub(getUserByName(username))
        Catch e As Exception
            addEventLineSub(cEvent.err, e.Message)
        End Try

        setViewersSub(viewerList.ToArray.Length.ToString)

    End Sub

    Public Sub addUserInit(ByVal username As String) Handles ircclient.initJoin

        addUserSub(New User(username, False))

    End Sub

    Public Sub finalizeInit() Handles ircclient.initCompleted

        setViewersSub(viewerList.ToArray.Length.ToString)

    End Sub

    Public Sub modeChanged(ByVal username As String, ByVal isMod As Boolean) Handles ircclient.modeChanged

        Try
            Dim user As User = viewerList.Item(viewerList.IndexOf(getUserByName(username)))
            removeUserSub(getUserByName(username))
            user.isMod = isMod
            addUserSub(user)

            If isMod = True Then
                addEventLineSub(cEvent.IRC, username + " is now moderator")
            Else
                addEventLineSub(cEvent.IRC, username + " is no longer moderator")
            End If

        Catch e As Exception
            'addEventLineSub(cEvent.err, "Error changing mode:" + e.Message)
        End Try

    End Sub
#End Region

    Private Sub settings(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles Image1.MouseDown
        Dim settings As New WpfSettings
        settings.ShowDialog()
        If settings.DialogResult Then

            viewerCount = 0
            vc.Content = "0"
            listEvents.Clear()
            viewerList.Clear()
            graph.reset()

            ircclient.disconnect()
            ircclient = New ircclient
            ircclient.connect()

        End If
    End Sub

    Private Sub sendRequest() Handles TextBox1.TextChanged
        If TextBox1.LineCount > 1 Then
            If TextBox1.Text.Replace(Environment.NewLine, "").Trim() = "" Then
                TextBox1.Text = ""
                Return
            End If
            ircclient.send(TextBox1.Text.Replace(Environment.NewLine, "").Trim())
            addEventLineSub(cEvent.Chat, TextBox1.Text.Replace(Environment.NewLine, "").Trim(), My.Settings.nick)
            TextBox1.Text = ""
        End If
    End Sub

    Private Sub Window_Closed(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Closed
        ircclient.disconnect()
        backgroundWorker.Dispose()
        followerTimer.Stop()
        graphTimer.Stop()
        Environment.Exit(0)
    End Sub

    Private Sub graphTimerTick() Handles graphTimer.Tick

        If Not suppressUpdate Then
            graph.addStop(CInt(Val(vc.Content)))
        Else
            suppressUpdate = False
        End If

    End Sub

    Private Sub viewersLB_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles viewersLB.SelectionChanged
        viewersLB.SelectedIndex = -1
    End Sub

    Private Sub eventList_SelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs) Handles eventList.SelectionChanged
        eventList.SelectedIndex = -1
    End Sub

    Private Async Sub Window_SizeChanged(ByVal sender As System.Object, ByVal e As System.Windows.SizeChangedEventArgs) Handles MyBase.SizeChanged

        suppressUpdate = True
        If Not IsNothing(graph) Then Await graph.setVdWidth(ViewerDiagramm.ActualWidth)

    End Sub

    Private Function getUserByName(ByVal name As String) As User

        For Each user As User In viewerList

            If user.name = name Then
                Return user
            End If

        Next

        Return New User("404", False)

    End Function

    Private Function getColorByUsername(ByVal name As String) As String

        Dim color As String
        Dim user As User = getUserByName(name)

        If user.name = "404" Then
            color = twitchColors.getColorByUsername(name).ToString()
        Else
            color = getUserByName(name).color.ToString
        End If

        Return color

    End Function

    Private Sub cm_Timeout(ByVal sender As Object, ByVal e As RoutedEventArgs)

        Dim username As String = getUsernameByContextMenu(CType(sender, System.Windows.Controls.MenuItem))

        ircclient.send("/timeout " + username)
        addEventLineSub(cEvent.IRC, "Timed out " + username)

    End Sub

    Private Sub cm_Mod(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim username As String = getUsernameByContextMenu(CType(sender, System.Windows.Controls.MenuItem))

        ircclient.send("/mod " + username)

    End Sub

    Private Sub cm_Unmod(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim username As String = getUsernameByContextMenu(CType(sender, System.Windows.Controls.MenuItem))

        ircclient.send("/unmod " + username)

    End Sub

    Private Function getUsernameByContextMenu(ByVal cm As System.Windows.Controls.MenuItem) As String
        Dim contextMenu As System.Windows.Controls.ContextMenu = CType(cm.Parent, System.Windows.Controls.ContextMenu)
        Dim lblUsername As System.Windows.Controls.TextBlock = CType(contextMenu.PlacementTarget, System.Windows.Controls.TextBlock)
        Return lblUsername.Text
    End Function

    Private Sub initCM(ByVal sender As System.Object, ByVal e As RoutedEventArgs)
        Dim cm As System.Windows.Controls.ContextMenu = CType(sender, System.Windows.Controls.ContextMenu)
        Dim lblUsername As System.Windows.Controls.TextBlock = CType(cm.PlacementTarget, System.Windows.Controls.TextBlock)
        Dim username As String = lblUsername.Text
        Dim user As User = getUserByName(username)
        Dim ircUser As User = getUserByName(My.Settings.nick)

        cm.Items(0).IsEnabled = ircUser.isMod
        cm.Items(2).IsEnabled = ircUser.isMod And Not user.isMod
        cm.Items(3).IsEnabled = ircUser.isMod And user.isMod

    End Sub

    Private Sub ViewerDiagramm_MouseLeave(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles ViewerDiagramm.MouseLeave

        Dim fadeOut As New DoubleAnimation(0, TimeSpan.FromSeconds(0.5))
        InfoPanel.BeginAnimation(OpacityProperty, fadeOut)
        InfoEllipse.BeginAnimation(OpacityProperty, fadeOut)

    End Sub

    Private Sub ViewerDiagramm_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles ViewerDiagramm.MouseMove

        If IsNothing(graph) Then Return

        Dim x As Integer = CInt(e.GetPosition(ViewerDiagramm).X)

        If x > ViewerDiagramm.ActualWidth Then Return

        Dim roundedX As Integer = CInt(x / 2)
        roundedX = roundedX * 2

        If x < 20 Then
            If InfoPanel.Opacity = 1 Then
                Dim fadeOut As New DoubleAnimation(0, TimeSpan.FromSeconds(0.5))
                InfoPanel.BeginAnimation(OpacityProperty, fadeOut)
                InfoEllipse.BeginAnimation(OpacityProperty, fadeOut)
            End If
        Else
            If InfoPanel.Opacity = 0 Then
                Dim fadeIn As New DoubleAnimation(1, TimeSpan.FromSeconds(0.5))
                InfoPanel.BeginAnimation(OpacityProperty, fadeIn)
                InfoEllipse.BeginAnimation(OpacityProperty, fadeIn)
            End If

            If x <= (ViewerDiagramm.ActualWidth - 50) And x >= 50 Then
                Canvas.SetLeft(InfoPanel, x - 50)
            ElseIf x > (ViewerDiagramm.ActualWidth - 50) Then
                Canvas.SetLeft(InfoPanel, (ViewerDiagramm.ActualWidth - 100))
            ElseIf x < 50 Then
                Canvas.SetLeft(InfoPanel, 0)
            End If

            Dim y As Integer = graph.getYbyX(x)

            If y > 60 Then
                Canvas.SetTop(InfoPanel, y - 60)
            Else
                Canvas.SetTop(InfoPanel, y + 10)
            End If

            Dim vhs As ViewerHistoryStop = graph.getViewerHistoryStopByX(x)

            lblViewerInfoPanel.Content = vhs.count
            lblTimeInfoPanel.Content = vhs.timestamp.ToString("HH:mm:ss")
            Canvas.SetLeft(InfoEllipse, roundedX - 2)
            Canvas.SetTop(InfoEllipse, y - 2)

        End If

    End Sub

    Private Sub spEllipse_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles spEllipse.MouseDown

        Dim ellipseFillBrush As RadialGradientBrush

        If spamProtector.isActive Then
            spamProtector.isActive = False

            ellipseFillBrush = New RadialGradientBrush(Color.FromRgb(204, 102, 102), Color.FromRgb(170, 0, 0))
            spEllipse.Fill = ellipseFillBrush

            spLabel.Content = "SpamProtector inactive"
            spLabel.Foreground = New SolidColorBrush(Color.FromRgb(255, 0, 0))

        Else

            If Not spamProtector.isInitialized Then
                loadPhrasesHelperSub()
            Else

                spamProtector.isActive = True

                ellipseFillBrush = New RadialGradientBrush(Color.FromRgb(102, 204, 102), Color.FromRgb(0, 170, 0))
                spEllipse.Fill = ellipseFillBrush

                spLabel.Content = "SpamProtector active"
                spLabel.Foreground = New SolidColorBrush(Color.FromRgb(70, 255, 0))

            End If
        End If

    End Sub

End Class

Public Structure cEvent

    Public Const err As Integer = 0
    Public Const IRC As Integer = 1
    Public Const Join As Integer = 2
    Public Const Part As Integer = 3
    Public Const Chat As Integer = 4

    Private _EventType As Integer
    Private _username As String
    Private _text As String
    Private _color As String
    Private _isMod As Boolean
    Private _eventID As UInteger

    Property EventType() As Integer
        Get
            Return _EventType
        End Get
        Set(ByVal value As Integer)
            _EventType = value
        End Set
    End Property

    Property username() As String
        Get
            Return _username
        End Get
        Set(ByVal value As String)
            _username = value
        End Set
    End Property

    Property text() As String
        Get
            Return _text
        End Get
        Set(ByVal value As String)
            _text = value
        End Set
    End Property

    Property color() As String
        Get
            Return _color
        End Get
        Set(ByVal value As String)
            _color = value
        End Set
    End Property

    Property isMod() As Boolean
        Get
            Return _isMod
        End Get
        Set(ByVal value As Boolean)
            _isMod = value
        End Set
    End Property

    Property eventID() As UInteger
        Get
            Return _eventID
        End Get
        Set(value As UInteger)
            _eventID = value
        End Set
    End Property

End Structure

Public Class User
    Implements System.ComponentModel.INotifyPropertyChanged

    Private _color As Color
    Private _name As String
    Private _isMod As Boolean
    Private _displayName As String
    Public needsUpdate As Boolean

    Property color() As Color
        Get
            Return _color
        End Get
        Set(ByVal value As Color)
            _color = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("color"))
        End Set
    End Property

    Property name() As String
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("name"))
        End Set
    End Property

    Property isMod() As Boolean
        Get
            Return _isMod
        End Get
        Set(ByVal value As Boolean)
            _isMod = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("isMod"))
        End Set
    End Property

    Property displayName() As String
        Get
            If Not IsNothing(_displayName) Then
                Return _displayName
            Else
                Return _name
            End If
        End Get
        Set(value As String)
            _displayName = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("displayName"))
        End Set
    End Property

    Public Sub New(ByVal name As String, ByVal isMod As Boolean)

        _name = name
        _isMod = isMod

        Dim tc As New TwitchColors()
        tc.initializeColors()

        _color = tc.getColorByUsername(_name)

        tc.Dispose()

        If Not _name = "404" Then getDisplayName()

    End Sub

    Private Async Sub getDisplayName()

        Dim wr As New System.Net.WebClient
        Dim json As String

        json = Await wr.DownloadStringTaskAsync("https://api.twitch.tv/kraken/users/" + _name)

        Dim jsonSerializer As New JavaScriptSerializer
        Dim dict As Dictionary(Of String, Object) = jsonSerializer.Deserialize(Of Dictionary(Of String, Object))(json)

        displayName = CStr(dict("display_name"))

    End Sub

    Public Event PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs) Implements ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class