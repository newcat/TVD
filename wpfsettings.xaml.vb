Option Strict On
Option Explicit On

Public Class wpfsettings

    Private Sub linkGenerate_MouseLeftButtonDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles linkGenerate.MouseLeftButtonDown
        Process.Start("http://www.twitchapps.com/tmi")
    End Sub


    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnOK.Click
        If tbNick.Text = "" Or tbOauth.Text = "" Or tbChannel.Text = "" Or tbOauth.Text.Substring(0, 6) <> "oauth:" Then

            MsgBox("Invalid input!")
            Return

        End If

        Me.DialogResult = True

        My.Settings.nick = tbNick.Text
        My.Settings.oauth = tbOauth.Text
        My.Settings.channel = tbChannel.Text
        My.Settings.debug = CBool(cbDebug.IsChecked)
        My.Settings.showJoinLeaveEvents = CBool(cbShowEvents.IsChecked)
        My.Settings.Save()

        Me.Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnCancel.Click
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub Window_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        tbNick.Text = My.Settings.nick
        tbOauth.Text = My.Settings.oauth
        tbChannel.Text = My.Settings.channel
        cbDebug.IsChecked = My.Settings.debug
        cbShowEvents.IsChecked = My.Settings.showJoinLeaveEvents
    End Sub

    Private Sub cbDebug_Checked(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles cbDebug.MouseDown

        cbDebug.InvalidateVisual()

    End Sub
End Class
