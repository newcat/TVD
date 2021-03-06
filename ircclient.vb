﻿Option Strict On
Option Explicit On

Imports System.IO
Imports System.Threading
Imports System.Net.Sockets
Imports System.Net

Public Class ircclient
    Implements IDisposable

    Dim tcps As NetworkStream
    Dim tcpw As StreamWriter
    Dim tcpr As StreamReader
    Dim tcpc As New TcpClient

    Dim ip As String = "irc.twitch.tv"
    Dim port As Integer = 6667

    Dim receiveThread As New Thread(AddressOf client_receive)

    Public Event rawmsgReceived(ByVal msg As String)
    Public Event privmsgReceived(ByVal username As String, ByVal msg As String)
    Public Event cmdmsgReceived(ByVal msg As String)
    Public Event connected()
    Public Event connectionError()
    Public Event join(ByVal username As String)
    Public Event part(ByVal username As String)
    Public Event initCompleted()
    Public Event initJoin(ByVal username As String)
    Public Event modeChanged(ByVal username As String, ByVal isMod As Boolean)


    Public Sub send(ByVal msg As String)

        If (msg.StartsWith("%"c)) Then

            client_send(msg.Trim("%"c))

        Else

            client_send("PRIVMSG #" & My.Settings.channel & " :" & msg)

        End If

    End Sub


    Public Async Function connect() As Task

        Try
            Await tcpc.ConnectAsync(ip, port)
        Catch e As SocketException
            RaiseEvent connectionError()
            Return
        End Try

        If tcpc.Connected Then

            RaiseEvent connected()

            tcps = tcpc.GetStream()
            tcpw = New StreamWriter(tcps)
            tcpr = New StreamReader(tcps)

            receiveThread.Start()

        Else

            RaiseEvent connectionError()
            Return

        End If

        client_send("PASS " & My.Settings.oauth)
        client_send("NICK " & My.Settings.nick)

    End Function

    Private Sub client_send(ByVal text As String)
        If tcpc.Connected Then
            tcpw.WriteLine(text)
            tcpw.Flush()
        End If
    End Sub

    Private Sub client_receive()
        While True
            Dim client_receive As String
            client_receive = tcpr.ReadLine

            RaiseEvent rawmsgReceived(client_receive)

            'analyze the received message and put it into a more
            'userfriendly format or answer automatically

            If client_receive.Contains("PING") Then
                client_send("PONG tmi.twitch.tv")
            ElseIf client_receive.Contains("NOTICE") Then
                RaiseEvent cmdmsgReceived(client_receive)
                MsgBox("Login not successfull! Please check your settings")
                disconnect()
            ElseIf client_receive.Contains("JOIN") Then
                RaiseEvent join(getNickname(client_receive))
            ElseIf client_receive.Contains("PRIVMSG") And Not client_receive.Contains("jtv") Then
                RaiseEvent privmsgReceived(getNickname(client_receive), getMessage(client_receive))
            ElseIf client_receive.Contains("PART") Then
                RaiseEvent part(getNickname(client_receive))
            ElseIf client_receive.Contains("376 " + My.Settings.nick.ToLower) Then
                RaiseEvent cmdmsgReceived(client_receive)
                client_send("CAP REQ :twitch.tv/membership")
            ElseIf client_receive.Contains("CAP * ACK :twitch.tv/membership") Then
                RaiseEvent cmdmsgReceived(client_receive)
                client_send("JOIN #" & My.Settings.channel)
            ElseIf client_receive.Contains("353") Then
                RaiseEvent cmdmsgReceived(client_receive)
                getConnectedUsers(client_receive)
            ElseIf client_receive.Contains("366") Then
                RaiseEvent initCompleted()
                RaiseEvent cmdmsgReceived(client_receive)
            ElseIf client_receive.Contains("MODE #" + My.Settings.channel) Then
                RaiseEvent modeChanged(getModNick(client_receive), getModStatus(client_receive))
                RaiseEvent cmdmsgReceived(client_receive)
            Else
                RaiseEvent cmdmsgReceived(client_receive)
            End If
        End While
    End Sub

    Private Function getModNick(ByVal msg As String) As String

        Return msg.Split(Chr(32))(4)

    End Function

    Private Function getModStatus(ByVal msg As String) As Boolean

        If msg.Contains("+o") Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function getNickname(ByVal msg As String) As String

        getNickname = msg.Split("!"c)(0).Trim(":"c)

    End Function

    Private Function getMessage(ByVal msg As String) As String

        Dim cI As Integer = msg.IndexOf(":", 3)

        getMessage = msg.Substring(cI + 1, msg.Length - cI - 1)

    End Function

    Private Sub getConnectedUsers(ByVal msg As String)

        Dim namesList() As String = msg.Split(":"c)(2).Split(" "c)

        For i As Integer = 0 To namesList.Length - 1

            If namesList(i) <> My.Settings.nick Then
                RaiseEvent initJoin(namesList(i))
            End If

        Next

    End Sub

    Public Sub disconnect()

        receiveThread.Abort()
        tcpc.Close()

    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' Dient zur Erkennung redundanter Aufrufe.

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                tcpc.Close()
            End If

            ' TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalize() weiter unten überschreiben.
            ' TODO: große Felder auf Null setzen.
        End If
        disposedValue = True
    End Sub

    ' TODO: Finalize() nur überschreiben, wenn Dispose(disposing As Boolean) weiter oben Code zur Bereinigung nicht verwalteter Ressourcen enthält.
    'Protected Overrides Sub Finalize()
    '    ' Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(disposing As Boolean) weiter oben ein.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Dieser Code wird von Visual Basic hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(disposing As Boolean) weiter oben ein.
        Dispose(True)
        ' TODO: Auskommentierung der folgenden Zeile aufheben, wenn Finalize() oben überschrieben wird.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
