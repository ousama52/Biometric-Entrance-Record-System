Imports System.Drawing.Drawing2D
Imports System.Drawing.Text

Public Class LoadingForm

    Private Sub LoadingForm_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        Dim w As Integer = Me.ClientSize.Width
        Dim h As Integer = Me.ClientSize.Height

        ' Darkened scrim over the lower part so the caption text stays readable
        ' on top of the photo (no letterboxing, the picture fills the window).
        Dim scrim As New Rectangle(0, h - 175, w, 175)
        Using lg As New LinearGradientBrush(scrim,
                                            Color.FromArgb(0, 8, 15, 35),
                                            Color.FromArgb(225, 8, 15, 35),
                                            LinearGradientMode.Vertical)
            g.FillRectangle(lg, scrim)
        End Using

        Using sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center
            Using titleFont As New Font("Segoe UI Semibold", 22.0F, FontStyle.Bold)
                g.DrawString("Entry Record System", titleFont, Brushes.White,
                             New RectangleF(0, h - 120, w, 42), sf)
            End Using
            Using subFont As New Font("Segoe UI", 11.0F)
                Using subBrush As New SolidBrush(Color.FromArgb(228, 234, 246))
                    g.DrawString("Fingerprint Attendance System", subFont, subBrush,
                                 New RectangleF(0, h - 80, w, 24), sf)
                End Using
            End Using
            Using ftFont As New Font("Segoe UI", 8.5F)
                Using ftBrush As New SolidBrush(Color.FromArgb(190, 200, 216))
                    g.DrawString("Developed & Designed by Project Group I", ftFont, ftBrush,
                                 New RectangleF(0, h - 54, w, 20), sf)
                End Using
            End Using
        End Using
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        ' Show the splash for the timer interval, then hand off to the main window.
        Timer1.Stop()
        Me.Hide()
        Form1.Show()
    End Sub

End Class
