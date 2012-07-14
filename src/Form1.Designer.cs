/*
 * Copyright (c) 2007, 2008 dmz
 * 
 * This file is part of TsRemux.
 * 
 * TsRemux is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * TsRemux is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with TsRemux.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

namespace TsRemux
{
    partial class TsRemux
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TsRemux));
            this.InputFileLabel = new System.Windows.Forms.Label();
            this.InputFileTextBox = new System.Windows.Forms.TextBox();
            this.InputFileBrowseButton = new System.Windows.Forms.Button();
            this.OutputFileBrowseButton = new System.Windows.Forms.Button();
            this.OutputFileTextBox = new System.Windows.Forms.TextBox();
            this.OutputFileLabel = new System.Windows.Forms.Label();
            this.OpenInputFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.OpenOutputFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ElementaryStreamsToKeep = new System.Windows.Forms.Label();
            this.ElementaryStreamsListBox = new System.Windows.Forms.CheckedListBox();
            this.ElementaryContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SaveElementaryStream = new System.Windows.Forms.SaveFileDialog();
            this.cbxUseAsyncIO = new System.Windows.Forms.CheckBox();
            this.OpenSupFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SupOffsetNumericSeconds = new System.Windows.Forms.NumericUpDown();
            this.SupOffsetNumericMinutes = new System.Windows.Forms.NumericUpDown();
            this.SupBegin = new System.Windows.Forms.Label();
            this.SupOffsetNumericHours = new System.Windows.Forms.NumericUpDown();
            this.ChapterLengthUpDown = new System.Windows.Forms.NumericUpDown();
            this.ChapterLengthMinutesLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.cbxBypassAudioProcessing = new System.Windows.Forms.CheckBox();
            this.cbxMlpToAc3 = new System.Windows.Forms.CheckBox();
            this.SelectOutputFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.TrimSetTimer = new System.Windows.Forms.Timer(this.components);
            this.SidePanel = new System.Windows.Forms.Panel();
            this.TrimEndSet = new System.Windows.Forms.Button();
            this.TrimStartSet = new System.Windows.Forms.Button();
            this.TrimEndNumericSeconds = new System.Windows.Forms.NumericUpDown();
            this.TrimEndNumericMinutes = new System.Windows.Forms.NumericUpDown();
            this.TrimEndLabel = new System.Windows.Forms.Label();
            this.TrimEndNumericHours = new System.Windows.Forms.NumericUpDown();
            this.TrimStartNumericSeconds = new System.Windows.Forms.NumericUpDown();
            this.TrimStartNumericMinutes = new System.Windows.Forms.NumericUpDown();
            this.TrimStartLabel = new System.Windows.Forms.Label();
            this.TrimStartNumericHours = new System.Windows.Forms.NumericUpDown();
            this.RemuxProgressTimeTextBox = new System.Windows.Forms.TextBox();
            this.Quit = new System.Windows.Forms.Button();
            this.RemuxButton = new System.Windows.Forms.Button();
            this.RemuxProgressLabel = new System.Windows.Forms.Label();
            this.RemuxProgressBar = new System.Windows.Forms.ProgressBar();
            this.InputInfoLabel = new System.Windows.Forms.Label();
            this.DtcpInfo = new System.Windows.Forms.TreeView();
            this.FormatBox = new System.Windows.Forms.GroupBox();
            this.DemuxFormatRadioButton = new System.Windows.Forms.RadioButton();
            this.MKVFormatRadioButton = new System.Windows.Forms.RadioButton();
            this.BluRayFormatRadioButton = new System.Windows.Forms.RadioButton();
            this.TsFormatRadioButton = new System.Windows.Forms.RadioButton();
            this.M2tsFormatRadioButton = new System.Windows.Forms.RadioButton();
            this.OuterPanel = new System.Windows.Forms.Panel();
            this.MPlayerPreview = new System.Windows.Forms.PictureBox();
            this.ControlPanel = new System.Windows.Forms.Panel();
            this.CurPlayPos = new System.Windows.Forms.Label();
            this.LargeForward = new System.Windows.Forms.Button();
            this.MediumForward = new System.Windows.Forms.Button();
            this.SmallForward = new System.Windows.Forms.Button();
            this.SmallBack = new System.Windows.Forms.Button();
            this.FrameStep = new System.Windows.Forms.Button();
            this.MediumBack = new System.Windows.Forms.Button();
            this.LargeBack = new System.Windows.Forms.Button();
            this.PlayPause = new System.Windows.Forms.Button();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.PlaybackPos = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.SupOffsetNumericSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SupOffsetNumericMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SupOffsetNumericHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ChapterLengthUpDown)).BeginInit();
            this.SidePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrimEndNumericSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimEndNumericMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimEndNumericHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimStartNumericSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimStartNumericMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimStartNumericHours)).BeginInit();
            this.FormatBox.SuspendLayout();
            this.OuterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MPlayerPreview)).BeginInit();
            this.ControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PlaybackPos)).BeginInit();
            this.SuspendLayout();
            // 
            // InputFileLabel
            // 
            this.InputFileLabel.AutoSize = true;
            this.InputFileLabel.Location = new System.Drawing.Point(13, 13);
            this.InputFileLabel.Name = "InputFileLabel";
            this.InputFileLabel.Size = new System.Drawing.Size(63, 13);
            this.InputFileLabel.TabIndex = 0;
            this.InputFileLabel.Text = "Source File:";
            // 
            // InputFileTextBox
            // 
            this.InputFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFileTextBox.ForeColor = System.Drawing.Color.Green;
            this.InputFileTextBox.Location = new System.Drawing.Point(83, 8);
            this.InputFileTextBox.MaxLength = 256;
            this.InputFileTextBox.Name = "InputFileTextBox";
            this.InputFileTextBox.ReadOnly = true;
            this.InputFileTextBox.Size = new System.Drawing.Size(684, 20);
            this.InputFileTextBox.TabIndex = 1;
            this.InputFileTextBox.WordWrap = false;
            // 
            // InputFileBrowseButton
            // 
            this.InputFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFileBrowseButton.Location = new System.Drawing.Point(774, 8);
            this.InputFileBrowseButton.Name = "InputFileBrowseButton";
            this.InputFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.InputFileBrowseButton.TabIndex = 2;
            this.InputFileBrowseButton.Text = "Browse";
            this.InputFileBrowseButton.UseVisualStyleBackColor = true;
            this.InputFileBrowseButton.Click += new System.EventHandler(this.InputFileBrowseButton_Click);
            // 
            // OutputFileBrowseButton
            // 
            this.OutputFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFileBrowseButton.Enabled = false;
            this.OutputFileBrowseButton.Location = new System.Drawing.Point(774, 37);
            this.OutputFileBrowseButton.Name = "OutputFileBrowseButton";
            this.OutputFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.OutputFileBrowseButton.TabIndex = 5;
            this.OutputFileBrowseButton.Text = "Browse";
            this.OutputFileBrowseButton.UseVisualStyleBackColor = true;
            this.OutputFileBrowseButton.Click += new System.EventHandler(this.OutputFileBrowseButton_Click);
            // 
            // OutputFileTextBox
            // 
            this.OutputFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFileTextBox.ForeColor = System.Drawing.Color.Green;
            this.OutputFileTextBox.Location = new System.Drawing.Point(83, 39);
            this.OutputFileTextBox.MaxLength = 256;
            this.OutputFileTextBox.Name = "OutputFileTextBox";
            this.OutputFileTextBox.ReadOnly = true;
            this.OutputFileTextBox.Size = new System.Drawing.Size(684, 20);
            this.OutputFileTextBox.TabIndex = 4;
            this.OutputFileTextBox.WordWrap = false;
            // 
            // OutputFileLabel
            // 
            this.OutputFileLabel.AutoSize = true;
            this.OutputFileLabel.Location = new System.Drawing.Point(13, 42);
            this.OutputFileLabel.Name = "OutputFileLabel";
            this.OutputFileLabel.Size = new System.Drawing.Size(61, 13);
            this.OutputFileLabel.TabIndex = 3;
            this.OutputFileLabel.Text = "Output File:";
            // 
            // OpenInputFileDialog
            // 
            this.OpenInputFileDialog.FileName = "OpenInputFileDialog";
            this.OpenInputFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenInputFileDialog_FileOk);
            // 
            // OpenOutputFileDialog
            // 
            this.OpenOutputFileDialog.AddExtension = false;
            this.OpenOutputFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenOutputFileDialog_FileOk);
            // 
            // ElementaryStreamsToKeep
            // 
            this.ElementaryStreamsToKeep.AutoSize = true;
            this.ElementaryStreamsToKeep.Location = new System.Drawing.Point(13, 71);
            this.ElementaryStreamsToKeep.Name = "ElementaryStreamsToKeep";
            this.ElementaryStreamsToKeep.Size = new System.Drawing.Size(236, 13);
            this.ElementaryStreamsToKeep.TabIndex = 7;
            this.ElementaryStreamsToKeep.Text = "Select the elementary streams you want to keep:";
            // 
            // ElementaryStreamsListBox
            // 
            this.ElementaryStreamsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ElementaryStreamsListBox.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.ElementaryStreamsListBox.CheckOnClick = true;
            this.ElementaryStreamsListBox.ContextMenuStrip = this.ElementaryContextMenu;
            this.ElementaryStreamsListBox.Enabled = false;
            this.ElementaryStreamsListBox.FormattingEnabled = true;
            this.ElementaryStreamsListBox.Location = new System.Drawing.Point(16, 95);
            this.ElementaryStreamsListBox.Name = "ElementaryStreamsListBox";
            this.ElementaryStreamsListBox.Size = new System.Drawing.Size(583, 79);
            this.ElementaryStreamsListBox.TabIndex = 8;
            this.ElementaryStreamsListBox.ThreeDCheckBoxes = true;
            this.ElementaryStreamsListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ElementaryStreamsListBox_ItemCheck);
            // 
            // ElementaryContextMenu
            // 
            this.ElementaryContextMenu.BackColor = System.Drawing.Color.White;
            this.ElementaryContextMenu.Name = "ElementaryContextMenu";
            this.ElementaryContextMenu.Size = new System.Drawing.Size(61, 4);
            this.ElementaryContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ElementaryContextMenu_Opening);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            // 
            // SaveElementaryStream
            // 
            this.SaveElementaryStream.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveElementaryStream_FileOk);
            // 
            // cbxUseAsyncIO
            // 
            this.cbxUseAsyncIO.AutoSize = true;
            this.cbxUseAsyncIO.Checked = true;
            this.cbxUseAsyncIO.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxUseAsyncIO.Location = new System.Drawing.Point(10, 319);
            this.cbxUseAsyncIO.Name = "cbxUseAsyncIO";
            this.cbxUseAsyncIO.Size = new System.Drawing.Size(95, 17);
            this.cbxUseAsyncIO.TabIndex = 28;
            this.cbxUseAsyncIO.Text = "Use async I/O";
            this.cbxUseAsyncIO.UseVisualStyleBackColor = true;
            // 
            // OpenSupFileDialog
            // 
            this.OpenSupFileDialog.FileName = "OpenSupFileDialog";
            this.OpenSupFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenSupFileDialog_FileOk);
            // 
            // SupOffsetNumericSeconds
            // 
            this.SupOffsetNumericSeconds.Location = new System.Drawing.Point(85, 477);
            this.SupOffsetNumericSeconds.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.SupOffsetNumericSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.SupOffsetNumericSeconds.Name = "SupOffsetNumericSeconds";
            this.SupOffsetNumericSeconds.Size = new System.Drawing.Size(34, 20);
            this.SupOffsetNumericSeconds.TabIndex = 32;
            this.SupOffsetNumericSeconds.ValueChanged += new System.EventHandler(this.SupOffsetNumericSeconds_ValueChanged);
            // 
            // SupOffsetNumericMinutes
            // 
            this.SupOffsetNumericMinutes.Location = new System.Drawing.Point(48, 477);
            this.SupOffsetNumericMinutes.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.SupOffsetNumericMinutes.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.SupOffsetNumericMinutes.Name = "SupOffsetNumericMinutes";
            this.SupOffsetNumericMinutes.Size = new System.Drawing.Size(31, 20);
            this.SupOffsetNumericMinutes.TabIndex = 31;
            this.SupOffsetNumericMinutes.ValueChanged += new System.EventHandler(this.SupOffsetNumericMinutes_ValueChanged);
            // 
            // SupBegin
            // 
            this.SupBegin.AutoSize = true;
            this.SupBegin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SupBegin.Location = new System.Drawing.Point(8, 461);
            this.SupBegin.Name = "SupBegin";
            this.SupBegin.Size = new System.Drawing.Size(165, 13);
            this.SupBegin.TabIndex = 30;
            this.SupBegin.Text = "SUP Offset (H:M:S) (Subtitle shift)";
            // 
            // SupOffsetNumericHours
            // 
            this.SupOffsetNumericHours.Location = new System.Drawing.Point(16, 477);
            this.SupOffsetNumericHours.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.SupOffsetNumericHours.Name = "SupOffsetNumericHours";
            this.SupOffsetNumericHours.Size = new System.Drawing.Size(26, 20);
            this.SupOffsetNumericHours.TabIndex = 29;
            this.SupOffsetNumericHours.ValueChanged += new System.EventHandler(this.SupOffsetNumericHours_ValueChanged);
            // 
            // ChapterLengthUpDown
            // 
            this.ChapterLengthUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChapterLengthUpDown.Location = new System.Drawing.Point(513, 377);
            this.ChapterLengthUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.ChapterLengthUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ChapterLengthUpDown.Name = "ChapterLengthUpDown";
            this.ChapterLengthUpDown.Size = new System.Drawing.Size(54, 20);
            this.ChapterLengthUpDown.TabIndex = 34;
            this.ChapterLengthUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.ChapterLengthUpDown.ValueChanged += new System.EventHandler(this.ChapterLengthUpDown_ValueChanged);
            // 
            // ChapterLengthMinutesLabel
            // 
            this.ChapterLengthMinutesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChapterLengthMinutesLabel.AutoSize = true;
            this.ChapterLengthMinutesLabel.Location = new System.Drawing.Point(573, 379);
            this.ChapterLengthMinutesLabel.Name = "ChapterLengthMinutesLabel";
            this.ChapterLengthMinutesLabel.Size = new System.Drawing.Size(43, 13);
            this.ChapterLengthMinutesLabel.TabIndex = 35;
            this.ChapterLengthMinutesLabel.Text = "minutes";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(428, 379);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 36;
            this.label1.Text = "Chapter length:";
            // 
            // backgroundWorker2
            // 
            this.backgroundWorker2.WorkerReportsProgress = true;
            this.backgroundWorker2.WorkerSupportsCancellation = true;
            this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            this.backgroundWorker2.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
            this.backgroundWorker2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker2_ProgressChanged);
            // 
            // cbxBypassAudioProcessing
            // 
            this.cbxBypassAudioProcessing.AutoSize = true;
            this.cbxBypassAudioProcessing.Checked = true;
            this.cbxBypassAudioProcessing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxBypassAudioProcessing.Location = new System.Drawing.Point(10, 296);
            this.cbxBypassAudioProcessing.Name = "cbxBypassAudioProcessing";
            this.cbxBypassAudioProcessing.Size = new System.Drawing.Size(139, 17);
            this.cbxBypassAudioProcessing.TabIndex = 37;
            this.cbxBypassAudioProcessing.Text = "Bypass Audio Alignment";
            this.cbxBypassAudioProcessing.UseVisualStyleBackColor = true;
            // 
            // cbxMlpToAc3
            // 
            this.cbxMlpToAc3.AutoSize = true;
            this.cbxMlpToAc3.Enabled = false;
            this.cbxMlpToAc3.Location = new System.Drawing.Point(10, 273);
            this.cbxMlpToAc3.Name = "cbxMlpToAc3";
            this.cbxMlpToAc3.Size = new System.Drawing.Size(222, 17);
            this.cbxMlpToAc3.TabIndex = 38;
            this.cbxMlpToAc3.Text = "Blu-Ray TrueHD to AC3/DTS HD to DTS";
            this.cbxMlpToAc3.UseVisualStyleBackColor = true;
            // 
            // TrimSetTimer
            // 
            this.TrimSetTimer.Tick += new System.EventHandler(this.TrimSetTimer_Tick);
            // 
            // SidePanel
            // 
            this.SidePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SidePanel.Controls.Add(this.TrimEndSet);
            this.SidePanel.Controls.Add(this.TrimStartSet);
            this.SidePanel.Controls.Add(this.cbxMlpToAc3);
            this.SidePanel.Controls.Add(this.TrimEndNumericSeconds);
            this.SidePanel.Controls.Add(this.TrimEndNumericMinutes);
            this.SidePanel.Controls.Add(this.TrimEndLabel);
            this.SidePanel.Controls.Add(this.cbxUseAsyncIO);
            this.SidePanel.Controls.Add(this.cbxBypassAudioProcessing);
            this.SidePanel.Controls.Add(this.TrimEndNumericHours);
            this.SidePanel.Controls.Add(this.TrimStartNumericSeconds);
            this.SidePanel.Controls.Add(this.TrimStartNumericMinutes);
            this.SidePanel.Controls.Add(this.ChapterLengthMinutesLabel);
            this.SidePanel.Controls.Add(this.TrimStartLabel);
            this.SidePanel.Controls.Add(this.ChapterLengthUpDown);
            this.SidePanel.Controls.Add(this.TrimStartNumericHours);
            this.SidePanel.Controls.Add(this.label1);
            this.SidePanel.Controls.Add(this.RemuxProgressTimeTextBox);
            this.SidePanel.Controls.Add(this.SupBegin);
            this.SidePanel.Controls.Add(this.SupOffsetNumericSeconds);
            this.SidePanel.Controls.Add(this.Quit);
            this.SidePanel.Controls.Add(this.SupOffsetNumericMinutes);
            this.SidePanel.Controls.Add(this.RemuxButton);
            this.SidePanel.Controls.Add(this.SupOffsetNumericHours);
            this.SidePanel.Controls.Add(this.RemuxProgressLabel);
            this.SidePanel.Controls.Add(this.RemuxProgressBar);
            this.SidePanel.Controls.Add(this.InputInfoLabel);
            this.SidePanel.Controls.Add(this.DtcpInfo);
            this.SidePanel.Controls.Add(this.FormatBox);
            this.SidePanel.Location = new System.Drawing.Point(605, 71);
            this.SidePanel.Name = "SidePanel";
            this.SidePanel.Size = new System.Drawing.Size(244, 662);
            this.SidePanel.TabIndex = 42;
            // 
            // TrimEndSet
            // 
            this.TrimEndSet.Location = new System.Drawing.Point(143, 426);
            this.TrimEndSet.Name = "TrimEndSet";
            this.TrimEndSet.Size = new System.Drawing.Size(92, 20);
            this.TrimEndSet.TabIndex = 58;
            this.TrimEndSet.Text = "Set";
            this.TrimEndSet.UseVisualStyleBackColor = true;
            this.TrimEndSet.Click += new System.EventHandler(this.TrimEndSet_Click);
            // 
            // TrimStartSet
            // 
            this.TrimStartSet.Location = new System.Drawing.Point(143, 379);
            this.TrimStartSet.Name = "TrimStartSet";
            this.TrimStartSet.Size = new System.Drawing.Size(92, 20);
            this.TrimStartSet.TabIndex = 57;
            this.TrimStartSet.Text = "Set";
            this.TrimStartSet.UseVisualStyleBackColor = true;
            this.TrimStartSet.Click += new System.EventHandler(this.TrimStartSet_Click);
            // 
            // TrimEndNumericSeconds
            // 
            this.TrimEndNumericSeconds.Location = new System.Drawing.Point(85, 428);
            this.TrimEndNumericSeconds.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.TrimEndNumericSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.TrimEndNumericSeconds.Name = "TrimEndNumericSeconds";
            this.TrimEndNumericSeconds.Size = new System.Drawing.Size(34, 20);
            this.TrimEndNumericSeconds.TabIndex = 56;
            this.TrimEndNumericSeconds.ValueChanged += new System.EventHandler(this.TrimEndNumericSeconds_ValueChanged);
            // 
            // TrimEndNumericMinutes
            // 
            this.TrimEndNumericMinutes.Location = new System.Drawing.Point(48, 428);
            this.TrimEndNumericMinutes.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.TrimEndNumericMinutes.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.TrimEndNumericMinutes.Name = "TrimEndNumericMinutes";
            this.TrimEndNumericMinutes.Size = new System.Drawing.Size(31, 20);
            this.TrimEndNumericMinutes.TabIndex = 55;
            this.TrimEndNumericMinutes.ValueChanged += new System.EventHandler(this.TrimEndNumericMinutes_ValueChanged);
            // 
            // TrimEndLabel
            // 
            this.TrimEndLabel.AutoSize = true;
            this.TrimEndLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TrimEndLabel.Location = new System.Drawing.Point(9, 412);
            this.TrimEndLabel.Name = "TrimEndLabel";
            this.TrimEndLabel.Size = new System.Drawing.Size(88, 13);
            this.TrimEndLabel.TabIndex = 54;
            this.TrimEndLabel.Text = "Trim End (H:M:S)";
            this.TrimEndLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TrimEndNumericHours
            // 
            this.TrimEndNumericHours.Location = new System.Drawing.Point(16, 428);
            this.TrimEndNumericHours.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.TrimEndNumericHours.Name = "TrimEndNumericHours";
            this.TrimEndNumericHours.Size = new System.Drawing.Size(26, 20);
            this.TrimEndNumericHours.TabIndex = 53;
            this.TrimEndNumericHours.ValueChanged += new System.EventHandler(this.TrimEndNumericHours_ValueChanged);
            // 
            // TrimStartNumericSeconds
            // 
            this.TrimStartNumericSeconds.Location = new System.Drawing.Point(85, 379);
            this.TrimStartNumericSeconds.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.TrimStartNumericSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.TrimStartNumericSeconds.Name = "TrimStartNumericSeconds";
            this.TrimStartNumericSeconds.Size = new System.Drawing.Size(34, 20);
            this.TrimStartNumericSeconds.TabIndex = 52;
            this.TrimStartNumericSeconds.ValueChanged += new System.EventHandler(this.TrimStartNumericSeconds_ValueChanged);
            // 
            // TrimStartNumericMinutes
            // 
            this.TrimStartNumericMinutes.Location = new System.Drawing.Point(48, 379);
            this.TrimStartNumericMinutes.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.TrimStartNumericMinutes.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.TrimStartNumericMinutes.Name = "TrimStartNumericMinutes";
            this.TrimStartNumericMinutes.Size = new System.Drawing.Size(31, 20);
            this.TrimStartNumericMinutes.TabIndex = 51;
            this.TrimStartNumericMinutes.ValueChanged += new System.EventHandler(this.TrimStartNumericMinutes_ValueChanged);
            // 
            // TrimStartLabel
            // 
            this.TrimStartLabel.AutoSize = true;
            this.TrimStartLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TrimStartLabel.Location = new System.Drawing.Point(9, 363);
            this.TrimStartLabel.Name = "TrimStartLabel";
            this.TrimStartLabel.Size = new System.Drawing.Size(116, 13);
            this.TrimStartLabel.TabIndex = 50;
            this.TrimStartLabel.Text = "Trim Beginning (H:M:S)";
            // 
            // TrimStartNumericHours
            // 
            this.TrimStartNumericHours.Location = new System.Drawing.Point(16, 379);
            this.TrimStartNumericHours.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.TrimStartNumericHours.Name = "TrimStartNumericHours";
            this.TrimStartNumericHours.Size = new System.Drawing.Size(26, 20);
            this.TrimStartNumericHours.TabIndex = 49;
            this.TrimStartNumericHours.ValueChanged += new System.EventHandler(this.TrimStartNumericHours_ValueChanged);
            // 
            // RemuxProgressTimeTextBox
            // 
            this.RemuxProgressTimeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RemuxProgressTimeTextBox.Location = new System.Drawing.Point(116, 577);
            this.RemuxProgressTimeTextBox.Name = "RemuxProgressTimeTextBox";
            this.RemuxProgressTimeTextBox.ReadOnly = true;
            this.RemuxProgressTimeTextBox.Size = new System.Drawing.Size(119, 20);
            this.RemuxProgressTimeTextBox.TabIndex = 48;
            this.RemuxProgressTimeTextBox.TabStop = false;
            // 
            // Quit
            // 
            this.Quit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Quit.Location = new System.Drawing.Point(143, 634);
            this.Quit.Name = "Quit";
            this.Quit.Size = new System.Drawing.Size(92, 23);
            this.Quit.TabIndex = 47;
            this.Quit.Text = "Quit";
            this.Quit.UseVisualStyleBackColor = true;
            this.Quit.Click += new System.EventHandler(this.Quit_Click);
            // 
            // RemuxButton
            // 
            this.RemuxButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RemuxButton.Location = new System.Drawing.Point(10, 634);
            this.RemuxButton.Name = "RemuxButton";
            this.RemuxButton.Size = new System.Drawing.Size(92, 23);
            this.RemuxButton.TabIndex = 46;
            this.RemuxButton.UseVisualStyleBackColor = true;
            this.RemuxButton.Click += new System.EventHandler(this.RemuxButton_Click);
            // 
            // RemuxProgressLabel
            // 
            this.RemuxProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RemuxProgressLabel.AutoSize = true;
            this.RemuxProgressLabel.Location = new System.Drawing.Point(10, 580);
            this.RemuxProgressLabel.Name = "RemuxProgressLabel";
            this.RemuxProgressLabel.Size = new System.Drawing.Size(87, 13);
            this.RemuxProgressLabel.TabIndex = 45;
            this.RemuxProgressLabel.Text = "Remux Progress:";
            // 
            // RemuxProgressBar
            // 
            this.RemuxProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RemuxProgressBar.Location = new System.Drawing.Point(10, 605);
            this.RemuxProgressBar.Name = "RemuxProgressBar";
            this.RemuxProgressBar.Size = new System.Drawing.Size(225, 23);
            this.RemuxProgressBar.TabIndex = 44;
            // 
            // InputInfoLabel
            // 
            this.InputInfoLabel.AutoSize = true;
            this.InputInfoLabel.Location = new System.Drawing.Point(9, 8);
            this.InputInfoLabel.Name = "InputInfoLabel";
            this.InputInfoLabel.Size = new System.Drawing.Size(84, 13);
            this.InputInfoLabel.TabIndex = 43;
            this.InputInfoLabel.Text = "Source File Info:";
            // 
            // DtcpInfo
            // 
            this.DtcpInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DtcpInfo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.DtcpInfo.Enabled = false;
            this.DtcpInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.DtcpInfo.Location = new System.Drawing.Point(10, 24);
            this.DtcpInfo.Name = "DtcpInfo";
            this.DtcpInfo.Size = new System.Drawing.Size(225, 121);
            this.DtcpInfo.TabIndex = 41;
            // 
            // FormatBox
            // 
            this.FormatBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FormatBox.Controls.Add(this.DemuxFormatRadioButton);
            this.FormatBox.Controls.Add(this.MKVFormatRadioButton);
            this.FormatBox.Controls.Add(this.BluRayFormatRadioButton);
            this.FormatBox.Controls.Add(this.TsFormatRadioButton);
            this.FormatBox.Controls.Add(this.M2tsFormatRadioButton);
            this.FormatBox.Location = new System.Drawing.Point(10, 151);
            this.FormatBox.Name = "FormatBox";
            this.FormatBox.Size = new System.Drawing.Size(225, 116);
            this.FormatBox.TabIndex = 42;
            this.FormatBox.TabStop = false;
            this.FormatBox.Text = "Output Format";
            // 
            // DemuxFormatRadioButton
            // 
            this.DemuxFormatRadioButton.AutoSize = true;
            this.DemuxFormatRadioButton.Enabled = false;
            this.DemuxFormatRadioButton.Location = new System.Drawing.Point(7, 88);
            this.DemuxFormatRadioButton.Name = "DemuxFormatRadioButton";
            this.DemuxFormatRadioButton.Size = new System.Drawing.Size(88, 17);
            this.DemuxFormatRadioButton.TabIndex = 4;
            this.DemuxFormatRadioButton.TabStop = true;
            this.DemuxFormatRadioButton.Text = "Demux (beta)";
            this.DemuxFormatRadioButton.UseVisualStyleBackColor = true;
            // 
            // MKVFormatRadioButton
            // 
            this.MKVFormatRadioButton.AutoSize = true;
            this.MKVFormatRadioButton.Enabled = false;
            this.MKVFormatRadioButton.Location = new System.Drawing.Point(112, 88);
            this.MKVFormatRadioButton.Name = "MKVFormatRadioButton";
            this.MKVFormatRadioButton.Size = new System.Drawing.Size(74, 17);
            this.MKVFormatRadioButton.TabIndex = 3;
            this.MKVFormatRadioButton.TabStop = true;
            this.MKVFormatRadioButton.Text = "MKV (exp)";
            this.MKVFormatRadioButton.UseVisualStyleBackColor = true;
            this.MKVFormatRadioButton.Visible = false;
            this.MKVFormatRadioButton.CheckedChanged += new System.EventHandler(this.MKVFormatRadioButton_CheckedChanged);
            // 
            // BluRayFormatRadioButton
            // 
            this.BluRayFormatRadioButton.AutoSize = true;
            this.BluRayFormatRadioButton.Enabled = false;
            this.BluRayFormatRadioButton.Location = new System.Drawing.Point(7, 65);
            this.BluRayFormatRadioButton.Name = "BluRayFormatRadioButton";
            this.BluRayFormatRadioButton.Size = new System.Drawing.Size(62, 17);
            this.BluRayFormatRadioButton.TabIndex = 2;
            this.BluRayFormatRadioButton.TabStop = true;
            this.BluRayFormatRadioButton.Text = "Blu-Ray";
            this.BluRayFormatRadioButton.UseVisualStyleBackColor = true;
            this.BluRayFormatRadioButton.CheckedChanged += new System.EventHandler(this.BluRayFormatRadioButton_CheckedChanged);
            // 
            // TsFormatRadioButton
            // 
            this.TsFormatRadioButton.AutoSize = true;
            this.TsFormatRadioButton.Enabled = false;
            this.TsFormatRadioButton.Location = new System.Drawing.Point(7, 42);
            this.TsFormatRadioButton.Name = "TsFormatRadioButton";
            this.TsFormatRadioButton.Size = new System.Drawing.Size(130, 17);
            this.TsFormatRadioButton.TabIndex = 1;
            this.TsFormatRadioButton.TabStop = true;
            this.TsFormatRadioButton.Text = "TS (188 byte packets)";
            this.TsFormatRadioButton.UseVisualStyleBackColor = true;
            this.TsFormatRadioButton.CheckedChanged += new System.EventHandler(this.TsFormatRadioButton_CheckedChanged);
            // 
            // M2tsFormatRadioButton
            // 
            this.M2tsFormatRadioButton.AutoSize = true;
            this.M2tsFormatRadioButton.Enabled = false;
            this.M2tsFormatRadioButton.Location = new System.Drawing.Point(7, 19);
            this.M2tsFormatRadioButton.Name = "M2tsFormatRadioButton";
            this.M2tsFormatRadioButton.Size = new System.Drawing.Size(145, 17);
            this.M2tsFormatRadioButton.TabIndex = 0;
            this.M2tsFormatRadioButton.TabStop = true;
            this.M2tsFormatRadioButton.Text = "M2TS (192 byte packets)";
            this.M2tsFormatRadioButton.UseVisualStyleBackColor = true;
            this.M2tsFormatRadioButton.CheckedChanged += new System.EventHandler(this.M2tsFormatRadioButton_CheckedChanged);
            // 
            // OuterPanel
            // 
            this.OuterPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.OuterPanel.Controls.Add(this.MPlayerPreview);
            this.OuterPanel.Location = new System.Drawing.Point(16, 180);
            this.OuterPanel.Name = "OuterPanel";
            this.OuterPanel.Size = new System.Drawing.Size(583, 383);
            this.OuterPanel.TabIndex = 43;
            // 
            // MPlayerPreview
            // 
            this.MPlayerPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.MPlayerPreview.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MPlayerPreview.Location = new System.Drawing.Point(3, 3);
            this.MPlayerPreview.Name = "MPlayerPreview";
            this.MPlayerPreview.Size = new System.Drawing.Size(573, 373);
            this.MPlayerPreview.TabIndex = 42;
            this.MPlayerPreview.TabStop = false;
            // 
            // ControlPanel
            // 
            this.ControlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ControlPanel.Controls.Add(this.CurPlayPos);
            this.ControlPanel.Controls.Add(this.LargeForward);
            this.ControlPanel.Controls.Add(this.MediumForward);
            this.ControlPanel.Controls.Add(this.SmallForward);
            this.ControlPanel.Controls.Add(this.SmallBack);
            this.ControlPanel.Controls.Add(this.FrameStep);
            this.ControlPanel.Controls.Add(this.MediumBack);
            this.ControlPanel.Controls.Add(this.LargeBack);
            this.ControlPanel.Controls.Add(this.PlayPause);
            this.ControlPanel.Location = new System.Drawing.Point(16, 614);
            this.ControlPanel.Name = "ControlPanel";
            this.ControlPanel.Size = new System.Drawing.Size(583, 33);
            this.ControlPanel.TabIndex = 44;
            // 
            // CurPlayPos
            // 
            this.CurPlayPos.AutoSize = true;
            this.CurPlayPos.Enabled = false;
            this.CurPlayPos.Location = new System.Drawing.Point(486, 10);
            this.CurPlayPos.Name = "CurPlayPos";
            this.CurPlayPos.Size = new System.Drawing.Size(94, 13);
            this.CurPlayPos.TabIndex = 60;
            this.CurPlayPos.Text = "Pos: 00:00:00:000";
            // 
            // LargeForward
            // 
            this.LargeForward.Enabled = false;
            this.LargeForward.Location = new System.Drawing.Point(430, 5);
            this.LargeForward.Name = "LargeForward";
            this.LargeForward.Size = new System.Drawing.Size(50, 23);
            this.LargeForward.TabIndex = 59;
            this.LargeForward.Text = ">>>";
            this.LargeForward.UseVisualStyleBackColor = true;
            this.LargeForward.Click += new System.EventHandler(this.LargeForward_Click);
            // 
            // MediumForward
            // 
            this.MediumForward.Enabled = false;
            this.MediumForward.Location = new System.Drawing.Point(374, 5);
            this.MediumForward.Name = "MediumForward";
            this.MediumForward.Size = new System.Drawing.Size(50, 23);
            this.MediumForward.TabIndex = 58;
            this.MediumForward.Text = ">>";
            this.MediumForward.UseVisualStyleBackColor = true;
            this.MediumForward.Click += new System.EventHandler(this.MediumForward_Click);
            // 
            // SmallForward
            // 
            this.SmallForward.Enabled = false;
            this.SmallForward.Location = new System.Drawing.Point(318, 5);
            this.SmallForward.Name = "SmallForward";
            this.SmallForward.Size = new System.Drawing.Size(50, 23);
            this.SmallForward.TabIndex = 57;
            this.SmallForward.Text = ">";
            this.SmallForward.UseVisualStyleBackColor = true;
            this.SmallForward.Click += new System.EventHandler(this.SmallForward_Click);
            // 
            // SmallBack
            // 
            this.SmallBack.Enabled = false;
            this.SmallBack.Location = new System.Drawing.Point(262, 5);
            this.SmallBack.Name = "SmallBack";
            this.SmallBack.Size = new System.Drawing.Size(50, 23);
            this.SmallBack.TabIndex = 56;
            this.SmallBack.Text = "<";
            this.SmallBack.UseVisualStyleBackColor = true;
            this.SmallBack.Click += new System.EventHandler(this.SmallBack_Click);
            // 
            // FrameStep
            // 
            this.FrameStep.Enabled = false;
            this.FrameStep.Location = new System.Drawing.Point(87, 5);
            this.FrameStep.Name = "FrameStep";
            this.FrameStep.Size = new System.Drawing.Size(58, 23);
            this.FrameStep.TabIndex = 55;
            this.FrameStep.Text = "Frame+";
            this.FrameStep.UseVisualStyleBackColor = true;
            this.FrameStep.Click += new System.EventHandler(this.FrameStep_Click);
            // 
            // MediumBack
            // 
            this.MediumBack.Enabled = false;
            this.MediumBack.Location = new System.Drawing.Point(206, 5);
            this.MediumBack.Name = "MediumBack";
            this.MediumBack.Size = new System.Drawing.Size(50, 23);
            this.MediumBack.TabIndex = 54;
            this.MediumBack.Text = "<<";
            this.MediumBack.UseVisualStyleBackColor = true;
            this.MediumBack.Click += new System.EventHandler(this.MediumBack_Click);
            // 
            // LargeBack
            // 
            this.LargeBack.Enabled = false;
            this.LargeBack.Location = new System.Drawing.Point(151, 5);
            this.LargeBack.Name = "LargeBack";
            this.LargeBack.Size = new System.Drawing.Size(49, 23);
            this.LargeBack.TabIndex = 53;
            this.LargeBack.Text = "<<<";
            this.LargeBack.UseVisualStyleBackColor = true;
            this.LargeBack.Click += new System.EventHandler(this.LargeBack_Click);
            // 
            // PlayPause
            // 
            this.PlayPause.Enabled = false;
            this.PlayPause.Location = new System.Drawing.Point(6, 5);
            this.PlayPause.Name = "PlayPause";
            this.PlayPause.Size = new System.Drawing.Size(75, 23);
            this.PlayPause.TabIndex = 52;
            this.PlayPause.Text = "Play/Pause";
            this.PlayPause.UseVisualStyleBackColor = true;
            this.PlayPause.Click += new System.EventHandler(this.PlayPause_Click);
            // 
            // LogBox
            // 
            this.LogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LogBox.BackColor = System.Drawing.Color.Black;
            this.LogBox.Enabled = false;
            this.LogBox.Font = new System.Drawing.Font("Courier New", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogBox.ForeColor = System.Drawing.Color.White;
            this.LogBox.Location = new System.Drawing.Point(16, 653);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogBox.Size = new System.Drawing.Size(583, 80);
            this.LogBox.TabIndex = 45;
            // 
            // PlaybackPos
            // 
            this.PlaybackPos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PlaybackPos.Enabled = false;
            this.PlaybackPos.Location = new System.Drawing.Point(16, 569);
            this.PlaybackPos.Name = "PlaybackPos";
            this.PlaybackPos.Size = new System.Drawing.Size(582, 45);
            this.PlaybackPos.TabIndex = 46;
            this.PlaybackPos.Scroll += new System.EventHandler(this.PlaybackPos_Scroll);
            // 
            // TsRemux
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 746);
            this.Controls.Add(this.PlaybackPos);
            this.Controls.Add(this.LogBox);
            this.Controls.Add(this.ControlPanel);
            this.Controls.Add(this.OuterPanel);
            this.Controls.Add(this.SidePanel);
            this.Controls.Add(this.ElementaryStreamsListBox);
            this.Controls.Add(this.OutputFileBrowseButton);
            this.Controls.Add(this.OutputFileTextBox);
            this.Controls.Add(this.ElementaryStreamsToKeep);
            this.Controls.Add(this.OutputFileLabel);
            this.Controls.Add(this.InputFileTextBox);
            this.Controls.Add(this.InputFileBrowseButton);
            this.Controls.Add(this.InputFileLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(877, 784);
            this.Name = "TsRemux";
            this.Text = "TsRemux";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.TsRemux_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.TsRemux_DragEnter);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TsRemux_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.SupOffsetNumericSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SupOffsetNumericMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SupOffsetNumericHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ChapterLengthUpDown)).EndInit();
            this.SidePanel.ResumeLayout(false);
            this.SidePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrimEndNumericSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimEndNumericMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimEndNumericHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimStartNumericSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimStartNumericMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrimStartNumericHours)).EndInit();
            this.FormatBox.ResumeLayout(false);
            this.FormatBox.PerformLayout();
            this.OuterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MPlayerPreview)).EndInit();
            this.ControlPanel.ResumeLayout(false);
            this.ControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PlaybackPos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label InputFileLabel;
        private System.Windows.Forms.TextBox InputFileTextBox;
        private System.Windows.Forms.Button InputFileBrowseButton;
        private System.Windows.Forms.Button OutputFileBrowseButton;
        private System.Windows.Forms.TextBox OutputFileTextBox;
        private System.Windows.Forms.Label OutputFileLabel;
        private System.Windows.Forms.OpenFileDialog OpenInputFileDialog;
        private System.Windows.Forms.SaveFileDialog OpenOutputFileDialog;
        private System.Windows.Forms.Label ElementaryStreamsToKeep;
        private System.Windows.Forms.CheckedListBox ElementaryStreamsListBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ContextMenuStrip ElementaryContextMenu;
        private System.Windows.Forms.SaveFileDialog SaveElementaryStream;
        private System.Windows.Forms.CheckBox cbxUseAsyncIO;
        private System.Windows.Forms.OpenFileDialog OpenSupFileDialog;
        private System.Windows.Forms.NumericUpDown SupOffsetNumericSeconds;
        private System.Windows.Forms.NumericUpDown SupOffsetNumericMinutes;
        private System.Windows.Forms.Label SupBegin;
        private System.Windows.Forms.NumericUpDown SupOffsetNumericHours;
        private System.Windows.Forms.NumericUpDown ChapterLengthUpDown;
        private System.Windows.Forms.Label ChapterLengthMinutesLabel;
        private System.Windows.Forms.Label label1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.Windows.Forms.CheckBox cbxBypassAudioProcessing;
        private System.Windows.Forms.CheckBox cbxMlpToAc3;
        private System.Windows.Forms.FolderBrowserDialog SelectOutputFolderDialog;
        private System.Windows.Forms.Timer TrimSetTimer;
        private System.Windows.Forms.Panel SidePanel;
        private System.Windows.Forms.Button TrimEndSet;
        private System.Windows.Forms.Button TrimStartSet;
        private System.Windows.Forms.NumericUpDown TrimEndNumericSeconds;
        private System.Windows.Forms.NumericUpDown TrimEndNumericMinutes;
        private System.Windows.Forms.Label TrimEndLabel;
        private System.Windows.Forms.NumericUpDown TrimEndNumericHours;
        private System.Windows.Forms.NumericUpDown TrimStartNumericSeconds;
        private System.Windows.Forms.NumericUpDown TrimStartNumericMinutes;
        private System.Windows.Forms.Label TrimStartLabel;
        private System.Windows.Forms.NumericUpDown TrimStartNumericHours;
        private System.Windows.Forms.TextBox RemuxProgressTimeTextBox;
        private System.Windows.Forms.Button Quit;
        private System.Windows.Forms.Button RemuxButton;
        private System.Windows.Forms.Label RemuxProgressLabel;
        private System.Windows.Forms.ProgressBar RemuxProgressBar;
        private System.Windows.Forms.Label InputInfoLabel;
        private System.Windows.Forms.TreeView DtcpInfo;
        private System.Windows.Forms.GroupBox FormatBox;
        private System.Windows.Forms.RadioButton BluRayFormatRadioButton;
        private System.Windows.Forms.RadioButton TsFormatRadioButton;
        private System.Windows.Forms.RadioButton M2tsFormatRadioButton;
        private System.Windows.Forms.Panel OuterPanel;
        private System.Windows.Forms.PictureBox MPlayerPreview;
        private System.Windows.Forms.Panel ControlPanel;
        private System.Windows.Forms.Button LargeForward;
        private System.Windows.Forms.Button MediumForward;
        private System.Windows.Forms.Button SmallForward;
        private System.Windows.Forms.Button SmallBack;
        private System.Windows.Forms.Button FrameStep;
        private System.Windows.Forms.Button MediumBack;
        private System.Windows.Forms.Button LargeBack;
        private System.Windows.Forms.Button PlayPause;
        private System.Windows.Forms.Label CurPlayPos;
        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.TrackBar PlaybackPos;
        private System.Windows.Forms.RadioButton MKVFormatRadioButton;
        private System.Windows.Forms.RadioButton DemuxFormatRadioButton;
    }
}

