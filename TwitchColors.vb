Option Strict On
Option Explicit On

Public Class TwitchColors
    Implements IDisposable

    Dim colors(15) As Color

    Public Sub initializeColors()

        colors(0) = Color.FromRgb(255, 0, 0)
        colors(1) = Color.FromRgb(0, 255, 0)
        colors(2) = Color.FromRgb(0, 0, 255)
        colors(3) = Color.FromRgb(178, 34, 34)
        colors(4) = Color.FromRgb(255, 127, 80)
        colors(5) = Color.FromRgb(154, 205, 50)
        colors(6) = Color.FromRgb(255, 69, 0)
        colors(7) = Color.FromRgb(46, 139, 87)
        colors(8) = Color.FromRgb(218, 165, 32)
        colors(9) = Color.FromRgb(210, 105, 30)
        colors(10) = Color.FromRgb(95, 158, 160)
        colors(11) = Color.FromRgb(30, 144, 255)
        colors(12) = Color.FromRgb(255, 105, 180)
        colors(13) = Color.FromRgb(138, 43, 226)
        colors(14) = Color.FromRgb(0, 255, 127)

    End Sub

    Public Function getRandomColor() As Color

        Return colors(CInt(Math.Floor((14 + 1) * Rnd())))

    End Function

    Public Function getColorByUsername(ByVal username As String) As Color

        Dim hash As Integer = username.GetHashCode()

        Return colors(Math.Abs(hash Mod 14))

    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' So ermitteln Sie überflüssige Aufrufe

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: Verwalteten Zustand löschen (verwaltete Objekte).
                colors = Nothing
            End If

            ' TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalize() unten überschreiben.
            ' TODO: Große Felder auf NULL festlegen.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: Finalize() nur überschreiben, wenn Dispose(ByVal disposing As Boolean) oben über Code zum Freigeben von nicht verwalteten Ressourcen verfügt.
    'Protected Overrides Sub Finalize()
    '    ' Ändern Sie diesen Code nicht. Fügen Sie oben in Dispose(ByVal disposing As Boolean) Bereinigungscode ein.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Dieser Code wird von Visual Basic hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Ändern Sie diesen Code nicht. Fügen Sie oben in Dispose(disposing As Boolean) Bereinigungscode ein.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
