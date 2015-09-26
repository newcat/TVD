Option Strict On
Option Explicit On

Imports System.IO
Imports System.Net

Public Class SpamProtector

    Dim phrases As New List(Of SpamPhrase)
    Public isInitialized As Boolean = False
    Public isActive As Boolean = False

    Public Async Function loadPhrases() As Task

        Dim wc As New WebClient
        Dim phraseData As String

        Try
            phraseData = Await wc.DownloadStringTaskAsync(New Uri("http://newcat.bplaced.net/tvd/spamPhrases.txt"))
        Catch e As WebException
            MsgBox("Failed to initialize the SpamProtector. It will stay inactive for this session.")
            Return
        End Try

        If IsNothing(phraseData) OrElse phraseData = "" OrElse (phraseData.Contains("404") AndAlso phraseData.Contains("bplaced.net")) Then Return

        For Each phrase As String In phraseData.Split(Chr(13))
            phrases.Add(New SpamPhrase(phrase))
        Next

        isInitialized = True
        isActive = True

    End Function

    ''' <summary>
    ''' Checks, if a message is spam
    ''' </summary>
    ''' <param name="msg">The message to check</param>
    ''' <returns>True if message is spam, false if not</returns>
    ''' <remarks></remarks>
    Public Function checkMsg(ByVal msg As String) As Boolean

        If Not isInitialized OrElse Not isActive Then Return False

        Dim isSpam As Boolean = False

        For Each phrase As SpamPhrase In phrases

            If phrase.check(msg) Then isSpam = True

        Next

        Return isSpam

    End Function

End Class

Public Class SpamPhrase

    Dim words As New List(Of String)

    Public Sub New(ByVal phrase As String)

        Dim wordArray As String() = phrase.Split(","c)

        For Each word As String In wordArray
            words.Add(word)
        Next

    End Sub

    Public Function check(ByVal msg As String) As Boolean

        Dim lowerMsg = msg.ToLower()
        Dim containsPhrase As Boolean = True

        For Each word As String In words
            If Not lowerMsg.Contains(word) Then containsPhrase = False
        Next

        Return containsPhrase
    End Function

End Class