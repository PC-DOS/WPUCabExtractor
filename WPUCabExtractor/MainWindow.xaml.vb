Imports System.IO
Imports System.Windows.Forms
Imports System.Windows.Window
Imports System.Xml
Class MainWindow
    Dim InputDirectory As String
    Dim OutputDiectory As String
    Dim ResourceDirectory As String
    Dim IsKeepStructure As Boolean
    Dim EmptyList As New List(Of String)
    Dim MessageList As New List(Of String)
    Sub RefreshMessageList()
        lstMessage.ItemsSource = EmptyList
        lstMessage.ItemsSource = MessageList
        DoEvents()
    End Sub
    Sub AddMessage(MessageText As String)
        MessageList.Add(MessageText)
        RefreshMessageList()
        lstMessage.SelectedIndex = lstMessage.Items.Count - 1
        lstMessage.ScrollIntoView(lstMessage.SelectedItem)
    End Sub
    Sub LockUI()
        txtInputDir.IsEnabled = False
        txtResourceDir.IsEnabled = False
        txtOutputDir.IsEnabled = False
        btnBrowseInput.IsEnabled = False
        btnBrowseResource.IsEnabled = False
        btnBrowseOutput.IsEnabled = False
        btnStart.IsEnabled = False
        chkKeepStructure.IsEnabled = False
    End Sub
    Sub UnlockUI()
        txtInputDir.IsEnabled = True
        txtResourceDir.IsEnabled = True
        txtOutputDir.IsEnabled = True
        btnBrowseInput.IsEnabled = True
        btnBrowseResource.IsEnabled = True
        btnBrowseOutput.IsEnabled = True
        btnStart.IsEnabled = True
        chkKeepStructure.IsEnabled = True
    End Sub
    Private Sub SetTaskbarProgess(MaxValue As Integer, MinValue As Integer, CurrentValue As Integer, Optional State As Shell.TaskbarItemProgressState = Shell.TaskbarItemProgressState.Normal)
        If MaxValue <= MinValue Or CurrentValue < MinValue Or CurrentValue > MaxValue Then
            Exit Sub
        End If
        TaskbarItem.ProgressValue = (CurrentValue - MinValue) / (MaxValue - MinValue)
        TaskbarItem.ProgressState = State
    End Sub
    Function GetPathFromFile(FilePath As String) As String
        If FilePath.Trim = "" Then
            Return ""
        End If
        If FilePath(FilePath.Length - 1) = "\" Then
            Return FilePath
        End If
        Try
            Return FilePath.Substring(0, FilePath.LastIndexOf("\"))
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Function GetNameFromFullPath(FullPath As String) As String
        If FullPath.Trim = "" Then
            Return ""
        End If
        If FullPath(FullPath.Length - 1) = "\" Then
            Return ""
        End If
        Try
            Return FullPath.Substring(FullPath.LastIndexOf("\") + 1, FullPath.LastIndexOf(".") - FullPath.LastIndexOf("\") - 1)
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Sub btnBrowseInput_Click(sender As Object, e As RoutedEventArgs) Handles btnBrowseInput.Click
        Dim FolderBrowser As New FolderBrowserDialog
        With FolderBrowser
            .Description = "请指定 DSM 文件的位置，然后单击""确定""按钮。"
        End With
        If FolderBrowser.ShowDialog() = Forms.DialogResult.OK Then
            InputDirectory = FolderBrowser.SelectedPath
            If InputDirectory(InputDirectory.Length - 1) <> "\" Then
                InputDirectory = InputDirectory & "\"
            End If
            txtInputDir.Text = InputDirectory
        End If
    End Sub
    Private Sub btnBrowseResource_Click(sender As Object, e As RoutedEventArgs) Handles btnBrowseResource.Click
        Dim FolderBrowser As New FolderBrowserDialog
        With FolderBrowser
            .Description = "请指定用于抽取文件的目录的位置，然后单击""确定""按钮。"
        End With
        If FolderBrowser.ShowDialog() = Forms.DialogResult.OK Then
            ResourceDirectory = FolderBrowser.SelectedPath
            If ResourceDirectory(ResourceDirectory.Length - 1) <> "\" Then
                ResourceDirectory = ResourceDirectory & "\"
            End If
            txtResourceDir.Text = ResourceDirectory
        End If
    End Sub

    Private Sub btnBrowseOutput_Click(sender As Object, e As RoutedEventArgs) Handles btnBrowseOutput.Click
        Dim FolderBrowser As New FolderBrowserDialog
        With FolderBrowser
            .Description = "请指定重建完成的目录结构要输出的位置，然后单击""确定""按钮。"
        End With
        If FolderBrowser.ShowDialog() = Forms.DialogResult.OK Then
            OutputDiectory = FolderBrowser.SelectedPath
            If OutputDiectory(OutputDiectory.Length - 1) <> "\" Then
                OutputDiectory = OutputDiectory & "\"
            End If
            txtOutputDir.Text = OutputDiectory
        End If
    End Sub

    Private Sub btnStart_Click(sender As Object, e As RoutedEventArgs) Handles btnStart.Click
        LockUI()
        If txtInputDir.Text.Trim = "" Then
            MessageBox.Show("DSM 文件输入路径不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            UnlockUI()
            Exit Sub
        End If
        If txtResourceDir.Text.Trim = "" Then
            MessageBox.Show("文件抽取源路径不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            UnlockUI()
            Exit Sub
        End If
        If txtOutputDir.Text.Trim = "" Then
            MessageBox.Show("输出路径不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            UnlockUI()
            Exit Sub
        End If
        If Not Directory.Exists(OutputDiectory) Then
            Try
                Directory.CreateDirectory(OutputDiectory)
            Catch ex As Exception
                MessageBox.Show("试图创建输出目录""" & OutputDiectory & """时发生错误: " & vbCrLf & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
                UnlockUI()
                Exit Sub
            End Try
        End If
        With prgProgress
            .Minimum = 0
            .Maximum = 100
            .Value = 0
        End With
        MessageList.Clear()
        RefreshMessageList()

        AddMessage("正在确定 DSM 文件总数。")
        Dim nDSMFileCount As Integer = Directory.GetFiles(InputDirectory, "*.dsm", SearchOption.TopDirectoryOnly).Length
        If nDSMFileCount = 0 Then
            MessageBox.Show("输入目录""" & InputDirectory & """中不包含任何 DSM 文件。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            AddMessage("输入目录""" & InputDirectory & """中不包含任何 DSM 文件。")
            AddMessage("发生错误，取消操作。")
            UnlockUI()
            Exit Sub
        End If
        AddMessage("计算完毕，共有 " & nDSMFileCount.ToString & " 个 DSM 文件。")
        With prgProgress
            .Minimum = 0
            .Maximum = nDSMFileCount
            .Value = 0
        End With
        SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
        Dim nSuccess As UInteger = 0
        Dim nFail As UInteger = 0
        Dim nIgnored As UInteger = 0
        Dim IsErrorOccurred As Boolean = False

        For Each DSMFilePath In Directory.EnumerateFiles(InputDirectory, "*.dsm", SearchOption.TopDirectoryOnly)
            Dim DSMFileName As String = GetNameFromFullPath(DSMFilePath)
            Dim UpdateInfoFile As New XmlDocument
            AddMessage("正在打开包描述文件""" & DSMFilePath & """。")
            Try
                UpdateInfoFile.Load(DSMFilePath)
            Catch ex As Exception
                AddMessage("无法打开包描述文件""" & DSMFilePath & """，发生错误: " & ex.Message)
                nFail += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
                Continue For
            End Try

            Dim nsMgr As New XmlNamespaceManager(UpdateInfoFile.NameTable)
            nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")

            Dim PartitionNode As XmlNode = UpdateInfoFile.SelectSingleNode("/ns:Package/ns:Partition", nsMgr)
            AddMessage("正在定位 XML 节点""/Package/Partition""。")
            If IsNothing(PartitionNode) Then
                AddMessage("XML 节点定位失败，退出操作。")
                nFail += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
                Continue For
            End If
            Dim PartitionName As String
            PartitionName = PartitionNode.InnerText
            AddMessage("XML 节点""/Package/PartitionName""定位成功，DSM 描述文件""" & DSMFilePath & """适用于分区""" & PartitionName & """。")

            Dim CustomInformationNode As XmlNode = UpdateInfoFile.SelectSingleNode("/ns:Package/ns:Files", nsMgr)
            AddMessage("正在定位 XML 节点""/Package/Files""。")
            If IsNothing(CustomInformationNode) Then
                AddMessage("XML 节点定位失败，退出操作。")
                nFail += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
                Continue For
            End If
            AddMessage("XML 节点""/Package/Files""定位成功，共有 " & CustomInformationNode.ChildNodes.Count & " 条记录。")
            Dim TempFileInfo As New WindowsUpdatePackageFileNodeProperties
            Dim FileList As XmlNodeList = CustomInformationNode.ChildNodes
            For Each FileNode As XmlNode In FileList
                IsErrorOccurred = False
                Dim FileElement As XmlElement = FileNode
                If FileElement.Name <> "FileEntry" Then
                    Continue For
                End If
                Try
                    With TempFileInfo
                        .CabPath = FileElement.GetElementsByTagName("CabPath")(0).InnerText
                        .DevicePath = FileElement.GetElementsByTagName("DevicePath")(0).InnerText
                        .FileType = FileElement.GetElementsByTagName("FileType")(0).InnerText
                    End With
                    Dim CopySource As String = ResourceDirectory & TempFileInfo.DevicePath
                    Dim CopyDest As String
                    If chkKeepStructure.IsChecked Then
                        CopyDest = OutputDiectory & TempFileInfo.DevicePath
                    Else
                        CopyDest = OutputDiectory & DSMFileName & "\" & TempFileInfo.CabPath
                    End If
                    If Not Directory.Exists(GetPathFromFile(CopyDest)) Then
                        Directory.CreateDirectory(GetPathFromFile(CopyDest))
                    End If
                    If File.Exists(CopyDest) Then
                        File.Delete(CopyDest)
                    End If
                    File.Copy(CopySource, CopyDest)
                    AddMessage("已成功从""" & CopySource & """复制文件到""" & CopyDest & """。")

                    DoEvents()
                Catch ex As Exception
                    AddMessage("根据 DSM 文件""" & DSMFilePath & """的指示复制文件时发生错误: " & ex.Message)
                    IsErrorOccurred = True
                    Continue For
                End Try
            Next
            If Not IsErrorOccurred Then
                AddMessage("对 DSM 文件""" & DSMFilePath & """的操作成功完成。")
                nSuccess += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
            Else
                AddMessage("处理 DSM 文件""" & DSMFilePath & """时发生一个或多个错误。")
                nFail += 1
                prgProgress.Value += 1
                SetTaskbarProgess(prgProgress.Maximum, 0, prgProgress.Value)
            End If
        Next

        MessageBox.Show("操作完成，共有 " & nSuccess.ToString & "个 DSM 文件被成功处理，有 " & nIgnored.ToString & " 个 DSM 文件被忽略，处理 " & nFail.ToString & " 个 DSM 文件时出错。", "大功告成!", MessageBoxButtons.OK, MessageBoxIcon.Information)
        UnlockUI()
        With prgProgress
            .Minimum = 0
            .Maximum = 100
            .Value = 0
        End With
        SetTaskbarProgess(100, 0, 0)
    End Sub

    Private Sub chkKeepStructure_Click(sender As Object, e As RoutedEventArgs) Handles chkKeepStructure.Click
        IsKeepStructure = chkKeepStructure.IsChecked
    End Sub
End Class
