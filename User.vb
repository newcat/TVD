Imports System.ComponentModel
Imports System.Web.Script.Serialization
Imports System.Net

Public Class User
    Implements System.ComponentModel.INotifyPropertyChanged

    Private _color As Color
    Private _name As String
    Private _isMod As Boolean
    Private _displayName As String
    Private _isFollower As Boolean
    Public updating As Boolean

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

    Public Property isFollower() As Boolean
        Get
            Return _isFollower
        End Get
        Set(ByVal value As Boolean)
            _isFollower = value
        End Set
    End Property

    Public Sub New(ByVal name As String, ByVal isMod As Boolean)

        _name = name
        _isMod = isMod

        updating = True

        Dim tc As New TwitchColors()
        tc.initializeColors()

        _color = tc.getColorByUsername(_name)

        tc.Dispose()

        If Not _name = "404" Then getDisplayName()

    End Sub

    Private Async Sub getDisplayName()

        Dim wr As New WebClient
        Dim json As String

        wr.Headers.Add("Client-ID", "3rxdy8dzlvpysys8fjfle0qqab77aqq")

        Try
            json = Await wr.DownloadStringTaskAsync("https://api.twitch.tv/kraken/users/" + _name)
        Catch e As Exception
            Return
        End Try

        Dim jsonSerializer As New JavaScriptSerializer
        Dim dict As Dictionary(Of String, Object) = jsonSerializer.Deserialize(Of Dictionary(Of String, Object))(json)

        displayName = CStr(dict("display_name"))

        Try
            json = Await wr.DownloadStringTaskAsync("https://api.twitch.tv/kraken/users/" + _name _
                + "/follows/channels/" + My.Settings.channel)
        Catch e As WebException
            If e.Message.Contains("404") Then
                isFollower = False
                updating = False
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("isFollower"))
            End If
            Return
        Catch e As Exception
            Return
        End Try

        dict = jsonSerializer.Deserialize(Of Dictionary(Of String, Object))(json)

        If dict.ContainsKey("channel") Then
            isFollower = True
            updating = False
        Else
            isFollower = False
            updating = False
        End If

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("isFollower"))

        wr.Dispose()

    End Sub

    Public Event PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs) Implements ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class
