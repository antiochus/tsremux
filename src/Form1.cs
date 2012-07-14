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
 * MODIFIED 04/14/2008 by spacecat56: 
 *    Use a folder browser dialog for blu-ray output; interlock with user changes to selected output type.
 *    Not allow repeated remux without re-opening the file (some side effect causes 10x slowing on repeated remux).
 *    Show 4 places of version.
 *    Minor refactoring and added methods to support command-line execution.
 *    
 * MODIFIED 04/16/2010 by hobBIT:
 *    Add option to use MPlayer to set trim points visually.
 *    
 * MODIFIED 04/19/2010 by hobBIT: 
 *    MPlayer integration, UI revamp, file filter for dialogs added (configurable).
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

using System.Diagnostics;
using System.Threading;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections;

namespace TsRemux
{
    public enum SaveState
    {
        Remux,
        DemuxSup,
        DemuxStream,
        DemuxPes
    }

    public partial class TsRemux : Form
    {
        private PesFile inFile;
        TimeSpan length;
        List<ushort> pidList;
        List<ushort> pidsToKeep;
        SaveState state;
        string elmName;
        ushort elmPid;
        bool supPresent;
        PesFile supFile;
        DateTime whenStarted = new DateTime();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
        string[] args;
        frmConsole con = null;
        bool keepConOpen = false;

        public TsRemux()
        {
            init();
        }

        public TsRemux(string[] args)
        {
            this.args = args;
            this.Visible = false;
            init();

            try
            {
                // only works with XP
                AttachConsole(ATTACH_PARENT_PROCESS);
            }
            catch (Exception)
            {
                // use our own form
                con = new frmConsole();
                Console.SetOut(con.ConWriter);
                con.Show();
            }
        }

