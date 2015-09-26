Option Strict On
Option Explicit On

Imports System.Net.Http

Public Class autoupdater

    Dim cDir As String = My.Computer.FileSystem.CurrentDirectory

    Public Async Sub check()

        If My.Computer.FileSystem.FileExists(cDir + "\updater.exe") Then
            My.Computer.FileSystem.DeleteFile(cDir + "\updater.exe")
        End If

        Dim localVersion As String = GetType(Application).Assembly.GetName().Version.ToString()

        Dim cl As New HttpClient
        Dim newestVersion As String = Await cl.GetStringAsync(New Uri("http://newcat.bplaced.net/tvd/version"))

        If localVersion <> newestVersion Then
            updateFound()
        End If

    End Sub

    Private Sub updateFound()

        If MsgBox("Update found! Download now?", MsgBoxStyle.YesNo, "TVD Updater") = MsgBoxResult.Yes Then
            Process.Start("http://newcat.bplaced.net/tvd/tvd.exe")
        End If

    End Sub

End Class
