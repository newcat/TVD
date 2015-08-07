Public Class cEvent

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

End Class
