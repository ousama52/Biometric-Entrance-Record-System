Imports System.Collections.ObjectModel
Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.Drawing.Drawing2D
Imports System.IO.Ports
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes
Imports System.Threading
Imports System.Xml
Imports MySql.Data.MySqlClient
Imports Mysqlx.XDevAPI.Common
Public Class Form1
    'server=localhost; user=yout_database_user; password=your_database_password; database=your_database_name
    Dim Connection As New MySqlConnection("server=localhost; user=root; password=; database=entrancerecord")
    Dim connectionString As String = "YourConnectionString" ' Replace with your MySQL connection string
    Dim MySQLCMD As New MySqlCommand
    Dim MySQLDA As New MySqlDataAdapter
    Dim DT As New DataTable
    Dim Dit As New DataTable
    Dim Table_Name As String = "entrancerecord" 'your table name
    Dim Data As Integer
    Dim LoadImagesStr As Boolean = False
    Dim IDRam As String
    Dim IMG_FileNameInput As String
    Dim StatusInput As String = "Save"
    Dim SqlCmdSearchstr As String
    Public result As String
    Public Function strstconnection() As MySqlConnection
        Return New MySqlConnection("server=localhost; user=root; password=; database=entrancerecord")
    End Function
    Public strcon As MySqlConnection = strstconnection()
    Public Shared StrSerialIn As String
    Dim GetID As Boolean = False
    Dim ViewUserData As Boolean = False
    Private borderRadius As Integer = 30
    Private borderSize As Integer = 3
    Private borderColor As Color = Color.Lavender
    'Constructor
    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.Style = cp.Style Or &H20000 '<--- Minimize borderless form from taskbar
            Return cp
        End Get
    End Property
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Padding = New Padding(borderSize)
        Me.BackColor = borderColor
    End Sub
    <DllImport("user32.DLL", EntryPoint:="ReleaseCapture")>
    Private Shared Sub ReleaseCapture()
    End Sub
    <DllImport("user32.DLL", EntryPoint:="SendMessage")>
    Private Shared Sub SendMessage(ByVal hWnd As System.IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer)
    End Sub
    Private Sub panelTitleBar_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseDown, Panel5.MouseDown
        ReleaseCapture()
        SendMessage(Me.Handle, &H112, &HF012, 0)
    End Sub
    Private Function GetRoundedPath(rect As Rectangle, radius As Single) As GraphicsPath
        Dim path As GraphicsPath = New GraphicsPath()
        Dim curveSize As Single = radius * 2.0F
        path.StartFigure()
        path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90)
        path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90)
        path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90)
        path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90)
        path.CloseFigure()
        Return path
    End Function
    Private Sub FormRegionAndBorder(form As Form, radius As Single, graph As Graphics, borderColor As Color, borderSize As Single)
        If Me.WindowState <> FormWindowState.Minimized Then
            Using roundPath As GraphicsPath = GetRoundedPath(form.ClientRectangle, radius)
                Using penBorder As Pen = New Pen(borderColor, borderSize)
                    Using transform As Matrix = New Matrix()
                        graph.SmoothingMode = SmoothingMode.AntiAlias
                        form.Region = New Region(roundPath)
                        If borderSize >= 1 Then
                            Dim rect As Rectangle = form.ClientRectangle
                            Dim scaleX As Single = 1.0F - ((borderSize + 1) / rect.Width)
                            Dim scaleY As Single = 1.0F - ((borderSize + 1) / rect.Height)
                            transform.Scale(scaleX, scaleY)
                            transform.Translate(borderSize / 1.6F, borderSize / 1.6F)
                            graph.Transform = transform
                            'graph.DrawPath(penBorder, roundPath)
                        End If
                    End Using
                End Using
            End Using
        End If
    End Sub
    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim rectForm As Rectangle = Me.ClientRectangle
        Dim mWidht As Integer = rectForm.Width / 2
        Dim mHeight As Integer = rectForm.Width / 2
        ' Dim fbColor = GetFormBoundsColors()
        DrawPath(rectForm, e.Graphics, Color.Lavender)
        Dim rectTopRight As New Rectangle(mWidht, rectForm.Y, mWidht, mHeight)
        DrawPath(rectTopRight, e.Graphics, Color.Lavender)
        Dim rectBottomLeft As New Rectangle(rectForm.X, rectForm.X + mHeight, mWidht, mHeight)
        DrawPath(rectBottomLeft, e.Graphics, Color.Lavender)
        Dim rectBottomRight As New Rectangle(mWidht, rectForm.Y + mHeight, mWidht, mHeight)
        DrawPath(rectBottomLeft, e.Graphics, Color.Lavender)
        FormRegionAndBorder(Me, borderRadius, e.Graphics, borderColor, borderSize)
    End Sub
    Private Sub DrawPath(rectForm As Rectangle, graphics As Graphics, color As Color)
        Using roundPath As GraphicsPath = GetRoundedPath(rectForm, borderRadius)
            Using penBorder As Pen = New Pen(color, 3)
                graphics.DrawPath(penBorder, roundPath)
            End Using
        End Using
    End Sub
    Private Structure FormBoundsColors
        Public TopLeftColor As Color
        Public TopRightColor As Color
        Public BottomLeftColor As Color
        Public BottomRightColor As Color
    End Structure
    Private Function GetFormBoundsColors() As FormBoundsColors
        Dim fbColor = New FormBoundsColors()
        Using bmp = New Bitmap(1, 1)
            Using graph As Graphics = Graphics.FromImage(bmp)
                Dim rectBmp As New Rectangle(0, 0, 1, 1)
                'Top Left
                rectBmp.X = Me.Bounds.X - 1
                rectBmp.Y = Me.Bounds.Y
                graph.CopyFromScreen(rectBmp.Location, Point.Empty, rectBmp.Size)
                fbColor.TopLeftColor = bmp.GetPixel(0, 0)
                'Top Right
                rectBmp.X = Me.Bounds.Right
                rectBmp.Y = Me.Bounds.Y
                graph.CopyFromScreen(rectBmp.Location, Point.Empty, rectBmp.Size)
                fbColor.TopRightColor = bmp.GetPixel(0, 0)
                'Bottom Left
                rectBmp.X = Me.Bounds.X
                rectBmp.Y = Me.Bounds.Bottom
                graph.CopyFromScreen(rectBmp.Location, Point.Empty, rectBmp.Size)
                fbColor.BottomLeftColor = bmp.GetPixel(0, 0)
                'Bottom Right
                rectBmp.X = Me.Bounds.Right
                rectBmp.Y = Me.Bounds.Bottom
                graph.CopyFromScreen(rectBmp.Location, Point.Empty, rectBmp.Size)
                fbColor.BottomRightColor = bmp.GetPixel(0, 0)
            End Using
        End Using
        Return fbColor
    End Function
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.CenterToScreen()
        PanelConnection.Visible = True
        PanelUserData.Visible = False
        PanelRegisterationandEditUserData.Visible = False
        PanelRecord.Visible = False
        PanelReport.Visible = False
        PanelAboutUs.Visible = False
        PanelDate.Visible = False
        PanelNo.Visible = False
        ComboBoxBaudRate.SelectedIndex = 3
        ShowData()
    End Sub
    Private Sub ShowData()
        Try
            Connection.Open()
        Catch ex As Exception
            MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Try
            If LoadImagesStr = False Then
                MySQLCMD.CommandType = CommandType.Text
                MySQLCMD.CommandText = "SELECT Name, ID, NRC, Email, Roll , Rank FROM " & Table_Name & " ORDER BY Name"
                MySQLDA = New MySqlDataAdapter(MySQLCMD.CommandText, Connection)
                DT = New DataTable
                Data = MySQLDA.Fill(DT)
                If Data > 0 Then
                    DataGridView1.DataSource = Nothing
                    DataGridView1.DataSource = DT
                    DataGridView1.Columns(2).DefaultCellStyle.Format = "c"
                    DataGridView1.DefaultCellStyle.ForeColor = Color.Black
                    DataGridView1.ClearSelection()
                Else
                    DataGridView1.DataSource = DT
                End If
            Else
                MySQLCMD.CommandType = CommandType.Text
                MySQLCMD.CommandText = "SELECT Images FROM " & Table_Name & " WHERE ID LIKE '" & IDRam & "'"
                MySQLDA = New MySqlDataAdapter(MySQLCMD.CommandText, Connection)
                DT = New DataTable
                Data = MySQLDA.Fill(DT)
                If Data > 0 Then
                    Dim ImgArray() As Byte = DT.Rows(0).Item("Images")
                    Dim lmgStr As New System.IO.MemoryStream(ImgArray)
                    PictureBoxImagePreview.Image = Image.FromStream(lmgStr)
                    PictureBoxImagePreview.SizeMode = PictureBoxSizeMode.Zoom
                    lmgStr.Close()
                End If
                LoadImagesStr = False
            End If
        Catch ex As Exception
            MsgBox("Failed to load Database !!!" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
            Connection.Close()
            Return
        End Try
        DT = Nothing
        Connection.Close()
    End Sub
    Private Sub ShowDataUser()
        Try
            Connection.Open()
        Catch ex As Exception
            MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Try
            MySQLCMD.CommandType = CommandType.Text
            MySQLCMD.CommandText = "SELECT * FROM " & Table_Name & " WHERE ID LIKE '" & LabelID.Text.Substring(5, LabelID.Text.Length - 5) & "'"
            MySQLDA = New MySqlDataAdapter(MySQLCMD.CommandText, Connection)
            DT = New DataTable
            Data = MySQLDA.Fill(DT)
            If Data > 0 Then
                Dim ImgArray() As Byte = DT.Rows(0).Item("Images")
                Dim lmgStr As New System.IO.MemoryStream(ImgArray)
                PictureBoxUserImage.Image = Image.FromStream(lmgStr)
                lmgStr.Close()
                LabelID.Text = "ID : " & DT.Rows(0).Item("ID")
                LabelName.Text = DT.Rows(0).Item("Name")
                LabelNRCNo.Text = DT.Rows(0).Item("NRC")
                LabelContact.Text = DT.Rows(0).Item("Email")
                LabelRoll.Text = DT.Rows(0).Item("Roll")
                LabelRank.Text = DT.Rows(0).Item("Rank")
            Else
                MsgBox("ID not found !!!" & vbCr & "Please register your ID.", MsgBoxStyle.Information, "Information Message")
            End If
        Catch ex As Exception
            MsgBox("Failed to load Database !!!" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
            Connection.Close()
            Return
        End Try

        DT = Nothing
        Connection.Close()
    End Sub
    Private Sub ClearInputUpdateData()
        TextBoxName.Text = ""
        LabelGetID.Text = "________"
        TextBoxNRCNo.Text = ""
        TextBoxContact.Text = ""
        TextBoxRoll.Text = ""
        TextBoxRank.Text = ""
        PictureBoxImageInput.Image = My.Resources._186390_refresh_sync_icon__2_
    End Sub
    Private Sub ButtonConnection_Click(sender As Object, e As EventArgs) Handles ButtonConnection.Click
        PanelConnection.Visible = True
        PanelUserData.Visible = False
        PanelRegisterationandEditUserData.Visible = False
        PanelRecord.Visible = False
        PanelReport.Visible = False
        PanelAboutUs.Visible = False
        PanelNo.Visible = False
    End Sub
    Private Sub ButtonUserData_Click(sender As Object, e As EventArgs) Handles ButtonUserData.Click
        If TimerSerialIn.Enabled = False Then
            MsgBox("Failed to open User Data !!!" & vbCr & "Click the Connection menu then click the Connect button.", MsgBoxStyle.Information, "Information")
            Return
        Else
            StrSerialIn = ""
            ViewUserData = True
            PanelRegisterationandEditUserData.Visible = False
            PanelConnection.Visible = False
            PanelUserData.Visible = True
            PanelRecord.Visible = False
            PanelReport.Visible = False
            PanelAboutUs.Visible = False
            PanelNo.Visible = False
        End If
    End Sub
    Private Sub ButtonRecord_Click(sender As Object, e As EventArgs) Handles ButtonRecord.Click
        PanelRegisterationandEditUserData.Visible = False
        PanelConnection.Visible = False
        PanelUserData.Visible = False
        PanelRecord.Visible = True
        PanelReport.Visible = False
        PanelAboutUs.Visible = False
        PanelNo.Visible = False
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) 
        PanelRegisterationandEditUserData.Visible = False
        PanelConnection.Visible = False
        PanelUserData.Visible = False
        PanelRecord.Visible = False
        PanelReport.Visible = False
        PanelAboutUs.Visible = False
        PanelNo.Visible = True
    End Sub
    Private Sub ButtonRegistration_Click(sender As Object, e As EventArgs) Handles ButtonRegistration.Click
        StrSerialIn = ""
        ViewUserData = False
        PanelConnection.Visible = False
        PanelUserData.Visible = False
        PanelRegisterationandEditUserData.Visible = True
        PanelRecord.Visible = False
        PanelReport.Visible = False
        PanelAboutUs.Visible = False
        PanelNo.Visible = False
        ShowData()
    End Sub
    Private Sub ButtonReport_Click(sender As Object, e As EventArgs) Handles ButtonReport.Click
        PanelRegisterationandEditUserData.Visible = False
        PanelConnection.Visible = False
        PanelUserData.Visible = False
        PanelRecord.Visible = False
        PanelReport.Visible = True
        PanelAboutUs.Visible = False
        PanelNo.Visible = False
    End Sub
    Private Sub ButtonAboutUs_Click(sender As Object, e As EventArgs) Handles ButtonAboutUs.Click
        PanelRegisterationandEditUserData.Visible = False
        PanelConnection.Visible = False
        PanelUserData.Visible = False
        PanelRecord.Visible = False
        PanelReport.Visible = False
        PanelAboutUs.Visible = True
        PanelNo.Visible = False
    End Sub
    Private Sub PanelConnection_Paint(sender As Object, e As PaintEventArgs) Handles PanelConnection.Paint
        e.Graphics.DrawRectangle(New Pen(Color.LightGray, 2), PanelConnection.ClientRectangle)
    End Sub
    Private Sub PanelConnection_Resize(sender As Object, e As EventArgs) Handles PanelConnection.Resize
        PanelConnection.Invalidate()
    End Sub
    Private Sub Form1_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        Me.Invalidate()
        GroupBoxImage.Location = New Point((PanelUserData.Width / 2) - (GroupBoxImage.Width / 2), GroupBoxImage.Top)
        PanelReadingTagProcess.Location = New Point((PanelRegisterationandEditUserData.Width / 2) - (PanelReadingTagProcess.Width / 2), 106)
    End Sub
    Private Sub PanelRegisterationandEditUserData_Resize(sender As Object, e As EventArgs) Handles PanelRegisterationandEditUserData.Resize
        PanelRegisterationandEditUserData.Invalidate()
    End Sub
    Private Sub PanelUserData_Paint(sender As Object, e As PaintEventArgs) Handles PanelUserData.Paint
        e.Graphics.DrawRectangle(New Pen(Color.LightGray, 2), PanelUserData.ClientRectangle)
    End Sub
    Private Sub PanelUserData_Resize(sender As Object, e As EventArgs) Handles PanelUserData.Resize
        PanelUserData.Invalidate()
    End Sub
    Private Sub ButtonScanPort_Click(sender As Object, e As EventArgs) Handles ButtonScanPort.Click
        ComboBoxPort.Items.Clear()
        Dim myPort As Array
        Dim i As Integer
        myPort = IO.Ports.SerialPort.GetPortNames()
        ComboBoxPort.Items.AddRange(myPort)
        i = ComboBoxPort.Items.Count
        i = i - i
        Try
            ComboBoxPort.SelectedIndex = i
        Catch ex As Exception
            MsgBox("Com port not detected", MsgBoxStyle.Critical, "Error Message")
            ComboBoxPort.Text = ""
            ComboBoxPort.Items.Clear()
            Return
        End Try
        ComboBoxPort.DroppedDown = True
    End Sub
    Private Sub ButtonScanPort_MouseHover(sender As Object, e As EventArgs) Handles ButtonScanPort.MouseHover
        ButtonScanPort.ForeColor = Color.White
    End Sub
    Private Sub ButtonScanPort_MouseLeave(sender As Object, e As EventArgs) Handles ButtonScanPort.MouseLeave
        ButtonScanPort.ForeColor = Color.FromArgb(6, 71, 165)
    End Sub
    Private Sub ButtonConnect_Click(sender As Object, e As EventArgs) Handles ButtonConnect.Click
        If ButtonConnect.Text = "Connect" Then
            SerialPort1.BaudRate = ComboBoxBaudRate.SelectedItem
            SerialPort1.PortName = ComboBoxPort.SelectedItem
            Try
                SerialPort1.Open()
                TimerSerialIn.Start()
                ButtonConnect.Text = "Disconnect"
                PictureBoxStatusConnect.Image = My.Resources.Connected
            Catch ex As Exception
                MsgBox("Failed to connect !!!" & vbCr & "Arduino is not detected.", MsgBoxStyle.Critical, "Error Message")
                PictureBoxStatusConnect.Image = My.Resources.Disconnect
            End Try
        ElseIf ButtonConnect.Text = "Disconnect" Then
            PictureBoxStatusConnect.Image = My.Resources.Disconnect
            ButtonConnect.Text = "Connect"
            LabelConnectionStatus.Text = "Connection Status : Disconnect"
            TimerSerialIn.Stop()
            SerialPort1.Close()
        End If
    End Sub
    Private Sub ButtonConnect_MouseHover(sender As Object, e As EventArgs) Handles ButtonConnect.MouseHover
        ButtonConnect.ForeColor = Color.White
    End Sub
    Private Sub ButtonConnect_MouseLeave(sender As Object, e As EventArgs) Handles ButtonConnect.MouseLeave
        ButtonConnect.ForeColor = Color.FromArgb(6, 71, 165)
    End Sub
    Private Sub ButtonClear_Click(sender As Object, e As EventArgs) Handles ButtonClear.Click
        LabelID.Text = "ID : ________"
        LabelName.Text = "Waiting..."
        LabelNRCNo.Text = "Waiting..."
        LabelContact.Text = "Waiting..."
        LabelRoll.Text = "Waiting..."
        LabelRank.Text = "Waiting..."
        PictureBoxUserImage.Image = Nothing
    End Sub
    Private Sub ButtonClear_MouseHover(sender As Object, e As EventArgs) Handles ButtonClear.MouseHover
        ButtonClear.ForeColor = Color.White
    End Sub
    Private Sub ButtonClear_MouseLeave(sender As Object, e As EventArgs) Handles ButtonClear.MouseLeave
        ButtonClear.ForeColor = Color.FromArgb(6, 71, 165)
    End Sub
    Private Sub ButtonSave_Click(sender As Object, e As EventArgs) Handles ButtonSave.Click
        Dim mstream As New System.IO.MemoryStream()
        Dim arrImage() As Byte
        If TextBoxName.Text = "" Then
            MessageBox.Show("Name cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If TextBoxNRCNo.Text = "" Then
            MessageBox.Show("NRC cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If TextBoxContact.Text = "" Then
            MessageBox.Show("Email cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If TextBoxRoll.Text = "" Then
            MessageBox.Show("Roll cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If TextBoxRank.Text = "" Then
            MessageBox.Show("Rank cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If StatusInput = "Save" Then
            If IMG_FileNameInput <> "" Then
                PictureBoxImageInput.Image.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg)
                arrImage = mstream.GetBuffer()
            Else
                MessageBox.Show("The image has not been selected !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            Try
                Connection.Open()
            Catch ex As Exception
                MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End Try
            Try
                MySQLCMD = New MySqlCommand
                With MySQLCMD
                    .CommandText = "INSERT INTO " & Table_Name & " (Name, ID, NRC, Email, Roll, Rank, Images) VALUES (@name, @ID, @NRC, @email, @roll, @rank, @images)"
                    .Connection = Connection
                    .Parameters.AddWithValue("@name", TextBoxName.Text)
                    .Parameters.AddWithValue("@id", LabelGetID.Text)
                    .Parameters.AddWithValue("@NRC", TextBoxNRCNo.Text)
                    .Parameters.AddWithValue("@email", TextBoxContact.Text)
                    .Parameters.AddWithValue("@roll", TextBoxRoll.Text)
                    .Parameters.AddWithValue("@rank", TextBoxRank.Text)
                    .Parameters.AddWithValue("@images", arrImage)
                    .ExecuteNonQuery()
                End With
                MsgBox("Data saved successfully", MsgBoxStyle.Information, "Information")
                IMG_FileNameInput = ""
                ClearInputUpdateData()
            Catch ex As Exception
                MsgBox("Data failed to save !!!" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
                Connection.Close()
                Return
            End Try
            Connection.Close()
        Else
            If IMG_FileNameInput <> "" Then
                PictureBoxImageInput.Image.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg)
                arrImage = mstream.GetBuffer()
                Try
                    Connection.Open()
                Catch ex As Exception
                    MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End Try
                Try
                    MySQLCMD = New MySqlCommand
                    With MySQLCMD
                        .CommandText = "UPDATE " & Table_Name & " SET  Name=@name,ID=@id,NRC=@NRC,Email=@email,Roll=@roll,Rank=@rank,Images=@images WHERE ID=@id "
                        .Connection = Connection
                        .Parameters.AddWithValue("@name", TextBoxName.Text)
                        .Parameters.AddWithValue("@id", LabelGetID.Text)
                        .Parameters.AddWithValue("@NRC", TextBoxNRCNo.Text)
                        .Parameters.AddWithValue("@mail", TextBoxContact.Text)
                        .Parameters.AddWithValue("@roll", TextBoxRoll.Text)
                        .Parameters.AddWithValue("@rank", TextBoxRank.Text)
                        .Parameters.AddWithValue("@images", arrImage)
                        .ExecuteNonQuery()
                    End With
                    MsgBox("Data updated successfully", MsgBoxStyle.Information, "Information")
                    IMG_FileNameInput = ""
                    ButtonSave.Text = "Save"
                    ClearInputUpdateData()
                Catch ex As Exception
                    MsgBox("Data failed to Update !!!" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
                    Connection.Close()
                    Return
                End Try
                Connection.Close()
            Else
                Try
                    Connection.Open()
                Catch ex As Exception
                    MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End Try
                Try
                    MySQLCMD = New MySqlCommand
                    With MySQLCMD
                        .CommandText = "UPDATE " & Table_Name & " SET  Name=@name,ID=@id,NRC=@NRC,Email=@email,Roll=@roll,Rank=@rank WHERE ID=@id "
                        .Connection = Connection
                        .Parameters.AddWithValue("@name", TextBoxName.Text)
                        .Parameters.AddWithValue("@id", LabelGetID.Text)
                        .Parameters.AddWithValue("@NRC", TextBoxNRCNo.Text)
                        .Parameters.AddWithValue("@email", TextBoxContact.Text)
                        .Parameters.AddWithValue("@roll", TextBoxRoll.Text)
                        .Parameters.AddWithValue("@rank", TextBoxRank.Text)
                        .ExecuteNonQuery()
                    End With
                    MsgBox("Data updated successfully", MsgBoxStyle.Information, "Information")
                    ButtonSave.Text = "Save"
                    ClearInputUpdateData()
                Catch ex As Exception
                    MsgBox("Data failed to Update !!!" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
                    Connection.Close()
                    Return
                End Try
                Connection.Close()
            End If
            StatusInput = "Save"
        End If
        PictureBoxImagePreview.Image = Nothing
        ShowData()
    End Sub
    Private Sub ButtonSave_MouseHover(sender As Object, e As EventArgs) Handles ButtonSave.MouseHover
        ButtonSave.ForeColor = Color.White
    End Sub
    Private Sub ButtonSave_MouseLeave(sender As Object, e As EventArgs) Handles ButtonSave.MouseLeave
        ButtonSave.ForeColor = Color.FromArgb(6, 71, 165)
    End Sub
    Private Sub ButtonClearForm_Click(sender As Object, e As EventArgs) Handles ButtonClearForm.Click
        ClearInputUpdateData()
    End Sub
    Private Sub ButtonClearForm_MouseHover(sender As Object, e As EventArgs) Handles ButtonClearForm.MouseHover
        ButtonClearForm.ForeColor = Color.White
    End Sub
    Private Sub ButtonClearForm_MouseLeave(sender As Object, e As EventArgs) Handles ButtonClearForm.MouseLeave
        ButtonClearForm.ForeColor = Color.FromArgb(6, 71, 165)
    End Sub
    Private Sub ButtonScanID_Click(sender As Object, e As EventArgs) Handles ButtonScanID.Click
        If TimerSerialIn.Enabled = True Then
            PanelReadingTagProcess.Visible = True
            GetID = True
            ButtonScanID.Enabled = False
        Else
            MsgBox("Failed to open User Data !!!" & vbCr & "Click the Connection menu then click the Connect button.", MsgBoxStyle.Critical, "Error Message")
        End If
    End Sub
    Private Sub ButtonScanID_MouseHover(sender As Object, e As EventArgs) Handles ButtonScanID.MouseHover
        ButtonScanID.ForeColor = Color.White
    End Sub
    Private Sub ButtonScanID_MouseLeave(sender As Object, e As EventArgs) Handles ButtonScanID.MouseLeave
        ButtonScanID.ForeColor = Color.FromArgb(6, 71, 165)
    End Sub
    Private Sub PictureBoxImageInput_Click(sender As Object, e As EventArgs) Handles PictureBoxImageInput.Click
        OpenFileDialog1.FileName = ""
        OpenFileDialog1.Filter = "JPEG (*.jpeg;*.jpg,*png)|*.jpeg;*.jpg;*png"

        If (OpenFileDialog1.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK) Then
            IMG_FileNameInput = OpenFileDialog1.FileName
            PictureBoxImageInput.ImageLocation = IMG_FileNameInput
        End If
    End Sub
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBoxByName.CheckedChanged
        If CheckBoxByName.Checked = True Then
            CheckBoxByID.Checked = False
        End If
        If CheckBoxByName.Checked = False Then
            CheckBoxByID.Checked = True
        End If
    End Sub
    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBoxByID.CheckedChanged
        If CheckBoxByID.Checked = True Then
            CheckBoxByName.Checked = False
        End If
        If CheckBoxByID.Checked = False Then
            CheckBoxByName.Checked = True
        End If
    End Sub
    Private Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBoxSearch.TextChanged
        If CheckBoxByID.Checked = True Then
            If TextBoxSearch.Text = Nothing Then
                SqlCmdSearchstr = "SELECT Name, ID, NRC, Email, Roll ,Rank FROM " & Table_Name & " ORDER BY Name"
            Else
                SqlCmdSearchstr = "SELECT Name, ID, NRC, Email, Roll ,Rank FROM " & Table_Name & " WHERE ID LIKE'" & TextBoxSearch.Text & "%'"
            End If
        End If
        If CheckBoxByName.Checked = True Then
            If TextBoxSearch.Text = Nothing Then
                SqlCmdSearchstr = "SELECT Name, ID, NRC, Email, Roll ,Rank FROM " & Table_Name & " ORDER BY Name"
            Else
                SqlCmdSearchstr = "SELECT Name, ID, NRC, Email, Roll ,Rank FROM " & Table_Name & " WHERE Name LIKE'" & TextBoxSearch.Text & "%'"
            End If
        End If

        Try
            Connection.Open()
        Catch ex As Exception
            MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Try
            MySQLDA = New MySqlDataAdapter(SqlCmdSearchstr, Connection)
            DT = New DataTable
            Data = MySQLDA.Fill(DT)
            If Data > 0 Then
                DataGridView1.DataSource = Nothing
                DataGridView1.DataSource = DT
                DataGridView1.DefaultCellStyle.ForeColor = Color.Black
                DataGridView1.ClearSelection()
            Else
                DataGridView1.DataSource = DT
            End If
        Catch ex As Exception
            MsgBox("Failed to search" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
            Connection.Close()
        End Try
        Connection.Close()
    End Sub
    Private Sub DataGridView1_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseDown
        Try
            If AllCellsSelected(DataGridView1) = False Then
                If e.Button = MouseButtons.Left Then
                    DataGridView1.CurrentCell = DataGridView1(e.ColumnIndex, e.RowIndex)
                    Dim i As Integer
                    With DataGridView1
                        If e.RowIndex >= 0 Then
                            i = .CurrentRow.Index
                            LoadImagesStr = True
                            IDRam = .Rows(i).Cells("ID").Value.ToString
                            ShowData()
                        End If
                    End With
                End If
            End If
        Catch ex As Exception
            Return
        End Try
    End Sub
    Private Function AllCellsSelected(dgv As DataGridView) As Boolean
        AllCellsSelected = (DataGridView1.SelectedCells.Count = (DataGridView1.RowCount * DataGridView1.Columns.GetColumnCount(DataGridViewElementStates.Visible)))
    End Function
    Private Sub TimerTimeDate_Tick(sender As Object, e As EventArgs) Handles TimerTimeDate.Tick
        LabelDateTime.Text = DateTime.Now.ToString("yyyy/M/d")
    End Sub
    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteToolStripMenuItem.Click
        If DataGridView1.RowCount = 0 Then
            MsgBox("Cannot delete, table data is empty", MsgBoxStyle.Critical, "Error Message")
            Return
        End If

        If DataGridView1.SelectedRows.Count = 0 Then
            MsgBox("Cannot delete, select the table data to be deleted", MsgBoxStyle.Critical, "Error Message")
            Return
        End If

        If MsgBox("Delete record?", MsgBoxStyle.Question + MsgBoxStyle.OkCancel, "Confirmation") = MsgBoxResult.Cancel Then Return

        Try
            Connection.Open()
        Catch ex As Exception
            MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        Try
            If AllCellsSelected(DataGridView1) = True Then
                MySQLCMD.CommandType = CommandType.Text
                MySQLCMD.CommandText = "DELETE FROM " & Table_Name
                MySQLCMD.Connection = Connection
                MySQLCMD.ExecuteNonQuery()
            End If

            For Each row As DataGridViewRow In DataGridView1.SelectedRows
                If row.Selected = True Then
                    MySQLCMD.CommandType = CommandType.Text
                    MySQLCMD.CommandText = "DELETE FROM " & Table_Name & " WHERE ID='" & row.DataBoundItem(1).ToString & "'"
                    MySQLCMD.Connection = Connection
                    MySQLCMD.ExecuteNonQuery()
                End If
            Next
        Catch ex As Exception
            MsgBox("Failed to delete" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
            Connection.Close()
        End Try
        PictureBoxImagePreview.Image = Nothing
        Connection.Close()
        ShowData()
    End Sub
    Private Sub SelectAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SelectAllToolStripMenuItem.Click
        DataGridView1.SelectAll()
    End Sub
    Private Sub ClearSelectionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClearSelectionToolStripMenuItem.Click
        DataGridView1.ClearSelection()
        PictureBoxImagePreview.Image = Nothing
    End Sub
    Private Sub RefreshToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RefreshToolStripMenuItem.Click
        ShowData()
    End Sub
    Private Sub TimerSerialIn_Tick(sender As Object, e As EventArgs) Handles TimerSerialIn.Tick
        Try
            StrSerialIn = SerialPort1.ReadExisting
            LabelConnectionStatus.Text = "Connection Status : Connected"
            If StrSerialIn <> "" Then
                If GetID = True Then
                    LabelGetID.Text = StrSerialIn
                    GetID = False
                    If LabelGetID.Text <> "________" Then
                        PanelReadingTagProcess.Visible = False
                        IDCheck()
                    End If
                End If
                If ViewUserData = True Then
                    ViewData()
                End If
            End If
        Catch ex As Exception
            TimerSerialIn.Stop()
            SerialPort1.Close()
            LabelConnectionStatus.Text = "Connection Status : Disconnect"
            PictureBoxStatusConnect.Image = My.Resources.Disconnect
            MsgBox("Failed to connect !!!" & vbCr & "Arduino is not detected.", MsgBoxStyle.Critical, "Error Message")
            ButtonConnect_Click(sender, e)
            Return
        End Try

        If PictureBoxStatusConnect.Visible = True Then
            PictureBoxStatusConnect.Visible = False
        ElseIf PictureBoxStatusConnect.Visible = False Then
            PictureBoxStatusConnect.Visible = True
        End If
    End Sub
    Private Sub IDCheck()
        Try
            Connection.Open()
        Catch ex As Exception
            MessageBox.Show("Connection failed !!!" & vbCrLf & "Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try
        Try
            MySQLCMD.CommandType = CommandType.Text
            MySQLCMD.CommandText = "SELECT * FROM entrancerecord WHERE ID LIKE '" & LabelGetID.Text & "'"
            MySQLDA = New MySqlDataAdapter(MySQLCMD.CommandText, Connection)
            DT = New DataTable
            Data = MySQLDA.Fill(DT)
            If Data > 0 Then
                If MsgBox("ID registered !" & vbCr & "Do you want to edit the data ?", MsgBoxStyle.Question + MsgBoxStyle.OkCancel, "Confirmation") = MsgBoxResult.Cancel Then
                    DT = Nothing
                    Connection.Close()
                    ButtonScanID.Enabled = True
                    GetID = False
                    LabelGetID.Text = "________"
                    Return
                Else
                    Dim ImgArray() As Byte = DT.Rows(0).Item("Images")
                    Dim lmgStr As New System.IO.MemoryStream(ImgArray)
                    PictureBoxImageInput.Image = Image.FromStream(lmgStr)
                    PictureBoxImageInput.SizeMode = PictureBoxSizeMode.Zoom

                    TextBoxName.Text = DT.Rows(0).Item("Name")
                    TextBoxNRCNo.Text = DT.Rows(0).Item("NRC")
                    TextBoxContact.Text = DT.Rows(0).Item("Email")
                    TextBoxRoll.Text = DT.Rows(0).Item("Roll")
                    TextBoxRank.Text = DT.Rows(0).Item("Rank")
                    StatusInput = "Update"
                End If
            End If
        Catch ex As Exception
            MsgBox("Failed to load Database !!!" & vbCr & ex.Message, MsgBoxStyle.Critical, "Error Message")
            Connection.Close()
            Return
        End Try
        DT = Nothing
        Connection.Close()

        ButtonScanID.Enabled = True
        GetID = False
    End Sub
    Private Sub ViewData()
        LabelID.Text = "ID : " & StrSerialIn
        LabelPrint.Text = StrSerialIn
        If LabelPrint.Text = StrSerialIn Then
            reloadtxt("SELECT * FROM entrancerecord WHERE ID ='" & LabelPrint.Text & "'")
            If Dit.Rows.Count > 0 Then
                reloadtxt("SELECT * FROM recordtable WHERE ID ='" & LabelPrint.Text & "'AND Date ='" & LabelDateTime.Text & "'AND AM='Time In' AND PM='Time Out'")
                If Dit.Rows.Count > 0 Then
                    MessageBox.Show("Already TIme In Out For Today")
                Else
                    reloadtxt("SELECT  * FROM recordtable WHERE ID ='" & LabelPrint.Text & "'AND Date ='" & LabelDateTime.Text & "'AND AM='Time In'")
                    If Dit.Rows.Count > 0 Then
                        updatesLogged("UPDATE recordtable SET TimeOut = '" & TimeOfDay & "', PM = 'Time Out' WHERE ID ='" & LabelPrint.Text & "'AND Date ='" & LabelDateTime.Text & "'AND AM='Time In'")
                        MessageBox.Show("Sucessfully Time Out", "Time Out", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        createLogged("INSERT INTO recordtable (ID, Date, TimeIn, AM, TimeOut, PM)VALUES ('" & LabelPrint.Text & "', '" & LabelDateTime.Text & "', '" & TimeOfDay & "', 'Time In', NULL, NULL);")
                        MessageBox.Show("Successfully Time In", "Time In", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If
            Else MessageBox.Show("Wrong")
            End If
        End If
        If LabelID.Text = "ID : ________" Then
            ViewData()
        Else
            ShowDataUser()
        End If
        Timer2.Interval = 2000 ' Set the interval to 1000 milliseconds (1 second)
        Timer2.Start()
    End Sub
    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        GroupBoxImage.Location = New Point((PanelUserData.Width / 2) - (GroupBoxImage.Width / 2), GroupBoxImage.Top)
        PanelReadingTagProcess.Location = New Point((PanelRegisterationandEditUserData.Width / 2) - (PanelReadingTagProcess.Width / 2), 106)
    End Sub
    Private Sub ButtonCloseReadingTag_Click(sender As Object, e As EventArgs) Handles ButtonCloseReadingTag.Click
        PanelReadingTagProcess.Visible = False
        ButtonScanID.Enabled = True
    End Sub
    Private Sub Form1_ResizeEnd(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd
        Me.Invalidate()
    End Sub
    Private Sub Form1_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Me.Invalidate()
    End Sub
    Private Sub ButtonExit_Click(sender As Object, e As EventArgs) Handles ButtonExit.Click
        Me.Close()
        LoadingForm.Close()
    End Sub
    Private Sub ButtonMinimize_Click(sender As Object, e As EventArgs) Handles ButtonMinimize.Click
        If Me.WindowState Or FormWindowState.Minimized Then
            Me.WindowState = FormWindowState.Minimized
        End If
    End Sub
    Private Sub ButtonMaximize_Click(sender As Object, e As EventArgs) Handles ButtonMaximize.Click
        If Me.WindowState = FormWindowState.Maximized Then
            Me.WindowState = FormWindowState.Normal
            Dim mouseLocX As Integer = 0, btncloseX = 0, btnMax = 0, btnMin = 0
            mouseLocX = ActiveForm.Width
            btncloseX = (mouseLocX - 50)
            btnMax = (mouseLocX - 80)
            btnMin = (mouseLocX - 110)
            Me.ButtonExit.Location = New Point(btncloseX, 0)
            Me.ButtonMaximize.Location = New Point(btnMax, 0)
            Me.ButtonMinimize.Location = New Point(btnMin, 0)
        Else
            Me.WindowState = FormWindowState.Maximized
            Dim mousecursorX = 0, closebtnX = 0, Maxbtn = 0, Minbtn = 0
            mousecursorX = ActiveForm.Width
            closebtnX = (mousecursorX - 50)
            Maxbtn = (mousecursorX - 80)
            Minbtn = (mousecursorX - 110)
            Me.ButtonExit.Location = New Point(closebtnX, 0)
            Me.ButtonMaximize.Location = New Point(Maxbtn, 0)
            Me.ButtonMinimize.Location = New Point(Minbtn, 0)
        End If
    End Sub
    Public Sub reload(ByVal sql As String, ByVal DTA As Object)
        Try
            strcon.Open()
            Dit = New DataTable
            With MySQLCMD
                .Connection = strcon
                .CommandText = sql
            End With
            MySQLDA.SelectCommand = MySQLCMD
            MySQLDA.Fill(Dit)
            DTA.DataSource = Dit
        Catch ex As Exception
        Finally
            strcon.Close()
            MySQLDA.Dispose()
        End Try
    End Sub
    Public Sub reloadtxt(ByVal sql As String)
        Try
            strcon.Open()
            With MySQLCMD
                .Connection = strcon
                .CommandText = sql
            End With
            Dit = New DataTable
            MySQLDA = New MySqlDataAdapter(sql, strcon)
            MySQLDA.Fill(Dit)
        Catch ex As Exception
        Finally
            strcon.Close()
            MySQLDA.Dispose()
        End Try
    End Sub
    Public Sub createLogged(ByVal sql As String)
        Try
            strcon.Open()
            With MySQLCMD
                .Connection = strcon
                .CommandText = sql
                result = MySQLCMD.ExecuteNonQuery
            End With
        Catch ex As Exception
        Finally
            strcon.Close()
        End Try
    End Sub
    Public Sub updatesLogged(ByVal sql As String)
        Try
            strcon.Open()
            With MySQLCMD
                .Connection = strcon
                .CommandText = sql
                result = MySQLCMD.ExecuteNonQuery
            End With
        Catch ex As Exception
        Finally
            strcon.Close()
        End Try
    End Sub
    Private Sub btnTimeInOut_Click(sender As Object, e As EventArgs) Handles btnTimeInOut.Click
        Try
            If TextBoxNRC.Text = "" Then
                MessageBox.Show("Please Enter Registered NRC", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Else
                reloadtxt("SELECT * FROM entrancerecord WHERE NRC='" & TextBoxNRC.Text & "'")
                If Dit.Rows.Count > 0 Then
                    reloadtxt("SELECT entrancerecord.ID FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE entrancerecord.NRC ='" & TextBoxNRC.Text & "'AND Date ='" & LabelDateTime.Text & "'AND AM='Time In' AND PM='Time Out'")
                    If Dit.Rows.Count > 0 Then
                        MessageBox.Show("Already Time In Out For Today", "Already", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        reloadtxt("SELECT  * FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE entrancerecord.NRC = '" & TextBoxNRC.Text & "' AND Date = '" & LabelDateTime.Text & "'AND AM = 'Time In';")
                        If Dit.Rows.Count > 0 Then
                            updatesLogged("UPDATE recordtable SET TimeOut = '" & TimeOfDay & "', PM = 'Time Out' WHERE ID IN (SELECT recordtable.ID FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE entrancerecord.NRC = '" & TextBoxNRC.Text & "' AND Date = '" & LabelDateTime.Text & "'AND AM = 'Time In')")
                            MessageBox.Show("Sucessfully Time Out", "Time Out", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Else
                            createLogged("INSERT INTO recordtable (ID, Date, TimeIn, AM, TimeOut, PM) SELECT entrancerecord.ID, '" & LabelDateTime.Text & "', '" & TimeOfDay & "', 'Time In', NULL, NULL FROM entrancerecord WHERE entrancerecord.NRC = '" & TextBoxNRC.Text & "';")
                            MessageBox.Show("Successfully Time In", "Time In", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If
                    End If
                Else
                    MessageBox.Show("NRC not Found", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)
                End If
            End If
        Catch ex As Exception
        End Try
        TextBoxNRC.Text = ""
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        LabelPrint.Text = "ID"
        Timer2.Stop()
    End Sub
    Public Sub ReloadRecord()
        Try
            Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID;"
            reload(sql, DataGridView2)

        Catch ex As Exception
        End Try
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        ReloadRecord()
    End Sub
    Private Sub CheckBoxAll_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBoxAll.CheckedChanged
        If CheckBoxAll.Checked = True Then
            PanelDate.Visible = True
            CheckBoxTeacher.Checked = False
            CheckBoxStudent.Checked = False
        End If
    End Sub
    Private Sub CheckBoxTeacher_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBoxTeacher.CheckedChanged
        If CheckBoxTeacher.Checked = True Then
            PanelDate.Visible = True
            CheckBoxAll.Checked = False
            CheckBoxStudent.Checked = False
        End If
    End Sub

    Private Sub CheckBoxStudent_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBoxStudent.CheckedChanged
        If CheckBoxStudent.Checked = True Then
            PanelDate.Visible = True
            CheckBoxAll.Checked = False
            CheckBoxTeacher.Checked = False
        End If
    End Sub
    Private Sub btnDaily_Click(sender As Object, e As EventArgs) Handles btnDaily.Click
        If CheckBoxAll.Checked = True Then
            Try
                Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE DATE(Date)=CURDATE();"
                reload(sql, DataGridView3)
                DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Catch ex As Exception
            End Try
        End If
        If CheckBoxTeacher.Checked = True Then
            Try
                Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE DATE(Date)=CURDATE() AND entrancerecord.Rank <> 'Student';"
                reload(sql, DataGridView3)
                DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Catch ex As Exception
            End Try
        End If
        If CheckBoxStudent.Checked = True Then
            Try
                Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE DATE(Date)=CURDATE() AND entrancerecord.Rank = 'Student';"
                reload(sql, DataGridView3)
                DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub btnWeekly_Click(sender As Object, e As EventArgs) Handles btnWeekly.Click
        If CheckBoxAll.Checked = True Then
            Try
                Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE WEEK(Date)=WEEK(NOW());"
                reload(sql, DataGridView3)
                DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Catch ex As Exception
            End Try
        End If
        If CheckBoxTeacher.Checked = True Then
                Try
                    Dim sql As String = "SELECT Name, Roll, Rank, Email, NRC, Date, TimeIn, TimeOut FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE WEEK(Date)=WEEK(NOW()) AND entrancerecord.Rank <> 'Student';"
                    reload(sql, DataGridView3)
                    DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                Catch ex As Exception
                End Try
            End If
            If CheckBoxStudent.Checked = True Then
                Try
                    Dim sql As String = "SELECT Name, Roll, Rank, Email, NRC, Date, TimeIn, TimeOut FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE WEEK(Date)=WEEK(NOW()) AND entrancerecord.Rank = 'Student';"
                    reload(sql, DataGridView3)
                    DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                Catch ex As Exception
                End Try

            End If
    End Sub
    Private Sub btnMonthly_Click(sender As Object, e As EventArgs) Handles btnMonthly.Click
        If CheckBoxAll.Checked = True Then
            Try
                Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE MONTH(Date)=MONTH(NOW());"
                reload(sql, DataGridView3)
                DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Catch ex As Exception
            End Try
        End If
        If CheckBoxTeacher.Checked = True Then
            Try
                Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE MONTH(Date)=MONTH(NOW()) AND entrancerecord.Rank <> 'Student';"
                reload(sql, DataGridView3)
                DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Catch ex As Exception
            End Try
        End If
        If CheckBoxStudent.Checked = True Then
            Try
                Dim sql As String = "SELECT  Name, Roll, Rank, Email, NRC,  Date, TimeIn,  TimeOut  FROM recordtable JOIN entrancerecord ON recordtable.ID = entrancerecord.ID WHERE MONTH(Date)=MONTH(NOW()) AND entrancerecord.Rank = 'Student';"
                reload(sql, DataGridView3)
                DataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub ButtonPrint_Click(sender As Object, e As EventArgs) Handles ButtonPrint.Click
        PrintPreviewDialog1.ShowDialog()
        PrintDocument1.Print()
    End Sub
    Private Sub PrintDocument1_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintDocument1.PrintPage
        Dim columnWidth As Integer = DataGridView3.Columns.GetColumnsWidth(DataGridViewElementStates.Visible)
        Dim columnHeight As Integer = DataGridView3.Rows.GetRowsHeight(DataGridViewElementStates.Visible)
        Dim bm As New Bitmap(columnWidth, columnHeight)
        DataGridView3.DrawToBitmap(bm, New Rectangle(0, 0, columnWidth, columnHeight))
        Dim scaleWidth As Single = e.PageBounds.Width / columnWidth
        Dim scaleHeight As Single = e.PageBounds.Height / columnHeight
        Dim scale As Single = Math.Min(scaleWidth, scaleHeight)
        Dim destinationWidth As Integer = CInt(columnWidth * scale)
        Dim destinationHeight As Integer = CInt(columnHeight * scale)
        Dim destinationRectangle As New Rectangle(0, 0, destinationWidth, destinationHeight)
        e.Graphics.DrawImage(bm, destinationRectangle)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) 
        Label44.Visible = True
        PictureBox12.Visible = True
    End Sub

    Private Sub Label44_Click(sender As Object, e As EventArgs) 

    End Sub
End Class