        public Form exec()
        {
            Console.WriteLine();
            Console.WriteLine(this.Text);
            if (args.Length < 2)
            {
                Console.WriteLine("usage: tsremux input-file output-path [-a] [+b] [+m] [+c]");
                Console.WriteLine("   -a: do not use async io (default on)");
                Console.WriteLine("   +b: bypass audio alignment (default off)");
                Console.WriteLine("   +m: trueHd to ac3 (default off)");
                Console.WriteLine("   +c: keep console open when done (win2k)");
                Console.WriteLine("   output extension controls processing:");
                Console.WriteLine("       ts, m2ts, none for bluray directory");
                return con;
            }
            this.InputFileTextBox.Text = args[0];
            this.cbxUseAsyncIO.Checked = true;
            this.cbxBypassAudioProcessing.Checked = false;
            this.cbxMlpToAc3.Checked = false;
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].Equals("-a"))
                    this.cbxUseAsyncIO.Checked = false;
                else if (args[i].Equals("+b"))
                    this.cbxBypassAudioProcessing.Checked = true;
                else if (args[i].Equals("+c"))
                    this.keepConOpen = true;
                else if (args[i].Equals("+m"))
                    this.cbxMlpToAc3.Checked = true;
                else
                {
                    Console.WriteLine("unrecognized option: " + args[i]);
                    return con;
                }
            }

            // pre-process the input file
            say("pre-processing input file: " + args[0]);
            OpenFile(args[0]);

            // set the processing and output options
            this.OutputFileTextBox.Text = args[1];
            int ix = args[1].LastIndexOf(".")+1;
            string ext = (ix > 1) ? args[1].Substring(ix).ToLower() : "";
            if (ext.Equals("m2ts"))
            {
                this.M2tsFormatRadioButton.Checked = true;
            }
            else if (ext.Equals("ts"))
            {
                this.TsFormatRadioButton.Checked = true;
            }
            else if (ext.Equals(""))
            {
                this.BluRayFormatRadioButton.Checked = true;
            }
            else
            {
                System.Console.WriteLine("unrecognized output type");
                return con;
            }
            this.RemuxButton.Enabled = true;
            foreach (StreamInfo si in inFile.StreamInfos)
                pidsToKeep.Add(si.ElementaryPID);

            say("begin remuxing to: " + args[1]);
            backgroundWorker1_DoWork(this, null);
            //RemuxButton_Click(this, null);
            say("done remuxing");
            return (keepConOpen)?con:null;
        }

        private void say(string txt)
        {
            System.Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd.HH:mm:ss.fff ") + txt);
        }

        private void init()
        {
            InitializeComponent();
            inFile = null;
            length = new TimeSpan();
            pidList = new List<ushort>();
            pidsToKeep = new List<ushort>();
            OutputFileBrowseButton.Enabled = false;
            RemuxButton.Enabled = false;
            OpenOutputFileDialog.OverwritePrompt = false;
            DisableNumerics();
            RemuxButton.Text = "Remux";
            AssemblyName an = Assembly.GetExecutingAssembly().GetName();
            this.Text = an.Name + " v." + an.Version.ToString(4) + " by dmz, modified by spacecat56 and hobBIT";
            state = SaveState.Remux;
            elmName = string.Empty;
            elmPid = 0;
            supPresent = false;
            supFile = null;

            // hobBIT
            _bAutoFilled = false;
            _LogAddons = new ArrayList();
        }

        private void DisableNumerics()
        {
            TrimEndNumericHours.Enabled = false;
            TrimEndNumericMinutes.Enabled = false;
            TrimEndNumericSeconds.Enabled = false;
            TrimStartNumericHours.Enabled = false;
            TrimStartNumericMinutes.Enabled = false;
            TrimStartNumericSeconds.Enabled = false;
            ChapterLengthUpDown.Enabled = false;

            DisableSup();

            // hobBIT
            TrimStartSet.Enabled = false;
            TrimEndSet.Enabled = false;
        }

        private void DisableSup()
        {
            SupOffsetNumericHours.Enabled = false;
            SupOffsetNumericMinutes.Enabled = false;
            SupOffsetNumericSeconds.Enabled = false;
        }

        private void EnableSup()
        {
            SupOffsetNumericHours.Enabled = true;
            SupOffsetNumericMinutes.Enabled = true;
            SupOffsetNumericSeconds.Enabled = true;
        }

        private void EnableNumerics()
        {
            TrimEndNumericHours.Enabled = true;
            TrimEndNumericMinutes.Enabled = true;
            TrimEndNumericSeconds.Enabled = true;
            TrimStartNumericHours.Enabled = true;
            TrimStartNumericMinutes.Enabled = true;
            TrimStartNumericSeconds.Enabled = true;
            if (supPresent)
                EnableSup();
            if (BluRayFormatRadioButton.Checked)
                ChapterLengthUpDown.Enabled = true;

            // hobBIT
            TrimStartSet.Enabled = true;
            TrimEndSet.Enabled = true;
        }

        private void OpenFile(string filename)
        {
            if (null != inFile)
            {
                inFile.CloseFile();
                inFile = null;
            }
            if (null != supFile)
            {
                supFile.CloseFile();
                supFile = null;
            }
            inFile = PesFile.OpenFile(filename, cbxUseAsyncIO.Checked, backgroundWorker2);
        }

        private void OpenInputFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            AsyncOpenFile(OpenInputFileDialog.FileName);
        }

        private void AsyncOpenFile(string filename)
        {
            // hobBIT
            StopMPlayer();
            StartMPlayer(filename);

            RemuxProgressBar.Maximum = 100;
            RemoveSup();
            OutputFileBrowseButton.Enabled = false;
            RemuxButton.Text = "Cancel";
            RemuxButton.Enabled = true;
            Quit.Enabled = false;
            cbxUseAsyncIO.Enabled = false;
            cbxBypassAudioProcessing.Enabled = false;
            cbxMlpToAc3.Enabled = false;
            DisableNumerics();
            TrimEnd = new TimeSpan(0);
            TrimStart = new TimeSpan(0);
            ElementaryStreamsListBox.Items.Clear();
            ElementaryStreamsListBox.Enabled = false;
            InputFileTextBox.Text = String.Empty;
            DtcpInfo.Nodes.Clear();
            DtcpInfo.Nodes.Add("DTCP Info");
            DtcpInfo.Nodes[0].Nodes.Add("DTCP Descriptor not present");
            TsFormatRadioButton.Enabled = false;
            TsFormatRadioButton.Checked = false;
            M2tsFormatRadioButton.Enabled = false;
            M2tsFormatRadioButton.Checked = false;
            BluRayFormatRadioButton.Enabled = false;
            BluRayFormatRadioButton.Checked = false;
            MKVFormatRadioButton.Enabled = false;
            MKVFormatRadioButton.Checked = false;
            DemuxFormatRadioButton.Enabled = false;
            DemuxFormatRadioButton.Checked = false;
            length = new TimeSpan();
            pidList = new List<ushort>();
            pidsToKeep = new List<ushort>();
            this.Cursor = Cursors.WaitCursor;
            backgroundWorker2.RunWorkerAsync(filename);
        }

        private void InputFileBrowseButton_Click(object sender, EventArgs e)
        {
            // hobBIT
            AppSettingsReader Config = new AppSettingsReader();
            OpenInputFileDialog.Filter = (string)Config.GetValue("FileFilter", typeof(string));
            OpenInputFileDialog.FileName = InputFileTextBox.Text;
            OpenInputFileDialog.ShowDialog();
        }

        private void OpenOutputFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (string.CompareOrdinal(InputFileTextBox.Text, OpenOutputFileDialog.FileName) == 0)
            {
                OutputFileTextBox.Text = String.Empty;
                MessageBox.Show("Cannot open the same file as a source and an output file");
            }
            else
            {
                OutputFileTextBox.Text = OpenOutputFileDialog.FileName;
                RemuxButton.Enabled = true;
            }
        }

        private void OutputFileBrowseButton_Click(object sender, EventArgs e)
        {
            if (BluRayFormatRadioButton.Checked)
            {
                if (OutputFileTextBox.Text.LastIndexOf("\\") > 0)
                {
                    SelectOutputFolderDialog.SelectedPath = OutputFileTextBox.Text.Substring(0, OutputFileTextBox.Text.LastIndexOf("\\"));
                }
                DialogResult dr = SelectOutputFolderDialog.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    OutputFileTextBox.Text = SelectOutputFolderDialog.SelectedPath;
                    RemuxButton.Enabled = true;
                }
            }
            else
            {
                // hobBIT
                AppSettingsReader Config = new AppSettingsReader();
                //OpenOutputFileDialog.Filter = (string)Config.GetValue("FileFilter", typeof(string));
                OpenOutputFileDialog.FileName = InputFileTextBox.Text;
                OpenOutputFileDialog.ShowDialog();
            }
        }

        private void EnableCbxMlp()
        {
            cbxMlpToAc3.Enabled = false;
            if(inFile.FileType == TsFileType.M2TS || inFile.FileType == TsFileType.TS)
            {
                foreach (StreamInfo si in inFile.StreamInfos)
                    if ((pidsToKeep.Contains(si.ElementaryPID) && si.StreamType == ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD)
                        || (pidsToKeep.Contains(si.ElementaryPID) && si.StreamType == ElementaryStreamTypes.AUDIO_STREAM_DTS_HD)
                        ||(pidsToKeep.Contains(si.ElementaryPID) && si.StreamType == ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO))
                        cbxMlpToAc3.Enabled = true;
            }
        }

        private void ElementaryStreamsListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            pidsToKeep.Clear();
            for (int i = 0; i < ElementaryStreamsListBox.Items.Count; i++)
            {
                if (i != e.Index)
                {
                    if (ElementaryStreamsListBox.CheckedItems.Contains(ElementaryStreamsListBox.Items[i]))
                    {
                        pidsToKeep.Add(pidList[i]);
                    }
                }
                else if (e.NewValue == CheckState.Checked)
                    pidsToKeep.Add(pidList[i]);
            }
            EnableCbxMlp();
        }

        private void Quit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RemuxButton_Click(object sender, EventArgs e)
        {
            if(0 == pidsToKeep.Count)
            {
                MessageBox.Show(
                    this, 
                    "No single stream to keep selected !", 
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return;
            }

            if (backgroundWorker1.IsBusy == false && backgroundWorker2.IsBusy == false)
            {
                SendMPlayerCmd("frame_step");

                RemuxButton.Text = "Cancel";
                InputFileBrowseButton.Enabled = false;
                OutputFileBrowseButton.Enabled = false;
                InputFileTextBox.Enabled = false;
                OutputFileTextBox.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                RemuxProgressTimeTextBox.Text = length.Subtract(TrimEnd).ToString();
                RemuxProgressBar.Value = RemuxProgressBar.Minimum;
                RemuxProgressBar.Maximum = (int)length.Subtract(TrimEnd).TotalMinutes;
                FormatBox.Enabled = false;
                TsFormatRadioButton.Enabled = false;
                M2tsFormatRadioButton.Enabled = false;
                BluRayFormatRadioButton.Enabled = false;
                MKVFormatRadioButton.Enabled = false;
                DemuxFormatRadioButton.Enabled = false;
                Quit.Enabled = false;
                cbxUseAsyncIO.Enabled = false;
                cbxBypassAudioProcessing.Enabled = false;
                cbxMlpToAc3.Enabled = false;
                ElementaryStreamsListBox.Enabled = false;
                DisableNumerics();
                whenStarted = DateTime.Now;
                backgroundWorker1.RunWorkerAsync();
            }
            else if (backgroundWorker1.IsBusy)
            {
                RemuxButton.Enabled = false;
                backgroundWorker1.CancelAsync();
            }
            else if (backgroundWorker2.IsBusy)
            {
                RemuxButton.Enabled = false;
                backgroundWorker2.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Coordinator cor = new Coordinator();
            switch (state)
            {
                case SaveState.Remux:
                    if (TsFormatRadioButton.Checked)
                    {
                        cor.StartMuxing(OutputFileTextBox.Text, backgroundWorker1, TsFileType.TS, pidsToKeep, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, cbxBypassAudioProcessing.Checked, cbxMlpToAc3.Checked, inFile, supFile, SupStart, ChapterLen);
                    }
                    else if (M2tsFormatRadioButton.Checked)
                    {
                        cor.StartMuxing(OutputFileTextBox.Text, backgroundWorker1, TsFileType.M2TS, pidsToKeep, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, cbxBypassAudioProcessing.Checked, cbxMlpToAc3.Checked, inFile, supFile, SupStart, ChapterLen);
                    }
                    else if (BluRayFormatRadioButton.Checked)
                    {
                        cor.StartMuxing(OutputFileTextBox.Text, backgroundWorker1, TsFileType.BLU_RAY, pidsToKeep, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, cbxBypassAudioProcessing.Checked, cbxMlpToAc3.Checked, inFile, supFile, SupStart, ChapterLen);
                    }
                    else if (MKVFormatRadioButton.Checked)
                    {
                        cor.StartMuxing(OutputFileTextBox.Text, backgroundWorker1, TsFileType.MKV, pidsToKeep, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, cbxBypassAudioProcessing.Checked, cbxMlpToAc3.Checked, inFile, supFile, SupStart, ChapterLen);
                    }
                    else if (DemuxFormatRadioButton.Checked)
                    {
                        cor.StartMuxing(OutputFileTextBox.Text, backgroundWorker1, TsFileType.DEMUX, pidsToKeep, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, cbxBypassAudioProcessing.Checked, cbxMlpToAc3.Checked, inFile, supFile, SupStart, ChapterLen);
                    } 
                    break;
                case SaveState.DemuxSup:
                    if (elmPid != 0)
                    {
                        List<ushort> elmPids = new List<ushort>();
                        elmPids.Add(elmPid);
                        cor.StartMuxing(elmName, backgroundWorker1, TsFileType.SUP_ELEMENTARY, elmPids, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, inFile);
                    }
                    break;
                case SaveState.DemuxStream:
                    if (elmPid != 0)
                    {
                        List<ushort> elmPids = new List<ushort>();
                        elmPids.Add(elmPid);
                        cor.StartMuxing(elmName, backgroundWorker1, TsFileType.ELEMENTARY, elmPids, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, inFile);
                    }
                    break;
                case SaveState.DemuxPes:
                    if (elmPid != 0)
                    {
                        List<ushort> elmPids = new List<ushort>();
                        elmPids.Add(elmPid);
                        cor.StartMuxing(elmName, backgroundWorker1, TsFileType.PES_ELEMENTARY, elmPids, TrimStart, TrimEnd, cbxUseAsyncIO.Checked, inFile);
                    }
                    break;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TimeSpan ts = (TimeSpan)e.UserState;
            TimeSpan nts = length - TrimEnd - ts;
            RemuxProgressTimeTextBox.Text = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            int newValue = (int)nts.TotalMinutes;
            if (newValue != RemuxProgressBar.Value && newValue > 0)
                RemuxProgressBar.Value = newValue;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (whenStarted.Ticks > 0)
            {
                TimeSpan ts = DateTime.Now.Subtract(whenStarted);
                this.RemuxProgressTimeTextBox.Text = "elapsed: " + ts.TotalSeconds.ToString("#.00") + " sec.";
            }
            RemuxButton.Text = "Remux";
            InputFileBrowseButton.Enabled = true;
            OutputFileBrowseButton.Enabled = true;
            InputFileTextBox.Enabled = true;
            OutputFileTextBox.Enabled = true;
            FormatBox.Enabled = true;
            TsFormatRadioButton.Enabled = true;
            MKVFormatRadioButton.Enabled = true;
            DemuxFormatRadioButton.Enabled = true;
            cbxUseAsyncIO.Enabled = true;
            cbxBypassAudioProcessing.Enabled = true;
            EnableCbxMlp();
            if (length > TimeSpan.Zero)
            {
                M2tsFormatRadioButton.Enabled = true;
                BluRayFormatRadioButton.Enabled = true;
            }
            Quit.Enabled = true;
            ElementaryStreamsListBox.Enabled = true;
            if (length > TimeSpan.Zero)
                EnableNumerics();
            this.Cursor = Cursors.Default;
            state = SaveState.Remux;
            elmName = String.Empty;
            elmPid = 0;
            /***********
             * some artifact of the remux process causes extreme performance degradation, IF
             * you just re-remux the same file over again.  re-opening the input seems to 
             * clear this condition.  So here we force user to do that in order to re-remux
             */
            if (InputFileTextBox.Text.Length > 0 && OutputFileTextBox.Text.Length > 0)
                RemuxButton.Enabled = true;
            else
                RemuxButton.Enabled = false;
            // * *********/
            //RemuxButton.Enabled = false;
            if (e.Error != null)
                MessageBox.Show(e.Error.Message + "\n" + e.Error.StackTrace);               
        }

        private void RemuxButton_MouseEnter(object sender, EventArgs e)
        {
            if ((backgroundWorker1.IsBusy && backgroundWorker1.CancellationPending == false) ||
                (backgroundWorker2.IsBusy && backgroundWorker1.CancellationPending == false))
                this.Cursor = Cursors.Default;
        }

        private void RemuxButton_MouseLeave(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy || backgroundWorker2.IsBusy)
                this.Cursor = Cursors.WaitCursor;
        }

        private void TsRemux_DragDrop(object sender, DragEventArgs e)
        {
            string[] formats = e.Data.GetFormats(false);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AsyncOpenFile(files[0]);
            }
        }

        private void TsRemux_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void TrimStartNumericHours_ValueChanged(object sender, EventArgs e)
        {
            CheckTrimStart();
        }

        private void TrimStartNumericMinutes_ValueChanged(object sender, EventArgs e)
        {
            if (TrimStartNumericMinutes.Value == 60)
            {
                if (TrimStartNumericHours.Value < TrimStartNumericHours.Maximum)
                {
                    TrimStartNumericMinutes.Value = 0;
                    TrimStartNumericHours.Value += 1;
                }
                else
                    TrimStartNumericMinutes.Value = 59;
            }
            else if (TrimStartNumericMinutes.Value == -1)
            {
                if (TrimStartNumericHours.Value > 0)
                {
                    TrimStartNumericMinutes.Value = 59;
                    TrimStartNumericHours.Value -= 1;
                }
                else
                    TrimStartNumericMinutes.Value = 0;
            }
            CheckTrimStart();
        }

        private void TrimStartNumericSeconds_ValueChanged(object sender, EventArgs e)
        {
            if (TrimStartNumericSeconds.Value == 60)
            {
                if (TrimStartNumericHours.Value == TrimStartNumericHours.Maximum && TrimStartNumericMinutes.Value == 59)
                {
                    TrimStartNumericSeconds.Value = 59;
                }
                else
                {
                    TrimStartNumericSeconds.Value = 0;
                    TrimStartNumericMinutes.Value += 1;
                }
            }
            else if (TrimStartNumericSeconds.Value == -1)
            {
                if (TrimStartNumericMinutes.Value > 0 || TrimStartNumericHours.Value > 0)
                {
                    TrimStartNumericSeconds.Value = 59;
                    TrimStartNumericMinutes.Value -= 1;
                }
                else
                    TrimStartNumericSeconds.Value = 0;
            }
            CheckTrimStart();
        }

        private void TrimEndNumericHours_ValueChanged(object sender, EventArgs e)
        {
            CheckTrimEnd();
        }

        private void TrimEndNumericMinutes_ValueChanged(object sender, EventArgs e)
        {
            if (TrimEndNumericMinutes.Value == 60)
            {
                if (TrimEndNumericHours.Value < TrimEndNumericHours.Maximum)
                {
                    TrimEndNumericMinutes.Value = 0;
                    TrimEndNumericHours.Value += 1;
                }
                else
                    TrimEndNumericMinutes.Value = 59;
            }
            else if (TrimEndNumericMinutes.Value == -1)
            {
                if (TrimEndNumericHours.Value > 0)
                {
                    TrimEndNumericMinutes.Value = 59;
                    TrimEndNumericHours.Value -= 1;
                }
                else
                    TrimEndNumericMinutes.Value = 0;
            }
            CheckTrimEnd();
        }

        private void TrimEndNumericSeconds_ValueChanged(object sender, EventArgs e)
        {
            if (TrimEndNumericSeconds.Value == 60)
            {
                if (TrimEndNumericHours.Value == TrimEndNumericHours.Maximum && TrimEndNumericMinutes.Value == 59)
                {
                    TrimEndNumericSeconds.Value = 59;
                }
                else
                {
                    TrimEndNumericSeconds.Value = 0;
                    TrimEndNumericMinutes.Value += 1;
                }
            }
            else if (TrimEndNumericSeconds.Value == -1)
            {
                if (TrimEndNumericMinutes.Value > 0 || TrimEndNumericHours.Value > 0)
                {
                    TrimEndNumericSeconds.Value = 59;
                    TrimEndNumericMinutes.Value -= 1;
                }
                else
                    TrimEndNumericSeconds.Value = 0;
            }
            CheckTrimEnd();
        }

        private void CheckTrimStart()
        {
            // hobBIT 
            if (_bAutoFilled) return; // Don't fix if filled by MPlayer

            TimeSpan total = TrimStart.Add(TrimEnd);
            if (total.CompareTo(length) >= 0)
            {
                TimeSpan newEnd = length.Subtract(TrimStart);
                if (newEnd.TotalSeconds <= 0)
                {
                    TrimStart = new TimeSpan(length.Hours, length.Minutes, length.Seconds);
                    TrimEnd = new TimeSpan(0, 0, 0);
                }
                else
                    TrimEnd = new TimeSpan(newEnd.Hours, newEnd.Minutes, newEnd.Seconds);
            }
            CheckSupStart();
            CheckChapterLen();
        }

        private void CheckSupStart()
        {
            TimeSpan total = TrimStart.Add(TrimEnd);
            total = length.Subtract(total);
            if (SupStart.CompareTo(total) >= 0)
            {
                SupStart = total;
            }
        }

        private void CheckChapterLen()
        {
            TimeSpan total = TrimStart.Add(TrimEnd);
            total = length.Subtract(total);
            if (ChapterLen.CompareTo(total) >= 0)
            {
                ChapterLen = total;
            }
        }

        private void CheckTrimEnd()
        {
            // hobBIT 
            if (_bAutoFilled) return; // Don't fix if filled by MPlayer

            TimeSpan total = TrimEnd.Add(TrimStart);
            if (total.CompareTo(length) >= 0)
            {
                TimeSpan newStart = length.Subtract(TrimEnd);
                if (newStart.TotalSeconds <= 0)
                {
                    TrimEnd = new TimeSpan(length.Hours, length.Minutes, length.Seconds);
                    TrimStart = new TimeSpan(0, 0, 0);
                }
                else
                    TrimStart = new TimeSpan(newStart.Hours, newStart.Minutes, newStart.Seconds);
            }
            CheckSupStart();
            CheckChapterLen();
        }

        private TimeSpan TrimStart
        {
            get
            {
                return new TimeSpan((int)TrimStartNumericHours.Value, (int)TrimStartNumericMinutes.Value, (int)TrimStartNumericSeconds.Value);
            }
            set
            {
                TrimStartNumericHours.Value = value.Hours;
                TrimStartNumericMinutes.Value = value.Minutes;
                TrimStartNumericSeconds.Value = value.Seconds;
            }
        }

        private TimeSpan SupStart
        {
            get
            {
                return new TimeSpan((int)SupOffsetNumericHours.Value, (int)SupOffsetNumericMinutes.Value, (int)SupOffsetNumericSeconds.Value);
            }
            set
            {
                SupOffsetNumericHours.Value = value.Hours;
                SupOffsetNumericMinutes.Value = value.Minutes;
                SupOffsetNumericSeconds.Value = value.Seconds;
            }
        }

        private TimeSpan ChapterLen
        {
            get
            {
                return new TimeSpan((int)ChapterLengthUpDown.Value / 60, (int)ChapterLengthUpDown.Value % 60, 0);
            }
            set
            {
                int len = (int)value.TotalMinutes;
                if (len >= ChapterLengthUpDown.Minimum)
                    ChapterLengthUpDown.Value = len;
                else
                    ChapterLengthUpDown.Value = ChapterLengthUpDown.Minimum;
            }
        }

        private TimeSpan TrimEnd
        {
            get
            {
                return new TimeSpan((int)TrimEndNumericHours.Value, (int)TrimEndNumericMinutes.Value, (int)TrimEndNumericSeconds.Value);
            }
            set
            {
                TrimEndNumericHours.Value = value.Hours;
                TrimEndNumericMinutes.Value = value.Minutes;
                TrimEndNumericSeconds.Value = value.Seconds;
            }
        }

        private bool SupSelected
        {
            get
            {
                return (ElementaryStreamsListBox.SelectedIndex == ElementaryStreamsListBox.Items.Count - 1) && supPresent;
            }
        }

        private void ElementaryContextMenu_Opening(object sender, CancelEventArgs e)
        {
            ElementaryContextMenu.Items.Clear();
            e.Cancel = false;
            ToolStripLabel tl = null;
            if (supPresent)
                tl = new ToolStripLabel("Remove SUPread stream", null, false, RemoveStream_Click);
            else
                tl = new ToolStripLabel("Add a new SUPread stream", null, false, AddStream_Click);
            tl.MouseEnter += new EventHandler(tl_MouseEnter);
            tl.MouseLeave += new EventHandler(tl_MouseLeave);
            ElementaryContextMenu.Items.Add(tl);
            ElementaryContextMenu.Items.Add(new ToolStripSeparator());
            if (null != ElementaryStreamsListBox.SelectedItem && SupSelected == false)
            {
                tl = new ToolStripLabel(String.Format("Demux {0} to elementary stream", ElementaryStreamsListBox.SelectedItem.ToString()), null, false, DemuxElementary_Click);
                tl.MouseEnter += new EventHandler(tl_MouseEnter);
                tl.MouseLeave += new EventHandler(tl_MouseLeave);
                ElementaryContextMenu.Items.Add(tl);
                tl = new ToolStripLabel(String.Format("Demux {0} to PES stream", ElementaryStreamsListBox.SelectedItem.ToString()), null, false, DemuxPes_Click);
                tl.MouseEnter += new EventHandler(tl_MouseEnter);
                tl.MouseLeave += new EventHandler(tl_MouseLeave);
                ElementaryContextMenu.Items.Add(tl);
                ushort pid = pidList[ElementaryStreamsListBox.Items.IndexOf(ElementaryStreamsListBox.SelectedItem)];
                foreach (StreamInfo si in inFile.StreamInfos)
                {
                    if (si.ElementaryPID == pid)
                    {
                        if (si.StreamType == ElementaryStreamTypes.PRESENTATION_GRAPHICS_STREAM)
                        {
                            tl = new ToolStripLabel(String.Format("Demux {0} to SUPread stream", ElementaryStreamsListBox.SelectedItem.ToString()), null, false, DemuxSup_Click);
                            ElementaryContextMenu.Items.Add(tl);
                        }
                        break;
                    }
                }
            }
        }

        void tl_MouseLeave(object sender, EventArgs e)
        {
            ((ToolStripLabel)sender).ForeColor = Color.Black;
        }

        void tl_MouseEnter(object sender, EventArgs e)
        {
            ((ToolStripLabel)sender).ForeColor = Color.Aqua;
        }

        private void AddStream_Click(object sender, EventArgs e)
        {
            OpenSupFileDialog.ShowDialog();
        }

        private void RemoveStream_Click(object sender, EventArgs e)
        {
            RemoveSup();
        }

        private void DemuxElementary_Click(object sender, EventArgs e)
        {
            state = SaveState.DemuxStream;
            SaveElementaryStream.ShowDialog();
        }

        private void DemuxPes_Click(object sender, EventArgs e)
        {
            state = SaveState.DemuxPes;
            SaveElementaryStream.ShowDialog();
        }

        private void DemuxSup_Click(object sender, EventArgs e)
        {
            state = SaveState.DemuxSup;
            SaveElementaryStream.ShowDialog();
        }

        private void SaveElementaryStream_FileOk(object sender, CancelEventArgs e)
        {
            if (backgroundWorker1.IsBusy == false)
            {
                elmName = SaveElementaryStream.FileName;
                elmPid = pidList[ElementaryStreamsListBox.Items.IndexOf(ElementaryStreamsListBox.SelectedItem)];
                RemuxButton.Text = "Cancel";
                RemuxButton.Enabled = true;
                InputFileBrowseButton.Enabled = false;
                OutputFileBrowseButton.Enabled = false;
                InputFileTextBox.Enabled = false;
                OutputFileTextBox.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                RemuxProgressTimeTextBox.Text = length.Subtract(TrimEnd).ToString();
                RemuxProgressBar.Value = RemuxProgressBar.Minimum;
                RemuxProgressBar.Maximum = (int)length.Subtract(TrimEnd).TotalMinutes;
                FormatBox.Enabled = false;
                TsFormatRadioButton.Enabled = false;
                M2tsFormatRadioButton.Enabled = false;
                BluRayFormatRadioButton.Enabled = false;
                MKVFormatRadioButton.Enabled = false;
                DemuxFormatRadioButton.Enabled = false;
                Quit.Enabled = false;
                ElementaryStreamsListBox.Enabled = false;
                DisableNumerics();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void RemoveSup()
        {
            if (supPresent)
            {
                pidList.RemoveAt(pidList.Count - 1);
                ElementaryStreamsListBox.Items.RemoveAt(ElementaryStreamsListBox.Items.Count - 1);
                pidsToKeep.Remove(supFile.StreamInfos[0].ElementaryPID);
                supPresent = false;
                supFile.CloseFile();
                supFile = null;
            }
            SupStart = TimeSpan.Zero;
            DisableSup();
        }

        private void OpenSupFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            RemoveSup();

            supFile = PesFile.OpenFile(OpenSupFileDialog.FileName, cbxUseAsyncIO.Checked, backgroundWorker2);
            if (supFile.FileType != TsFileType.SUP_ELEMENTARY)
            {
                MessageBox.Show(String.Format("File \"{0}\" is not a valid SUP file.", OpenSupFileDialog.FileName));
                return;
            }

            ushort pid = Constants.DEFAULT_PRESENTATION_GRAPHICS_PID;
            while (PidExists(pid))
                pid++;
            supFile.StreamInfos[0].ElementaryPID = pid;
            pidList.Add(pid);
            ElementaryStreamsListBox.Items.Add(String.Format("Presentation Graphics Stream # {0}", supFile.StreamInfos[0].ElementaryPID & 0xf));
            ElementaryStreamsListBox.SetItemChecked(ElementaryStreamsListBox.Items.Count - 1, true);
            EnableSup();
            supPresent = true;
        }

        private bool PidExists(ushort pid)
        {
            foreach (StreamInfo si in inFile.StreamInfos)
                if (si.ElementaryPID == pid)
                    return true;
            return false;
        }

        private void SupOffsetNumericSeconds_ValueChanged(object sender, EventArgs e)
        {
            if (SupOffsetNumericSeconds.Value == 60)
            {
                if (SupOffsetNumericHours.Value == SupOffsetNumericHours.Maximum && SupOffsetNumericMinutes.Value == 59)
                {
                    SupOffsetNumericSeconds.Value = 59;
                }
                else
                {
                    SupOffsetNumericSeconds.Value = 0;
                    SupOffsetNumericMinutes.Value += 1;
                }
            }
            else if (SupOffsetNumericSeconds.Value == -1)
            {
                if (SupOffsetNumericMinutes.Value > 0 || SupOffsetNumericHours.Value > 0)
                {
                    SupOffsetNumericSeconds.Value = 59;
                    SupOffsetNumericMinutes.Value -= 1;
                }
                else
                    SupOffsetNumericSeconds.Value = 0;
            }
            CheckSupStart();
            CheckChapterLen();
        }

        private void SupOffsetNumericMinutes_ValueChanged(object sender, EventArgs e)
        {
            if (SupOffsetNumericMinutes.Value == 60)
            {
                if (SupOffsetNumericHours.Value < SupOffsetNumericHours.Maximum)
                {
                    SupOffsetNumericMinutes.Value = 0;
                    SupOffsetNumericHours.Value += 1;
                }
                else
                    SupOffsetNumericMinutes.Value = 59;
            }
            else if (SupOffsetNumericMinutes.Value == -1)
            {
                if (SupOffsetNumericHours.Value > 0)
                {
                    SupOffsetNumericMinutes.Value = 59;
                    SupOffsetNumericHours.Value -= 1;
                }
                else
                    SupOffsetNumericMinutes.Value = 0;
            }
            CheckSupStart();
            CheckChapterLen();
        }

        private void SupOffsetNumericHours_ValueChanged(object sender, EventArgs e)
        {
            CheckSupStart();
        }

        private void ChapterLengthUpDown_ValueChanged(object sender, EventArgs e)
        {
            CheckChapterLen();
        }

        private void BluRayFormatRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (BluRayFormatRadioButton.Checked)
            {
                ChapterLengthUpDown.Enabled = true;
                if (inFile.FileType == TsFileType.M2TS)
                    cbxBypassAudioProcessing.Checked = true;
                else
                    cbxBypassAudioProcessing.Checked = false;
                if (!this.RemuxButton.Enabled)
                {
                    if (this.OutputFileTextBox.Text.LastIndexOf(".") < this.OutputFileTextBox.Text.LastIndexOf("\\"))
                        this.RemuxButton.Enabled = true;
                }
                else
                {
                    if (this.OutputFileTextBox.Text.LastIndexOf(".") >= this.OutputFileTextBox.Text.LastIndexOf("\\"))
                        this.RemuxButton.Enabled = false;
                }
            }
            else
            {
                ChapterLengthUpDown.Enabled = false;
                // don't allow folder for other options
                if (this.RemuxButton.Enabled)
                {
                    if (this.OutputFileTextBox.Text.LastIndexOf(".") < this.OutputFileTextBox.Text.LastIndexOf("\\"))
                        this.RemuxButton.Enabled = false;
                }
                else
                {
                    if (this.OutputFileTextBox.Text.LastIndexOf(".") >= this.OutputFileTextBox.Text.LastIndexOf("\\"))
                        this.RemuxButton.Enabled = true;
                }
            }
        }

        private void TsFormatRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (TsFormatRadioButton.Checked)
                cbxBypassAudioProcessing.Checked = true;
        }

        private void M2tsFormatRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (M2tsFormatRadioButton.Checked)
                cbxBypassAudioProcessing.Checked = true;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                OpenFile((string)e.Argument);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
            e.Result = e.Argument;
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RemuxProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
            RemuxButton.Text = "Remux";
            RemuxButton.Enabled = false;
            Quit.Enabled = true;
            cbxUseAsyncIO.Enabled = true;
            cbxBypassAudioProcessing.Enabled = true;
            if (e.Error != null)
            {
                if (e.Error is ApplicationException)
                    return;
                else
                    MessageBox.Show(e.Error.Message + "\n" + e.Error.StackTrace);
                return;
            }
            if (e.Result != null && e.Result is Exception)
            {
                Exception ex = (Exception)e.Result;
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                return;
            }
            EnableCbxMlp();
            StreamInfo[] streams = null;
            streams = inFile.StreamInfos;
            if (null == streams)
            {
                MessageBox.Show("The specified file does not contain a valid PAT/PMT combo.");
                return;
            }

            InputFileTextBox.Text = (string)e.Result;
            foreach (StreamInfo si in streams)
            {
                // hobBIT
                pidList.Add(si.ElementaryPID);
                switch (si.StreamType)
                {
                    case ElementaryStreamTypes.VIDEO_STREAM_H264:
                        ElementaryStreamsListBox.Items.Add(String.Format("AVC Video Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoVideo(si)));
                        break;
                    case ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                        ElementaryStreamsListBox.Items.Add(String.Format("MPEG2 Video Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoVideo(si)));
                        break;
                    case ElementaryStreamTypes.VIDEO_STREAM_VC1:
                        ElementaryStreamsListBox.Items.Add(String.Format("VC-1 Video Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoVideo(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3:
                        ElementaryStreamsListBox.Items.Add(String.Format("Dolby Digital Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS:
                        ElementaryStreamsListBox.Items.Add(String.Format("Dolby Digital Plus Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD:
                        ElementaryStreamsListBox.Items.Add(String.Format("Dolby True HD Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS:
                        ElementaryStreamsListBox.Items.Add(String.Format("DTS Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD:
                        ElementaryStreamsListBox.Items.Add(String.Format("DTS-HD Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO:
                        ElementaryStreamsListBox.Items.Add(String.Format("DTS-HD Master Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_LPCM:
                        ElementaryStreamsListBox.Items.Add(String.Format("Lossless PCM Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG1:
                        ElementaryStreamsListBox.Items.Add(String.Format("MPEG1 Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG2:
                        ElementaryStreamsListBox.Items.Add(String.Format("MPEG2 Audio Stream # {0} {1}", si.ElementaryPID & 0xff, StreamInfoAudio(si)));
                        break;
                    case ElementaryStreamTypes.PRESENTATION_GRAPHICS_STREAM:
                        ElementaryStreamsListBox.Items.Add(String.Format("Presentation Graphics Stream # {0}", si.ElementaryPID & 0xff));
                        break;
                    case ElementaryStreamTypes.INTERACTIVE_GRAPHICS_STREAM:
                        ElementaryStreamsListBox.Items.Add(String.Format("Interactive Graphics Stream # {0}", si.ElementaryPID & 0xff));
                        break;
                    case ElementaryStreamTypes.SUBTITLE_STREAM:
                        ElementaryStreamsListBox.Items.Add(String.Format("Subtitle Stream # {0}", si.ElementaryPID & 0xff));
                        break;
                    default:
                        ElementaryStreamsListBox.Items.Add(String.Format("Unknown Stream of type {0}", si.StreamType));
                        break;
                }
            }
            DTCP_Descriptor ds = inFile.DtcpInfo;
            if (null != ds)
            {
                DtcpInfo.Nodes[0].Nodes.Clear();
                switch (ds.CopyStatus)
                {
                    case DtcpCci.CopyFree:
                        DtcpInfo.Nodes[0].Nodes.Add("Copy free");
                        break;
                    case DtcpCci.CopyNever:
                        DtcpInfo.Nodes[0].Nodes.Add("Copy never");
                        break;
                    case DtcpCci.CopyOnce:
                        DtcpInfo.Nodes[0].Nodes.Add("Copy once");
                        break;
                    case DtcpCci.NoMoreCopies:
                        DtcpInfo.Nodes[0].Nodes.Add("No more copies");
                        break;
                }
                if (ds.AnalogConstrain)
                    DtcpInfo.Nodes[0].Nodes.Add("HD analog output is constrained");
                else
                    DtcpInfo.Nodes[0].Nodes.Add("HD analog output is full resolution");
                if (ds.Macrovision)
                    DtcpInfo.Nodes[0].Nodes.Add("Macrovision is on");
                else
                    DtcpInfo.Nodes[0].Nodes.Add("Macrovision is off");
            }
            length = inFile.VideoLength;
            DtcpInfo.Nodes.Add("Video Length: " + length);
            if (length == TimeSpan.Zero)
            {
                TreeNode tn = new TreeNode();
                tn.ForeColor = Color.Red;
                tn.Text = "Warning: No PCRs available!";
                DtcpInfo.Nodes.Add(tn);
            }
            RemuxProgressBar.Maximum = (int)length.TotalMinutes;
            RemuxProgressBar.Step = 1;
            RemuxProgressBar.Value = 0;
            RemuxProgressTimeTextBox.Text = inFile.VideoLength.ToString();
            if (inFile.FileType == TsFileType.M2TS)
            {
                DtcpInfo.Nodes.Add("File format: M2TS (192 byte packets)");
            }
            else if (inFile.FileType == TsFileType.TS)
            {
                DtcpInfo.Nodes.Add("File format: TS (188 byte packets)");
            }
            OutputFileBrowseButton.Enabled = true;
            if (string.CompareOrdinal(InputFileTextBox.Text, OutputFileTextBox.Text) == 0)
                OutputFileTextBox.Text = String.Empty;
            ElementaryStreamsListBox.Enabled = true;
            if (inFile.FileType == TsFileType.M2TS)
            {
                TsFormatRadioButton.Enabled = true;
                TsFormatRadioButton.Checked = false;
                M2tsFormatRadioButton.Enabled = true;
                M2tsFormatRadioButton.Checked = true;
                BluRayFormatRadioButton.Checked = false;
                BluRayFormatRadioButton.Enabled = true;
                MKVFormatRadioButton.Checked = false;
                MKVFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Checked = false;
            }
            else if (inFile.FileType == TsFileType.TS)
            {
                TsFormatRadioButton.Enabled = true;
                TsFormatRadioButton.Checked = true;
                if (length > TimeSpan.Zero)
                {
                    M2tsFormatRadioButton.Enabled = true;
                    BluRayFormatRadioButton.Enabled = true;
                }
                M2tsFormatRadioButton.Checked = false;
                BluRayFormatRadioButton.Checked = false;
                MKVFormatRadioButton.Checked = false;
                MKVFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Checked = false;
            }
            else if (inFile.FileType == TsFileType.EVOB)
            {
                TsFormatRadioButton.Enabled = true;
                TsFormatRadioButton.Checked = false;
                M2tsFormatRadioButton.Enabled = true;
                M2tsFormatRadioButton.Checked = true;
                BluRayFormatRadioButton.Enabled = true;
                BluRayFormatRadioButton.Checked = false;
                MKVFormatRadioButton.Checked = false;
                MKVFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Checked = false;
            }
            else if (inFile.FileType == TsFileType.MKV)
            {
                TsFormatRadioButton.Enabled = true;
                TsFormatRadioButton.Checked = true;
                M2tsFormatRadioButton.Enabled = true;
                M2tsFormatRadioButton.Checked = false;
                BluRayFormatRadioButton.Enabled = true;
                BluRayFormatRadioButton.Checked = false;
                MKVFormatRadioButton.Checked = false;
                MKVFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Enabled = true;
                DemuxFormatRadioButton.Checked = false;
            }
            if (length > TimeSpan.Zero)
                EnableNumerics();
            DtcpInfo.Nodes[0].ExpandAll();
        }

        // ***************************************************************************************
        // hobBIT below 

        private static TsRemux _FormInstance = null; // Used to access form from static method.
        private Process _MPlayerProcess = null;      // The process handle of MPlayer.
        private double _dMPlayerStartTime;           // The first time MPlayer prints out, capture time is 
                                                     // relative to it.
        private bool _bAutoFilled;                   // Prevent trim correction if auto filled.
        private static object _Serializer = new object(); // Simple thread sync.

        private double _dCurrentTrimTime;            // Time captured from MPlayer output, stored by cap thread.
        private TimeSpan _CurrentTrimTime;           // Recalculated time.
        private ArrayList _LogAddons;                // Thread stores MPlayer log here.

        // Format string to contain video info.
        private string StreamInfoVideo(StreamInfo si)
        {
            string sAR = "Unknown AR";
            switch(si.AspectRatio)
            {
                case AspectRatio.a16_9: sAR = "16:9"; break;
                case AspectRatio.a4_3: sAR = "4:3"; break;
            }

            string sVF = "Unknown VF";
            switch(si.VideoFormat)
            {
                case VideoFormat.i1080: sVF = "1080i"; break;
                case VideoFormat.p1080: sVF = "1080p"; break;
                case VideoFormat.p720: sVF = "720p"; break;
                case VideoFormat.i576: sVF = "576i"; break;
                case VideoFormat.p576: sVF = "576p"; break;
                case VideoFormat.i480: sVF = "480i"; break;
                case VideoFormat.p480: sVF = "480p"; break;
            }

            string sFR = "Unknown FR";
            switch(si.FrameRate)
            {
                case FrameRate.f23_976: sFR = "23.976 fps"; break;
                case FrameRate.f24: sFR = "24 fps"; break;
                case FrameRate.f25: sFR = "25 fps"; break;
                case FrameRate.f29_97: sFR = "29.97 fps"; break;
                case FrameRate.f50: sFR = "50 fps"; break;
                case FrameRate.f59_94: sFR = "59.94 fps"; break;
            }

            return "(" + sAR + ", " + sVF + ", " + sFR + ")";
        }

        // Format string to contain audio details.
        private string StreamInfoAudio(StreamInfo si)
        {
            string sAPT = "Unknown APT";
            switch(si.AudioPresentationType)
            {
                case AudioPresentationType.mono: sAPT = "mono"; break;
                case AudioPresentationType.stereo: sAPT = "stereo"; break;
                case AudioPresentationType.multi: sAPT = "multi"; break;
                case AudioPresentationType.combo: sAPT = "combo"; break;
            }

            string sSF = "Unknown SF";
            switch(si.SamplingFrequency)
            {
                case SamplingFrequency.kHz48: sSF = "48 kHz"; break;
                case SamplingFrequency.kHz96: sSF = "96 kHz"; break;
                case SamplingFrequency.kHz192: sSF = "192 kHz"; break;
                case SamplingFrequency.kHz48_96: sSF = "48/96 kHz"; break;
                case SamplingFrequency.kHz48_192: sSF = "48/192 kHz"; break;
            }

            return "(" + sAPT + ", " + sSF + ")";
        }

        // Set Start trim point.
        private void TrimStartSet_Click(object sender, EventArgs e)
        {
            _bAutoFilled = true;
            TrimStart = _CurrentTrimTime;
            _bAutoFilled = false;
        }

        // Set End trim point.
        private void TrimEndSet_Click(object sender, EventArgs e)
        {
            _bAutoFilled = true;
            TrimEnd = length.Duration() - _CurrentTrimTime;
            _bAutoFilled = false;
        }

        // Start Mplayer and enable console output capture.
        private void StartMPlayer(string sFileName)
        {
            lock (_Serializer)
            {
                // Wait if the last process signals finish.
                if (_MPlayerProcess != null)
                {
                    MessageBox.Show(
                        this,
                        "MPlayer still running",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                MPlayerStartUIConfig();

                // Process related.
                _FormInstance = this;
                _dMPlayerStartTime = -1;
                _dCurrentTrimTime = 0;

                // Find MPlayer executable, either by user setting or in TsRemux.exe dir.
                AppSettingsReader Config = new AppSettingsReader();
                string sMPlayerExe = (string)Config.GetValue("MPlayerExe", typeof(string));
                // Check if configured exe exists.
                if (!File.Exists(sMPlayerExe))
                {
                    // Try to find one in startup dir.
                    sMPlayerExe = Path.Combine(Application.StartupPath, "mplayer.exe");
                    if (!File.Exists(sMPlayerExe))
                    {
                        MessageBox.Show(
                            this,
                            "MPlayer executable missing.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                }

                // Read user settings ...
                string sMPlayerCmdLine = (string)Config.GetValue("MPlayerCmdLine", typeof(string));

                // Append HWND from our picture box.
                sMPlayerCmdLine += string.Format(" -wid {0}", MPlayerPreview.Handle);

                // Need slave mode for control.
                sMPlayerCmdLine += " -loop 0 -slave";

                // Build file component.
                string sFile = "\"" + sFileName + "\"";

                // Create and configure process.
                try
                {
                    _MPlayerProcess = new Process();
                    _MPlayerProcess.StartInfo.FileName = sMPlayerExe;
                    _MPlayerProcess.StartInfo.Arguments = sMPlayerCmdLine + " " + sFile;
                    _MPlayerProcess.StartInfo.UseShellExecute = false;
                    _MPlayerProcess.StartInfo.CreateNoWindow = true;
                    _MPlayerProcess.StartInfo.RedirectStandardOutput = true;
                    _MPlayerProcess.OutputDataReceived += new DataReceivedEventHandler(CaptureMPlayer);
                    _MPlayerProcess.StartInfo.RedirectStandardInput = true;
                    /*
                    _MPlayerProcess.SynchronizingObject = this;
                    _MPlayerProcess.EnableRaisingEvents = true;
                    _MPlayerProcess.Exited += new EventHandler(MPlayerFinished);
                    */
                    _MPlayerProcess.Start();
                    _MPlayerProcess.BeginOutputReadLine();
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        this,
                        "MPlayer can't be started.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    _MPlayerProcess = null;
                    return;
                }

                // Start timer to update UI.
                TrimSetTimer.Enabled = true;
            }
        }

        // Terminate MPlayer.
        private void StopMPlayer()
        {
            lock (_Serializer)
            {
                if (_MPlayerProcess != null)
                {
                    _MPlayerProcess.CancelOutputRead();
                    _MPlayerProcess.Kill();

                    _FormInstance = null;
                    TrimSetTimer.Enabled = false;
                    _MPlayerProcess = null;            

                    MPlayerEndUIConfig();
                }
            }
        }

        // Timer called in 100 ms step, update position elements using the 
        // time captured in parser thread. Other threads are not allowed to 
        // update UI.
        private void TrimSetTimer_Tick(object sender, EventArgs e)
        {
            lock (_Serializer)
            {
                const double dSecToTicks = 10e6;

                // Create timespan from captured time in seconds.
                _CurrentTrimTime = new TimeSpan((long)(_dCurrentTrimTime * dSecToTicks));

                CurPlayPos.Text = string.Format("Pos: {0:00}:{1:00}:{2:00}:{3:000}",
                    _CurrentTrimTime.Hours, 
                    _CurrentTrimTime.Minutes, 
                    _CurrentTrimTime.Seconds, 
                    _CurrentTrimTime.Milliseconds);

                if (PlaybackPos.Maximum != (int)length.TotalSeconds)
                    PlaybackPos.Maximum = (int)length.TotalSeconds;

                if (PlaybackPos.Maximum < (int)_CurrentTrimTime.TotalSeconds)
                    PlaybackPos.Value = PlaybackPos.Maximum;
                else
                {
                    if (_CurrentTrimTime.TotalSeconds < 0)
                        PlaybackPos.Value = 0;
                    else
                        PlaybackPos.Value = (int)_CurrentTrimTime.TotalSeconds;
                }

                foreach (object Line in _LogAddons)
                {
                    LogBox.AppendText(Environment.NewLine + (string)Line);
                }

                _LogAddons.Clear();
            }
        }

        // This method is called with every line MPlayer writes to console.
        private static void CaptureMPlayer(Object SendingProcess, DataReceivedEventArgs OutLine)
        {
            lock (_Serializer)
            {
                if (_FormInstance != null)
                {
                    // Parse only valid strings.
                    if (!string.IsNullOrEmpty(OutLine.Data))
                    {
                        string sOutLine = OutLine.Data;

                        // Check if this line contains a video timestamp.
                        if (sOutLine.Contains("V:"))
                        {
                            // It has, cut it out, hopefully MPlayer doesn't modify it's
                            // output format in future releases.
                            Regex R = new Regex(@".*(V:\s*[\d\.]+).*", RegexOptions.IgnoreCase);
                            Match M = R.Match(sOutLine);
                            string sCut = "";
                            if (M.Success)
                            {
                                Group G = M.Groups[1];
                                sCut = G.Value.Substring(2).Trim();
                            }

                            // Convert.
                            try
                            {
                                if (0 != sCut.Length)
                                {
                                    double dValue = Convert.ToDouble(sCut);
                                    // If this is the first we found, store it as base time.
                                    if (_FormInstance._dMPlayerStartTime < 0)
                                    {
                                        _FormInstance._dMPlayerStartTime = dValue;
                                        _FormInstance._dCurrentTrimTime = 0;
                                    }
                                    else
                                        _FormInstance._dCurrentTrimTime =
                                            dValue -
                                            _FormInstance._dMPlayerStartTime;
                                }
                            }
                            catch (FormatException)
                            {
                                // Ignore unknown output.
                            }
                            catch (OverflowException)
                            {
                                // Ignore unknown output.
                            }
                        }
                        else
                            _FormInstance._LogAddons.Add(sOutLine);
                    }
                }
            }
        }

        // Called after finishing MPlayer.
        private void MPlayerFinished(Object Source, EventArgs e)
        {
            //MPlayerEndEvent();
        }

        // Things to do in front of starting mplayer process.
        private void MPlayerStartUIConfig()
        {
             PlayPause.Enabled = true;
            FrameStep.Enabled = true;
            LargeBack.Enabled = true;
            MediumBack.Enabled = true;
            SmallBack.Enabled = true;
            SmallForward.Enabled = true;
            MediumForward.Enabled = true;
            LargeForward.Enabled = true;
            CurPlayPos.Enabled = true;
            CurPlayPos.Text = "Pos: 00:00:00";
            LogBox.Enabled = true;
            PlaybackPos.Enabled = true;
            PlaybackPos.Value = 0;
        }

        //  Things to do after mplayer stops.
        private void MPlayerEndUIConfig()
        {
            // UI related.
            PlayPause.Enabled = false;
            FrameStep.Enabled = false;
            LargeBack.Enabled = false;
            MediumBack.Enabled = false;
            SmallBack.Enabled = false;
            SmallForward.Enabled = false;
            MediumForward.Enabled = false;
            LargeForward.Enabled = false;
            CurPlayPos.Enabled = false;
            CurPlayPos.Text = "Pos: 00:00:00";
            LogBox.Enabled = false;
            PlaybackPos.Enabled = false;
            PlaybackPos.Value = 0;
        }

        private void SendMPlayerCmd(string sCmd)
        {
            lock (_Serializer)
            {
                if (_MPlayerProcess != null)
                {
                    _MPlayerProcess.StandardInput.Write(sCmd + "\xa");
                }
            }
        }

        private void TsRemux_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopMPlayer();
        }

        private void PlayPause_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("pause");
        }

        private void FrameStep_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("frame_step");
        }

        private void LargeBack_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("seek -600 0");
        }

        private void MediumBack_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("seek -60 0");
        }

        private void SmallBack_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("seek -10 0");
        }

        private void SmallForward_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("seek +10 0");
        }

        private void MediumForward_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("seek +60 0");
        }

        private void LargeForward_Click(object sender, EventArgs e)
        {
            SendMPlayerCmd("seek +600 0");
        }

        private void PlaybackPos_Scroll(object sender, EventArgs e)
        {
            double dPercent = 0;
            if (PlaybackPos.Maximum != 0)
                dPercent = 100 * ((double)PlaybackPos.Value / (double)PlaybackPos.Maximum);
            SendMPlayerCmd(string.Format("seek {0} 1", dPercent));
        }

        private void MKVFormatRadioButton_CheckedChanged(object sender, EventArgs e)
        {

        }

        // hobBIT above
        // ***************************************************************************************
    }
}