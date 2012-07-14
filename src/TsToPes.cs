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
 *  PesFile constructor to use "reasonable" buffer size when not async
 *  EvoPesFile to use reasonable buffer size; 72MB buffer caused failure on all .mpg file open attempts
 *  EvoPesFile.GetNextPesPackets() to read up to 128 bytes at a time when getting packet content
 *  TsIo.Read() to not call System.ArrayCopy on a 1-byte read request
 * Measured throughput improvement from the above approximately 3.4x
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace TsRemux
{
    public delegate void PcrChanged(Int64 PCR);
    public delegate void PtsChanged(Int64 PTS, ushort PID);

    public abstract class PesFile
    {
        protected FileStream fs;
        protected TsIo tsior;
        protected TsFileType fileType;
        protected Int64 startPcr;
        protected Int64 endPcr;
        protected StreamInfo[] sis;
        protected PcrChanged pcrDelegate;
        protected PtsChanged ptsDelegate;
        protected BackgroundWorker openWorker;
        protected int lastPercent;

        protected PesFile(BackgroundWorker openWorker)
        {
            fs = null;
            tsior = null;
            fileType = TsFileType.UNKNOWN;
            startPcr = -1;
            endPcr = -1;
            sis = null;
            pcrDelegate = null;
            ptsDelegate = null;
            this.openWorker = openWorker;
            lastPercent = 0;
        }

        protected void ReportProgress(int percent)
        {
            if (openWorker.CancellationPending)
            {
                throw new ApplicationException("Opening file canceled");
            }
            if (lastPercent != percent && percent > 0 && percent <= 100)
            {
                lastPercent = percent;
                openWorker.ReportProgress(percent);
            }
        }

        public void SetPcrDelegate(PcrChanged pcr)
        {
            pcrDelegate = pcr;
        }

        public Int64 StartPcr
        {
            get { return startPcr; }
        }

        public Int64 EndPcr
        {
            get { return endPcr; }
        }

        public void SetPtsDelegate(PtsChanged pts)
        {
            ptsDelegate = pts;
        }

        public StreamInfo GetStreamInfo(ushort pid)
        {
            foreach (StreamInfo si in sis)
                if (si.ElementaryPID == pid)
                    return si;
            return null;
        }

        private void ParseElementaryStreams()
        {
            Seek(-1);
            for (int i = 0; i < 300;)
            {
                ReportProgress(50 + ((50 * i) / 300));
                PesPacket[] ppa = GetNextPesPackets();
                if(ppa == null)
                    goto done;
                foreach (PesPacket pp in ppa)
                {
                    if (null != pp)
                    {
                        byte[] payload = pp.GetPayload();
                        StreamInfo si = GetStreamInfo(pp.PID);
                        if (si != null)
                            ParseStream(si, payload);
                        i++;
                    }
                    else
                        goto done;
                }
            }
        done:
            Seek(-1);
        }

        private void ParseStream(StreamInfo si, byte[] payload)
        {
            switch (si.StreamType)
            {
                case ElementaryStreamTypes.VIDEO_STREAM_VC1:
                    if ((si.ElementaryDescriptors == null) ||
                        si.FrameRate == FrameRate.Reserved ||
                        si.VideoFormat == VideoFormat.Reserved ||
                        si.AspectRatio == AspectRatio.Reserved)
                    {
                        VC1SequenceInfo sq = new VC1SequenceInfo(payload, 0);
                        if (sq.Valid)
                        {
                            if (sq.AspectRatio != AspectRatio.Reserved)
                                si.AspectRatio = sq.AspectRatio;
                            if (sq.FrameRate != FrameRate.Reserved)
                                si.FrameRate = sq.FrameRate;
                            if (sq.VideoFormat != VideoFormat.Reserved)
                                si.VideoFormat = sq.VideoFormat;
                            if (si.ElementaryDescriptors == null)
                                si.ElementaryDescriptors = Constants.vc1_descriptor;
                        }
                    }
                    break;
                case ElementaryStreamTypes.AUDIO_STREAM_AC3:
                    if ((si.ElementaryDescriptors == null) ||
                        si.AudioPresentationType == AudioPresentationType.Reserved ||
                        si.SamplingFrequency == SamplingFrequency.Reserved)
                    {
                        AC3Info ac3 = new AC3Info(payload, 0);
                        if (ac3.Valid)
                        {
                            if (ac3.AudioPresentationType != AudioPresentationType.Reserved)
                                si.AudioPresentationType = ac3.AudioPresentationType;
                            if (ac3.SamplingFrequency != SamplingFrequency.Reserved)
                                si.SamplingFrequency = ac3.SamplingFrequency;
                            if (si.ElementaryDescriptors == null)
                                si.ElementaryDescriptors = ac3.ElementaryDescriptors;
                            if (ac3.SyntaxType == Ac3SyntaxType.Enhanced)
                                si.StreamType = ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS;
                        }
                    }
                    break;
                case ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS:
                    if (si.AudioPresentationType == AudioPresentationType.Reserved ||
                        si.SamplingFrequency == SamplingFrequency.Reserved)
                    {
                        AC3Info ac3 = new AC3Info(payload, 0);
                        if (ac3.Valid)
                        {
                            if (ac3.AudioPresentationType != AudioPresentationType.Reserved)
                                si.AudioPresentationType = ac3.AudioPresentationType;
                            if (ac3.SamplingFrequency != SamplingFrequency.Reserved)
                                si.SamplingFrequency = ac3.SamplingFrequency;
                            if (si.ElementaryDescriptors == null)
                                si.ElementaryDescriptors = ac3.ElementaryDescriptors;
                        }
                    }
                    break;
                case ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD:
                    if (si.AudioPresentationType == AudioPresentationType.Reserved ||
                        si.SamplingFrequency == SamplingFrequency.Reserved)
                    {
                        MlpInfo ac3 = new MlpInfo(payload, 0);
                        if (ac3.Valid)
                        {
                            if (ac3.AudioPresentationType != AudioPresentationType.Reserved)
                                si.AudioPresentationType = ac3.AudioPresentationType;
                            if (ac3.SamplingFrequency != SamplingFrequency.Reserved)
                                si.SamplingFrequency = ac3.SamplingFrequency;
                            if (si.ElementaryDescriptors == null)
                                si.ElementaryDescriptors = ac3.ElementaryDescriptors;
                        }
                    }
                    break;
                case ElementaryStreamTypes.VIDEO_STREAM_H264:
                    if ((si.ElementaryDescriptors == null) ||
                        si.FrameRate == FrameRate.Reserved ||
                        si.VideoFormat == VideoFormat.Reserved ||
                        si.AspectRatio == AspectRatio.Reserved)
                    {
                        H264Info h264 = new H264Info(payload, 0);
                        if(h264.Valid)
                        {
                            if (h264.AspectRatio != AspectRatio.Reserved)
                                si.AspectRatio = h264.AspectRatio;
                            if (h264.FrameRate != FrameRate.Reserved)
                                si.FrameRate = h264.FrameRate;
                            if (h264.VideoFormat != VideoFormat.Reserved)
                                si.VideoFormat = h264.VideoFormat;
                            if (si.ElementaryDescriptors == null)
                                si.ElementaryDescriptors = h264.ElementaryDescriptors;
                        }
                    }
                    break;
                case ElementaryStreamTypes.AUDIO_STREAM_DTS:
                case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD:
                case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO:
                    if (si.AudioPresentationType == AudioPresentationType.Reserved ||
                        si.SamplingFrequency == SamplingFrequency.Reserved)
                    {
                        DtsInfo dts = new DtsInfo(payload, 0);
                        if (dts.Valid)
                        {
                            if (dts.AudioPresentationType != AudioPresentationType.Reserved)
                                si.AudioPresentationType = dts.AudioPresentationType;
                            if (dts.SamplingFrequency != SamplingFrequency.Reserved)
                                si.SamplingFrequency = dts.SamplingFrequency;
                        }
                    }
                    break;
                case ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                    if ((si.ElementaryDescriptors == null) ||
                        si.FrameRate == FrameRate.Reserved ||
                        si.VideoFormat == VideoFormat.Reserved ||
                        si.AspectRatio == AspectRatio.Reserved)
                    {
                        Mpeg2Info mpeg2 = new Mpeg2Info(payload, 0);
                        if (mpeg2.Valid)
                        {
                            if (mpeg2.AspectRatio != AspectRatio.Reserved)
                                si.AspectRatio = mpeg2.AspectRatio;
                            if (mpeg2.FrameRate != FrameRate.Reserved)
                                si.FrameRate = mpeg2.FrameRate;
                            if (mpeg2.VideoFormat != VideoFormat.Reserved)
                                si.VideoFormat = mpeg2.VideoFormat;
                            if (si.ElementaryDescriptors == null)
                                si.ElementaryDescriptors = mpeg2.ElementaryDescriptors;
                        }
                    }
                    break;
            }
        }

        private static TsFileType GetFileType(FileStream fs)
        {
            byte[] inBuff = new byte[Constants.STRIDE_SIZE * 7];
            int offset;
            fs.Seek(0, SeekOrigin.Begin);

            if (fs.Read(inBuff, 0, inBuff.Length) != inBuff.Length)
                throw new FormatException("The specified file is too short");

            // try ts
            for (offset = 0; offset < Constants.STRIDE_SIZE; offset++)
            {
                if (inBuff[offset] == Constants.SYNC_BYTE)
                {
                    if (inBuff[offset + Constants.TS_SIZE] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.TS_SIZE * 2)] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.TS_SIZE * 3)] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.TS_SIZE * 4)] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.TS_SIZE * 5)] == Constants.SYNC_BYTE)
                        return TsFileType.TS;
                    else if (inBuff[offset + Constants.STRIDE_SIZE] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.STRIDE_SIZE * 2)] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.STRIDE_SIZE * 3)] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.STRIDE_SIZE * 4)] == Constants.SYNC_BYTE &&
                    inBuff[offset + (Constants.STRIDE_SIZE * 5)] == Constants.SYNC_BYTE)
                        return TsFileType.M2TS;
                }
            }
            // try SUP
            if ((inBuff[0] == (byte)'P' || inBuff[0] == (byte)'p') &&
                (inBuff[1] == (byte)'G' || inBuff[1] == (byte)'g'))
                return TsFileType.SUP_ELEMENTARY;
            // try EVOB
            UInt32 marker = 0xffffffff;
            int stuffing = 0;
            bool packstart = false;
            for (int i = 0; i < inBuff.Length; i++)
            {
                marker = marker << 8;
                marker &= 0xffffff00;
                marker += inBuff[i];
                if ((marker & 0xffffff00) == 0x00000100)
                {
                    switch (marker & 0xff)
                    {
                        case 0xba:
                            // pack start code
                            if (inBuff.Length < i + 11)
                            {
                                i = inBuff.Length;
                                break;
                            }
                            i += 10;
                            stuffing = inBuff[i] & 0x7;
                            if (inBuff.Length < i + stuffing + 1)
                            {
                                i = inBuff.Length;
                                break;
                            }
                            i += stuffing;
                            stuffing = 0;
                            marker = 0xffffffff;
                            packstart = true;
                            break;
                        default:
                            // other PES packet
                            if (inBuff.Length < i + 3)
                            {
                                i = inBuff.Length;
                                break;
                            }
                            stuffing = (inBuff[i + 1] << 8) + inBuff[i + 2];
                            i += 2;
                            if (inBuff.Length < i + stuffing)
                            {
                                i = inBuff.Length;
                                break;
                            }
                            i += stuffing;
                            stuffing = 0;
                            marker = 0xffffffff;
                            break;
                    }
                }
            }
            if (packstart)
                return TsFileType.EVOB;
            // try MKV
            marker = 0xffffffff;
            for (int i = 0; i < inBuff.Length; i++)
            {
                marker = marker << 8;
                marker &= 0xffffff00;
                marker += inBuff[i];
                if (marker == Constants.MKVFILE_START)
                    return TsFileType.MKV;
            }
            throw new FormatException("The specified file is not a valid TS/M2TS/EVOB/MKV file");
        }

        public static PesFile OpenFile(string path, bool useAsync, BackgroundWorker openWorker)
        {
            PesFile pf = null;
            FileStream fs = null;
            fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.DISK_BUFFER, useAsync); 
            try
            {
                TsIo tsior = new TsIo(fs, null, Constants.DISK_BUFFER ); 
                TsFileType fileType = GetFileType(fs);
                switch (fileType)
                {
                    case TsFileType.EVOB:
                        pf = new EvoPesFile(openWorker);
                        break;
                    case TsFileType.M2TS:
                        pf = new TsPesFile(openWorker,4);
                        break;
                    case TsFileType.TS:
                        pf = new TsPesFile(openWorker,0);
                        break;
                    case TsFileType.SUP_ELEMENTARY:
                        pf = new SupPesFile(openWorker);
                        break;
                    case TsFileType.MKV:
                        pf = new MkvPesFile(openWorker);
                        break;
                }
                pf.fileType = fileType;
                pf.fs = fs;
                pf.tsior = tsior;
                pf.GetInitialValues();
                pf.ParseElementaryStreams();
            }
            catch (Exception)
            {
                fs.Close();
                throw;
            }
            return pf;
        }

        public void CloseFile()
        {
            fs.Close();
            fs = null;
            tsior = null;
        }

        public void Clear()
        {
            tsior.Clear();
        }

        public TsFileType FileType
        {
            get { return fileType; }
        }

        public StreamInfo[] StreamInfos
        {
            get { return sis; }
        }

        public TimeSpan VideoLength
        {
            get
            {
                Int64 span = endPcr - startPcr;
                if (span < 0)
                    span += Constants.MAX_MPEG2TS_CLOCK;
                return new TimeSpan((span * 10) / 27);
            }
        }

        public abstract PesPacket[] GetNextPesPackets();
        public abstract void Seek(Int64 pcr);
        public abstract DTCP_Descriptor DtcpInfo { get; }
        protected abstract void GetInitialValues();
    }

    public class EvoPesFile : PesFile
    {
        private Dictionary<ushort, List<byte>> soundFrames;
        private Int64 lastPcr;

        public EvoPesFile(BackgroundWorker bw)
            : base(bw)
        {
            soundFrames = new Dictionary<ushort, List<byte>>();
            lastPcr = 0 - Constants.MPEG2TS_CLOCK_RATE;
        }

        public override DTCP_Descriptor DtcpInfo
        {
            get { return null; }
        }

        protected override void GetInitialValues()
        {
            GetTimeStamps();

            Dictionary<byte, StreamInfo> evoStreams = new Dictionary<byte, StreamInfo>();
            fs.Seek(0, SeekOrigin.Begin);
            byte[] inBuff = new byte[Constants.DISK_BUFFER]; // 72MB IS TOO BIG! CAUSES FAILURE: << 2]; note this was line 463
            int Length = fs.Read(inBuff, 0, inBuff.Length);
            UInt32 marker = 0xffffffff;
            int stuffing = 0;
            for (int i = 0; i < Length; i++)
            {
                ReportProgress(50 * i / Length);
                marker = marker << 8;
                marker &= 0xffffff00;
                marker += inBuff[i];
                if ((marker & 0xffffff00) == 0x00000100)
                {
                    switch (marker & 0xff)
                    {
                        case 0xba:
                            // pack start code
                            if (Length <= i + 10)
                            {
                                i = Length;
                                break;
                            }
                            i += 10;
                            stuffing = inBuff[i] & 0x7;
                            if (Length <= i + stuffing)
                            {
                                i = Length;
                                break;
                            }
                            i += stuffing;
                            stuffing = 0;
                            break;
                        default:
                            if (inBuff[i] == Constants.PES_PRIVATE1)
                            {
                                // skip to the end of the header
                                int endOfHeader = i + 6 + inBuff[i + 5];
                                if (inBuff.Length > endOfHeader + 4)
                                {
                                    if ((inBuff[endOfHeader] & 0xe0) == Constants.PES_PRIVATE_SUBTITLE)
                                    {
                                        byte subid = inBuff[endOfHeader];
                                        if (evoStreams.ContainsKey(subid) == false)
                                        {
                                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.PRESENTATION_GRAPHICS_STREAM, subid);
                                            evoStreams.Add(subid, si);
                                        }
                                    }
                                    else if ((inBuff[endOfHeader] & 0xf8) == Constants.PES_PRIVATE_AC3)
                                    {
                                        byte subid = inBuff[endOfHeader];
                                        if (evoStreams.ContainsKey(subid) == false)
                                        {
                                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3, subid);
                                            evoStreams.Add(subid, si);
                                        }
                                    }
                                    else if ((inBuff[endOfHeader] & 0xf8) == Constants.PES_PRIVATE_DTS_HD)
                                    {
                                        byte subid = inBuff[endOfHeader];
                                        if (evoStreams.ContainsKey(subid) == false)
                                        {
                                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_DTS_HD, subid);
                                            evoStreams.Add(subid, si);
                                        }
                                    }
                                    else if ((inBuff[endOfHeader] & 0xf0) == Constants.PES_PRIVATE_AC3_PLUS)
                                    {
                                        byte subid = inBuff[endOfHeader];
                                        if (evoStreams.ContainsKey(subid) == false)
                                        {
                                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS, subid);
                                            evoStreams.Add(subid, si);
                                        }
                                    }
                                    else if ((inBuff[endOfHeader] & 0xf8) == Constants.PES_PRIVATE_LPCM)
                                    {
                                        byte subid = inBuff[endOfHeader];
                                        if (evoStreams.ContainsKey(subid) == false)
                                        {
                                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_LPCM, subid);
                                            evoStreams.Add(subid, si);
                                        }
                                    }
                                    else if ((inBuff[endOfHeader] & 0xf8) == Constants.PES_PRIVATE_AC3_TRUE_HD)
                                    {
                                        byte subid = inBuff[endOfHeader];
                                        if (evoStreams.ContainsKey(subid) == false)
                                        {
                                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD, subid);
                                            evoStreams.Add(subid, si);
                                        }
                                    }
                                }
                            }
                            else if ((inBuff[i] & 0xe0) == Constants.PES_AUDIO_MPEG)
                            {
                                byte subid = inBuff[i];
                                if (evoStreams.ContainsKey(subid) == false)
                                {
                                    StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_MPEG2, subid);
                                    evoStreams.Add(subid, si);
                                }
                            }
                            else if ((inBuff[i] & 0xf0) == Constants.PES_VIDEO)
                            {
                                byte subid = inBuff[i];
                                if (evoStreams.ContainsKey(subid) == false)
                                {
                                    // need to differenciate between H.264 and MPEG2
                                    if (Length <= i + 5)
                                    {
                                        i = Length;
                                        break;
                                    }
                                    ElementaryStreamTypes videoFormat = ElementaryStreamTypes.INVALID;
                                    int startIndex = i + 6 + inBuff[i + 5];
                                    if (Length <= startIndex + 3)
                                    {
                                        i = Length;
                                        break;
                                    }
                                    UInt32 format = (UInt32)inBuff[startIndex] << 24;
                                    format += (UInt32)inBuff[startIndex + 1] << 16;
                                    format += (UInt32)inBuff[startIndex + 2] << 8;
                                    format += inBuff[startIndex + 3];
                                    if ((format & 0xffff) == 0x1)
                                    {
                                        videoFormat = ElementaryStreamTypes.VIDEO_STREAM_H264;
                                        StreamInfo si = new StreamInfo(videoFormat, subid);
                                        evoStreams.Add(subid, si);
                                    }
                                    else if ((format & 0xffffff00) == 0x100)
                                    {
                                        videoFormat = ElementaryStreamTypes.VIDEO_STREAM_MPEG2;
                                        StreamInfo si = new StreamInfo(videoFormat, subid);
                                        evoStreams.Add(subid, si);
                                    }
                                }
                            }
                            else if (inBuff[i] == Constants.PES_VIDEO_VC1)
                            {
                                byte subid = GetVC1SubstreamId(inBuff, i + 3);
                                if (subid != 0 && evoStreams.ContainsKey(subid) == false)
                                {
                                    StreamInfo si = new StreamInfo(ElementaryStreamTypes.VIDEO_STREAM_VC1, subid);
                                    si.ElementaryDescriptors = Constants.vc1_descriptor;
                                    evoStreams.Add(subid, si);
                                }
                            }

                            // other PES packet
                            if (Length < i + 3)
                            {
                                i = Length;
                                break;
                            }
                            stuffing = (inBuff[i + 1] << 8) + inBuff[i + 2];
                            i += 2;
                            if (Length < i + stuffing)
                            {
                                i = Length;
                                break;
                            }
                            i += stuffing;
                            stuffing = 0;
                            break;
                    }
                    marker = 0xffffffff;
                }
            }
            if (evoStreams.Count > 0)
            {
                sis = new StreamInfo[evoStreams.Values.Count];
                evoStreams.Values.CopyTo(sis, 0);
            }
        }

        public override PesPacket[] GetNextPesPackets()
        {
            // there was a big optimization opportunity here:
            // this method added to partial packets one byte at a time
            // the calls to PesPacket.AddData used about 40% of total CPU 
            byte[] inData = new byte[1];
            byte[] inData2 = new byte[128]; 
            int bytesRead = 0;
            int bytesToRead = 0;
            int bytesToGo = 0;
            UInt32 marker = 0xffffffff;
            PesPacket pp = null;
            PesPacket partial = null;
            int i = 0;
            while (tsior.Read(inData, 0, inData.Length) == inData.Length)
            {
                marker = marker << 8;
                marker &= 0xffffff00;
                marker += inData[0];
                if ((marker & 0xffffff00) == 0x00000100)
                {
                    if ((marker & 0xff) == Constants.PACK_ID)
                    {
                        // pack start code
                        UInt64 header = 0;
                        for (i = 4; i < 10; i++)
                        {
                            if (tsior.Read(inData, 0, inData.Length) != inData.Length)
                                goto done;
                            header <<= 8;
                            header |= inData[0];
                        }
                        for (i = 10; i < 14; i++) // skip mux rate etc.
                        {
                            if (tsior.Read(inData, 0, inData.Length) != inData.Length)
                                goto done;
                        }
                        Int64 pcr = (Int64)((header & 0x380000000000) >> 13);
                        pcr |= (Int64)((header & 0x3fff8000000) >> 12);
                        pcr |= (Int64)((header & 0x3fff800) >> 11);
                        pcr *= 300;
                        pcr |= (Int64)((header & 0x3fe) >> 1);
                        Int64 delta = pcr - lastPcr;
                        if (delta > Constants.MPEG2TS_CLOCK_RATE / 9)
                        {
                            if (pcrDelegate != null)
                                pcrDelegate(pcr);
                            lastPcr = pcr;
                        }
                    }
                    else if (((marker & 0xff) == Constants.SYS_ID)
                        || ((marker & 0xff) == Constants.MAP_ID)
                        || ((marker & 0xff) == Constants.PAD_ID)
                        || ((marker & 0xff) == Constants.DIR_ID))
                    {
                        ushort Length = 0;
                        for (i = 4; i < 6; i++)
                        {
                            if (tsior.Read(inData, 0, inData.Length) != inData.Length)
                                goto done;
                            Length <<= 8;
                            Length |= inData[0];
                        }
                        Length += 6;
                        for (i = 6; i < Length; i++)
                        {
                            if (tsior.Read(inData, 0, inData.Length) != inData.Length)
                                goto done;
                        }
                    }
                    else
                    {
                        byte end = inData[0];
                        inData[0] = 0;
                        partial = new PesPacket(inData, 0, 1, 0);
                        partial.AddData(inData, 0, 1);
                        inData[0] = 1;
                        partial.AddData(inData, 0, 1);
                        inData[0] = end;
                        partial.AddData(inData, 0, 1);
                        ushort Length = 0;
                        for (i = 4; i < 6; i++)
                        {
                            if (tsior.Read(inData, 0, inData.Length) != inData.Length)
                                goto done;
                            partial.AddData(inData, 0, 1);
                            Length <<= 8;
                            Length |= inData[0];
                        }
                        Length += 6;
                        // we don't need this byte-by-byte loop here, as we have just gotten the length:
                        /*
                        for (i = 6; i < Length; i++)
                        {
                            if (tsior.Read(inData, 0, inData.Length) != inData.Length)
                                goto done;
                            partial.AddData(inData, 0, 1);
                        }
                        */
                        bytesToGo = Length - 6;
                        while (bytesToGo > 0)
                        {
                            bytesToRead = (bytesToGo > inData2.Length) ? inData2.Length : bytesToGo;
                            if ((bytesRead = tsior.Read(inData2, 0, bytesToRead)) != bytesToRead)
                                goto done;
                            partial.AddData(inData2, 0, bytesRead);
                            bytesToGo -= bytesRead;
                        }
                        pp = partial;
                        PesHeader ph = partial.GetHeader();
                        if (partial.BaseId == Constants.PES_PRIVATE1)
                        {
                            if ((partial.ExtendedId & 0xf0) == Constants.PES_PRIVATE_AC3_PLUS ||
                                (partial.ExtendedId & 0xf8) == Constants.PES_PRIVATE_AC3)
                            {
                                if (soundFrames.ContainsKey(partial.ExtendedId) == false)
                                    soundFrames.Add(partial.ExtendedId, new List<byte>());
                                byte[] tempd = partial.GetPayload();
                                for (int j = 4; j < tempd.Length; j++)
                                    soundFrames[partial.ExtendedId].Add(tempd[j]);
                                tempd = soundFrames[partial.ExtendedId].ToArray();
                                int len = 0;
                                ushort mk2 = 0xffff;
                                if (tempd.Length > 2 && tempd[0] == (byte)(Constants.AC3_SYNC >> 8) &&
                                    tempd[1] == (byte)(Constants.AC3_SYNC & 0xff))
                                {
                                    pp = null;
                                    while (tempd.Length > 2 && tempd[0] == (byte)(Constants.AC3_SYNC >> 8)
                                        && tempd[1] == (byte)(Constants.AC3_SYNC & 0xff))
                                    {
                                        AC3Info ac3 = new AC3Info(tempd, 0);
                                        if (ac3.Valid == false || ac3.FrameLength == 0 || tempd.Length < ac3.FrameLength)
                                            break;
                                        if (pp == null)
                                            pp = new PesPacket(partial.GetData(), 0, ph.TotalHeaderLength, partial.ExtendedId);
                                        pp.AddData(tempd, 0, ac3.FrameLength);
                                        soundFrames[partial.ExtendedId].RemoveRange(0, ac3.FrameLength);
                                        tempd = soundFrames[partial.ExtendedId].ToArray();
                                    }
                                    if (pp != null)
                                    {
                                        pp.Complete = true;
                                        goto done;
                                    }
                                }
                                for (int j = 2; j < tempd.Length; j++)
                                {
                                    mk2 <<= 8;
                                    mk2 |= tempd[j];
                                    if (mk2 == Constants.AC3_SYNC)
                                    {
                                        len = j - 1;
                                        mk2 = 0xffff;
                                        break;
                                    }
                                }
                                if (len == 0)
                                    len = tempd.Length;
                                soundFrames[partial.ExtendedId].RemoveRange(0, len);
                                pp = new PesPacket(partial.GetData(), 0, ph.TotalHeaderLength, partial.ExtendedId);
                                pp.AddData(tempd, 0, len);
                                pp.Complete = true;
                            }
                            else if ((partial.ExtendedId & 0xf8) == Constants.PES_PRIVATE_AC3_TRUE_HD)
                            {
                                if (soundFrames.ContainsKey(partial.ExtendedId) == false)
                                    soundFrames.Add(partial.ExtendedId, new List<byte>());
                                byte[] tempd = partial.GetPayload();
                                for (int j = 5; j < tempd.Length; j++)
                                    soundFrames[partial.ExtendedId].Add(tempd[j]);
                                tempd = soundFrames[partial.ExtendedId].ToArray();
                                int len = tempd.Length;
                                /*
                                UInt32 mk2 = 0xffffffff;
                                for (int j = 5; j < tempd.Length; j++)
                                {
                                    mk2 <<= 8;
                                    mk2 |= tempd[j];
                                    if (mk2 == Constants.MLP_SYNC)
                                    {
                                        len = j - 3;
                                        mk2 = 0xffffffff;
                                    }
                                }
                                if (len == 0)
                                {
                                        len = tempd.Length;
                                }
                                */
                                soundFrames[partial.ExtendedId].RemoveRange(0, len);
                                pp = new PesPacket(partial.GetData(), 0, ph.TotalHeaderLength, partial.ExtendedId);
                                pp.AddData(tempd, 0, len);
                                pp.Complete = true;
                            }
                            else if ((partial.ExtendedId & 0xf8) == Constants.PES_PRIVATE_LPCM)
                            {
                                if (soundFrames.ContainsKey(partial.ExtendedId) == false)
                                    soundFrames.Add(partial.ExtendedId, new List<byte>());
                                byte[] tempd = partial.GetPayload();
                                for (int j = 7; j < tempd.Length; j++)
                                    soundFrames[partial.ExtendedId].Add(tempd[j]);
                                tempd = soundFrames[partial.ExtendedId].ToArray();
                                int len = tempd.Length;
                                /*
                                UInt32 mk2 = 0xffffffff;
                                for (int j = 5; j < tempd.Length; j++)
                                {
                                    mk2 <<= 8;
                                    mk2 |= tempd[j];
                                    if (mk2 == Constants.MLP_SYNC)
                                    {
                                        len = j - 3;
                                        mk2 = 0xffffffff;
                                    }
                                }
                                if (len == 0)
                                {
                                        len = tempd.Length;
                                }
                                */
                                soundFrames[partial.ExtendedId].RemoveRange(0, len);
                                pp = new PesPacket(partial.GetData(), 0, ph.TotalHeaderLength, partial.ExtendedId);
                                pp.AddData(tempd, 0, len);
                                pp.Complete = true;
                            }
                            else if ((partial.ExtendedId & 0xf8) == Constants.PES_PRIVATE_DTS_HD)
                            {
                                if (soundFrames.ContainsKey(partial.ExtendedId) == false)
                                    soundFrames.Add(partial.ExtendedId, new List<byte>());
                                byte[] tempd = partial.GetPayload();
                                for (int j = 4; j < tempd.Length; j++)
                                    soundFrames[partial.ExtendedId].Add(tempd[j]);
                                tempd = soundFrames[partial.ExtendedId].ToArray();
                                int len = 0;
                                UInt32 mk2 = 0xffffffff;
                                for (int j = 4; j < tempd.Length; j++)
                                {
                                    mk2 <<= 8;
                                    mk2 |= tempd[j];
                                    if (mk2 == Constants.DTS_SYNC)
                                    {
                                        len = j - 3;
                                        mk2 = 0xffffffff;
                                    }
                                }
                                if (len == 0)
                                {
                                    DtsInfo dts = new DtsInfo(tempd, 0);
                                    if (dts.Valid && (int)dts.FrameSize < tempd.Length &&
                                        (tempd.Length - (int)dts.FrameSize < 4))
                                        len = (int)dts.FrameSize;
                                    else
                                        len = tempd.Length;
                                }
                                soundFrames[partial.ExtendedId].RemoveRange(0, len);
                                pp = new PesPacket(partial.GetData(), 0, ph.TotalHeaderLength, partial.ExtendedId);
                                pp.AddData(tempd, 0, len);
                                pp.Complete = true;
                            }
                            else
                                pp.PID = pp.ExtendedId;
                        }
                        else if (pp.BaseId == Constants.PES_PADDING || pp.BaseId == Constants.PES_PRIVATE2)
                        {
                            marker = 0xffffffff;
                            continue;
                        }
                        else if (((pp.BaseId & 0xe0) == Constants.PES_AUDIO_MPEG)
                            || ((pp.BaseId & 0xf0) == Constants.PES_VIDEO))
                            pp.PID = pp.BaseId;
                        else if (pp.BaseId == Constants.PES_VIDEO_VC1 && ph != null)
                            pp.PID = ph.Extention2;
                        if (ph != null && ph.HasPts)
                        {
                            if (ptsDelegate != null)
                                ptsDelegate(ph.Pts, pp.PID);
                        }
                        goto done;
                    }
                    marker = 0xffffffff;
                }
            }
        done:
            if (null != pp)
            {
                PesPacket[] ppa = new PesPacket[1];
                ppa[0] = pp;
                return ppa;
            }
            else
                return null;
        }

        public override void Seek(long pcr)
        {
            tsior.Clear();

            if (pcr == -1)
            {
                fs.Seek(0, SeekOrigin.Begin);
                return;
            }

            int packetSize = sizeof(byte);
            Int64 span = endPcr - startPcr;
            if (span < 0)
                span += Constants.MAX_MPEG2TS_CLOCK;
            Int64 pcrPerByte = span / fs.Length;
            Int64 seek = pcr - startPcr;
            if (seek < 0)
                seek += Constants.MAX_MPEG2TS_CLOCK;
            Int64 length = seek / pcrPerByte;
            length /= packetSize;
            length *= packetSize;
            if (length > fs.Length)
                length = fs.Length;
            fs.Seek(length, SeekOrigin.Begin);
            bool found = false;

            while (found == false)
            {
                Int64 offset = GetCurrentPcrFromFile();
                if (-1 == offset)
                    offset = endPcr;
                offset -= pcr;
                if (offset > Constants.MAX_MPEG2TS_CLOCK / 2)
                    offset -= Constants.MAX_MPEG2TS_CLOCK;
                else if (offset < (0 - (Constants.MAX_MPEG2TS_CLOCK / 2)))
                    offset += Constants.MAX_MPEG2TS_CLOCK;
                Int64 target = offset / Constants.MPEG2TS_CLOCK_RATE;
                if (target > 0)
                {
                    offset /= pcrPerByte;
                    offset >>= 2;
                    offset /= packetSize;
                    offset *= packetSize;
                    fs.Seek(0 - offset, SeekOrigin.Current);
                }
                else if (target < 0)
                {
                    offset /= pcrPerByte;
                    offset >>= 2;
                    offset /= packetSize;
                    offset *= packetSize;
                    fs.Seek(0 - offset, SeekOrigin.Current);
                }
                else
                    break;
            }
        }

        private void GetTimeStamps()
        {
            fs.Seek(0, SeekOrigin.Begin);
            UInt64 header = 0xffffffff;
            byte b = 0;
            int packsize_reading = -1;
            for (int i = 0; i < Constants.DISK_BUFFER; i++)
            {
                int ib = fs.ReadByte();
                if (ib == -1)
                    throw new FormatException("The specified file is too short");
                b = (byte)(ib & 0xff);
                header <<= 8;
                header |= b;
                if ((header & 0xffffffff) == 0x000001ba)
                {
                    // pack start code
                    packsize_reading = 5;
                }
                else if (packsize_reading > 0)
                    packsize_reading--;
                else if (packsize_reading == 0)
                {
                    startPcr = (Int64)((header & 0x380000000000) >> 13);
                    startPcr |= (Int64)((header & 0x3fff8000000) >> 12);
                    startPcr |= (Int64)((header & 0x3fff800) >> 11);
                    startPcr *= 300;
                    startPcr |= (Int64)((header & 0x3fe) >> 1);
                    break;
                }
            }
            int lseek = -15;
            fs.Seek(lseek, SeekOrigin.End);
            header = 0xffffffff;
            b = 0;
            packsize_reading = -1;
            for (int i = 0; i < Constants.DISK_BUFFER; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    int ib = fs.ReadByte();
                    if (ib == -1)
                        throw new FormatException("The specified file is too short");
                    b = (byte)(ib & 0xff);
                    header <<= 8;
                    header |= b;
                    if ((header & 0xffffffff) == 0x000001ba)
                    {
                        // pack start code
                        packsize_reading = 5;
                    }
                    else if (packsize_reading > 0)
                        packsize_reading--;
                    else if (packsize_reading == 0)
                    {
                        endPcr = (Int64)((header & 0x380000000000) >> 13);
                        endPcr |= (Int64)((header & 0x3fff8000000) >> 12);
                        endPcr |= (Int64)((header & 0x3fff800) >> 11);
                        endPcr *= 300;
                        endPcr |= (Int64)((header & 0x3fe) >> 1);
                        break;
                    }
                }
                if (packsize_reading == 0)
                    break;
                lseek--;
                fs.Seek(lseek, SeekOrigin.End);

            }
        }

        private byte GetVC1SubstreamId(byte[] buff, int offset)
        {
            if (buff.Length <= offset + 3)
                return 0;
            if ((buff[offset] & 0xc0) != 0x80)
                return 0; // first two bits must be '10'
            byte PTS_DTS_flags = (byte)(buff[offset + 1] & 0xc0);
            byte ESCR_flag = (byte)(buff[offset + 1] & 0x20);
            byte ES_rate_flag = (byte)(buff[offset + 1] & 0x10);
            byte DSM_trick_mode_flag = (byte)(buff[offset + 1] & 0x08);
            byte additional_copy_info_flag = (byte)(buff[offset + 1] & 0x04);
            byte PES_CRC_flag = (byte)(buff[offset + 1] & 0x02);
            byte PES_extension_flag = (byte)(buff[offset + 1] & 0x01);
            if (buff[offset + 2] == 0)
                return 0;
            int length = offset + buff[offset + 2] + 3;
            if (buff.Length < length)
                return 0;
            offset += 3;
            if (PTS_DTS_flags == 0x80)
                offset += 5;
            if (PTS_DTS_flags == 0xc0)
                offset += 10;
            if (ESCR_flag > 0)
                offset += 6;
            if (ES_rate_flag > 0)
                offset += 3;
            if (DSM_trick_mode_flag > 0)
                offset += 1;
            if (additional_copy_info_flag > 0)
                offset += 1;
            if (PES_CRC_flag > 0)
                offset += 2;
            if (PES_extension_flag == 0)
                return 0;
            byte PES_private_data_flag = (byte)(buff[offset] & 0x80);
            byte pack_header_field_flag = (byte)(buff[offset] & 0x40);
            byte program_packet_sequence_counter_flag = (byte)(buff[offset] & 0x20);
            byte PSTD_buffer_flag = (byte)(buff[offset] & 0x10);
            byte PES_extension_flag_2 = (byte)(buff[offset] & 0x01);
            offset++;
            if (PES_private_data_flag > 0)
                offset += 25;
            if (pack_header_field_flag > 0)
                offset += (buff[offset] + 1);
            if (program_packet_sequence_counter_flag > 0)
                offset += 2;
            if (PSTD_buffer_flag > 0)
                offset += 2;
            if (PES_extension_flag_2 == 0)
                return 0;
            if (buff[offset] != 0x81)
                return 0;
            return buff[offset + 1];
        }

        private Int64 GetCurrentPcrFromFile()
        {
            UInt64 header = 0xffffffff;
            byte b = 0;
            int packsize_reading = -1;
            for (int i = 0; i < Constants.DISK_BUFFER; i++)
            {
                int ib = fs.ReadByte();
                if (ib == -1)
                    return -1;
                b = (byte)(ib & 0xff);
                header <<= 8;
                header |= b;
                if ((header & 0xffffffff) == 0x000001ba)
                {
                    // pack start code
                    packsize_reading = 5;
                }
                else if (packsize_reading > 0)
                    packsize_reading--;
                else if (packsize_reading == 0)
                {
                    Int64 pcr = (Int64)((header & 0x380000000000) >> 13);
                    pcr |= (Int64)((header & 0x3fff8000000) >> 12);
                    pcr |= (Int64)((header & 0x3fff800) >> 11);
                    pcr *= 300;
                    pcr |= (Int64)((header & 0x3fe) >> 1);
                    return pcr;
                }
            }
            return -1;
        }
    }

    public class SupPesFile : PesFile
    {
        private static readonly byte[] PesTemplate = new byte[4] {
            0x00, 0x00, 0x01, 0xbd};

        public SupPesFile(BackgroundWorker bw) : base(bw) { }

        public override DTCP_Descriptor DtcpInfo
        {
            get { return null; }
        }

        protected override void GetInitialValues()
        {
            GetTimeStamps();
            this.sis = new StreamInfo[1];
            this.sis[0] = new StreamInfo(ElementaryStreamTypes.PRESENTATION_GRAPHICS_STREAM, Constants.DEFAULT_PRESENTATION_GRAPHICS_PID);
        }

        public override PesPacket[] GetNextPesPackets()
        {
            byte b0 = 0;
            byte b1 = 0;
            byte[] inData = new byte[1];
            byte[] pgHeader = new byte[3];

            while (tsior.Read(inData, 0, inData.Length) == inData.Length)
            {
                b0 = b1;
                b1 = inData[0];
                if ((b0 == (byte)'P' || b0 == (byte)'p') &&
                    (b1 == (byte)'G' || b1 == (byte)'g'))
                {
                    Int64 Pts = 0;
                    Int64 Dts = 0;
                    ushort len = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        if (0 == tsior.Read(inData, 0, inData.Length))
                            return null;
                        Pts <<= 8;
                        Pts |= inData[0];
                    }
                    Pts <<= 1;
                    if (ptsDelegate != null)
                        ptsDelegate(Pts, sis[0].ElementaryPID);
                    for (int k = 0; k < 4; k++)
                    {
                        if (0 == tsior.Read(inData, 0, inData.Length))
                            return null;
                        Dts <<= 8;
                        Dts |= inData[0];
                    }
                    Dts <<= 1;
                    for (int k = 0; k < 3; k++)
                    {
                        if (0 == tsior.Read(inData, 0, inData.Length))
                            return null;
                        pgHeader[k] = inData[0];
                        if (k > 0)
                        {
                            len <<= 8;
                            len |= inData[0];
                        }
                    }
                    byte hlen = (byte)(Dts == 0 ? 0x05 : 0x0a);
                    ushort plen = (ushort)(6 + hlen + len);
                    PesPacket pp = new PesPacket(PesTemplate, 0, PesTemplate.Length, sis[0].ElementaryPID);
                    inData[0] = (byte)((plen >> 8) & 0xff);
                    pp.AddData(inData, 0, 1);
                    inData[0] = (byte)(plen & 0xff);
                    pp.AddData(inData, 0, 1);
                    inData[0] = 0x81;
                    pp.AddData(inData, 0, 1);
                    inData[0] = (byte)(Dts == 0 ? 0x80 : 0xc0);
                    pp.AddData(inData, 0, 1);
                    inData[0] = hlen;
                    pp.AddData(inData, 0, 1);
                    byte old = (byte)(Dts == 0 ? 0x21 : 0x31);
                    inData[0] = (byte)(((Pts & 0x1C0000000) >> 29) | old);
                    pp.AddData(inData, 0, 1);
                    inData[0] = (byte)((Pts & 0x3fC00000) >> 22);
                    pp.AddData(inData, 0, 1);
                    inData[0] = (byte)(((Pts & 0x3f8000) >> 14) | 0x01);
                    pp.AddData(inData, 0, 1);
                    inData[0] = (byte)((Pts & 0x7f80) >> 7);
                    pp.AddData(inData, 0, 1);
                    inData[0] = (byte)(((Pts & 0x7f) << 1) | 0x01);
                    pp.AddData(inData, 0, 1);
                    if (Dts != 0)
                    {
                        inData[0] = (byte)(((Dts & 0x1C0000000) >> 29) | 0x11);
                        pp.AddData(inData, 0, 1);
                        inData[0] = (byte)((Dts & 0x3fC00000) >> 22);
                        pp.AddData(inData, 0, 1);
                        inData[0] = (byte)(((Dts & 0x3f8000) >> 14) | 0x01);
                        pp.AddData(inData, 0, 1);
                        inData[0] = (byte)((Dts & 0x7f80) >> 7);
                        pp.AddData(inData, 0, 1);
                        inData[0] = (byte)(((Dts & 0x7f) << 1) | 0x01);
                        pp.AddData(inData, 0, 1);
                    }
                    pp.AddData(pgHeader, 0, pgHeader.Length);
                    for (int k = 0; k < len; k++)
                    {
                        if (0 == tsior.Read(inData, 0, inData.Length))
                            return null;
                        pp.AddData(inData, 0, 1);
                    }
                    PesPacket[] ppa = new PesPacket[1];
                    ppa[0] = pp;
                    return ppa;
                }
            }
            return null;
        }

        public override void Seek(long pcr)
        {
            tsior.Clear();

            if (pcr == -1)
            {
                fs.Seek(0, SeekOrigin.Begin);
                return;
            }

            int packetSize = sizeof(byte);
            Int64 span = endPcr - startPcr;
            if (span < 0)
                span += Constants.MAX_MPEG2TS_CLOCK;
            Int64 pcrPerByte = span / fs.Length;
            Int64 seek = pcr - startPcr;
            if (seek < 0)
                seek += Constants.MAX_MPEG2TS_CLOCK;
            Int64 length = seek / pcrPerByte;
            length /= packetSize;
            length *= packetSize;
            if (length > fs.Length)
                length = fs.Length;
            fs.Seek(length, SeekOrigin.Begin);
            bool found = false;

            while (found == false)
            {
                Int64 offset = GetCurrentPcrFromFile();
                if (-1 == offset)
                    offset = endPcr;
                offset -= pcr;
                if (offset > Constants.MAX_MPEG2TS_CLOCK / 2)
                    offset -= Constants.MAX_MPEG2TS_CLOCK;
                else if (offset < (0 - (Constants.MAX_MPEG2TS_CLOCK / 2)))
                    offset += Constants.MAX_MPEG2TS_CLOCK;
                Int64 target = offset / Constants.MPEG2TS_CLOCK_RATE;
                if (target > 0)
                {
                    offset /= pcrPerByte;
                    offset >>= 2;
                    offset /= packetSize;
                    offset *= packetSize;
                    fs.Seek(0 - offset, SeekOrigin.Current);
                }
                else if (target < 0)
                {
                    offset /= pcrPerByte;
                    offset >>= 2;
                    offset /= packetSize;
                    offset *= packetSize;
                    fs.Seek(0 - offset, SeekOrigin.Current);
                }
                else
                    break;
            }
        }

        private Int64 GetCurrentPcrFromFile()
        {
            byte b0 = 0;
            byte b1 = 0;
            Int64 pcr = 0;
            while (true)
            {
                int ib = fs.ReadByte();
                if (ib == -1)
                    return -1;
                b0 = b1;
                b1 = (byte)(ib & 0xff);
                if ((b0 == (byte)'P' || b0 == (byte)'p') &&
                    (b1 == (byte)'G' || b1 == (byte)'g'))
                {
                    pcr = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        ib = fs.ReadByte();
                        if (ib == -1)
                            return -1;
                        b1 = (byte)(ib & 0xff);
                        pcr <<= 8;
                        pcr |= b1;
                    }
                    pcr *= (Constants.MPEG2TS_CLOCK_RATE / Constants.PTS_CLOCK_RATE);
                    break;
                }
            }
            return pcr;
        }

        private void GetTimeStamps()
        {
            fs.Seek(0, SeekOrigin.Begin);
            byte b0 = 0;
            byte b1 = 0;
            for (int i = 0; i < Constants.DISK_BUFFER; i++)
            {
                int ib = fs.ReadByte();
                if (ib == -1)
                    throw new FormatException("The specified file is too short");
                b0 = b1;
                b1 = (byte)(ib & 0xff);
                if ((b0 == (byte)'P' || b0 == (byte)'p') &&
                    (b1 == (byte)'G' || b1 == (byte)'g'))
                {
                    startPcr = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        ib = fs.ReadByte();
                        if (ib == -1)
                            throw new FormatException("The specified file is too short");
                        b1 = (byte)(ib & 0xff);
                        startPcr <<= 8;
                        startPcr |= b1;
                    }
                    startPcr *= (Constants.MPEG2TS_CLOCK_RATE / Constants.PTS_CLOCK_RATE);
                    break;
                }
            }
            int lseek = -10;
            fs.Seek(lseek, SeekOrigin.End);
            b0 = b1 = 0;
            bool found = false;
            for (int i = 0; i < Constants.DISK_BUFFER; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int ib = fs.ReadByte();
                    if (ib == -1)
                        throw new FormatException("The specified file is too short");
                    b0 = b1;
                    b1 = (byte)(ib & 0xff);
                    if ((b0 == (byte)'P' || b0 == (byte)'p') &&
                        (b1 == (byte)'G' || b1 == (byte)'g'))
                    {
                        endPcr = 0;
                        for (int k = 0; k < 4; k++)
                        {
                            ib = fs.ReadByte();
                            if (ib == -1)
                                throw new FormatException("The specified file is too short");
                            b1 = (byte)(ib & 0xff);
                            endPcr <<= 8;
                            endPcr |= b1;
                        }
                        endPcr *= (Constants.MPEG2TS_CLOCK_RATE / Constants.PTS_CLOCK_RATE);
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
                lseek--;
                fs.Seek(lseek, SeekOrigin.End);
            }
        }
    }

    public class TsPesFile : PesFile
    {
        private int stride;
        private byte[] tsPack;
        TsPacket ts;
        private Dictionary<ushort, PesPacket> PesPackets;
        private DTCP_Descriptor dt;
        private ushort pcrPID;

        public TsPesFile(BackgroundWorker bw, int offset)
            : base(bw)
        {
            stride = offset;
            tsPack = new byte[Constants.TS_SIZE + offset];
            ts = new TsPacket();
            PesPackets = new Dictionary<ushort, PesPacket>();
            dt = null;
            pcrPID = 0xffff;
        }

        public override DTCP_Descriptor DtcpInfo
        {
            get { return dt; }
        }

        public override PesPacket[] GetNextPesPackets()
        {
            int i;
            while (true)
            {
                for (i = tsior.Read(tsPack, 0, 1 + stride); (i > 0) && (tsPack[stride] != Constants.SYNC_BYTE); i = tsior.Read(tsPack, stride, 1))
                    ; // ensure a good sync byte
                if (i == 0)
                {
                    List<PesPacket> ppa = new List<PesPacket>();
                    foreach (ushort pd in PesPackets.Keys)
                    {
                        PesPacket pp = PesPackets[pd];
                        pp.Complete = true;
                        ppa.Add(pp);
                    }
                    PesPackets.Clear();
                    if (ppa.Count == 0)
                        return null; // end of stream
                    else
                    {
                        return ppa.ToArray();
                    }
                }
                i = tsior.Read(tsPack, stride + 1, tsPack.Length - (stride + 1));
                if (i < tsPack.Length - (stride + 1))
                {
                    List<PesPacket> ppa = new List<PesPacket>();
                    foreach (ushort pd in PesPackets.Keys)
                    {
                        PesPacket pp = PesPackets[pd];
                        pp.Complete = true;
                        ppa.Add(pp);
                    }
                    PesPackets.Clear();
                    if (ppa.Count == 0)
                        return null; // end of stream
                    else
                        return ppa.ToArray();
                }
                ts.SetData(tsPack, stride);
                ushort pid = ts.PID;
                byte[] payload = ts.Payload;
                if (ts.HasPcr)
                {
                    if (null != pcrDelegate && pcrPID == pid)
                        pcrDelegate(ts.Pcr);
                }
                if (ts.HasPesHeader)
                {
                    if (payload != null && payload.Length > 3 && payload[0] == 0x00
                        && payload[1] == 0x00 && payload[2] == 0x01)
                    {
                        PesPacket newpp = new PesPacket(payload, 0, payload.Length, pid);
                        newpp.Priority = ts.Priority;
                        PesHeader ph = newpp.GetHeader();
                        if (ph != null && ph.HasPts)
                        {
                            if (ptsDelegate != null)
                                ptsDelegate(ph.Pts, pid);
                        }
                        if (PesPackets.ContainsKey(pid))
                        {
                            PesPacket pp = PesPackets[ts.PID];
                            pp.Complete = true;
                            PesPackets[ts.PID] = newpp;
                            PesPacket[] ppa = new PesPacket[1];
                            ppa[0] = pp;
                            return ppa;
                        }
                        else
                        {
                            if (newpp.Complete == true)
                            {
                                PesPacket[] ppa = new PesPacket[1];
                                ppa[0] = newpp;
                                return ppa;
                            }
                            PesPackets[ts.PID] = newpp;
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("invalid stream");
                    }
                }
                else if (PesPackets.ContainsKey(pid))
                {
                    PesPacket pp = PesPackets[ts.PID];
                    if (payload != null)
                    {
                        pp.AddData(payload, 0, payload.Length);
                        if (pp.Complete)
                        {
                            PesPackets.Remove(pid);
                            PesPacket[] ppa = new PesPacket[1];
                            ppa[0] = pp;
                            return ppa;
                        }
                    }
                }
            }
        }

        private void GetTimestamps()
        {
            int i = 0;

            startPcr = -1;
            endPcr = -1;
            if(null != tsior)
                tsior.Clear();
            fs.Seek(0, SeekOrigin.Begin);

            while (true)
            {
                for (i = tsior.Read(tsPack, 0, 1 + stride); (i > 0) && (tsPack[stride] != Constants.SYNC_BYTE); i = tsior.Read(tsPack, stride, 1))
                    ; // ensure a good sync byte
                if (i == 0)
                    return; // end of stream
                i = tsior.Read(tsPack, stride + 1, tsPack.Length - (stride + 1));
                if (i < tsPack.Length - (stride + 1))
                    return; // end of stream
                ts.SetData(tsPack, stride);
                if (ts.HasPcr)
                {
                    startPcr = ts.Pcr;
                    pcrPID = ts.PID;
                    break;
                }
                if (fs.Position > (Constants.DISK_BUFFER << 1))
                    return;
            }
            tsior.Clear();
            int packSize = Constants.TS_SIZE + stride;
            fs.Seek(0, SeekOrigin.End);
            for (i = packSize; (fs.Length - fs.Position) < Constants.DISK_BUFFER; i += packSize)
            {
                fs.Seek(0 - i, SeekOrigin.End);
                if (fs.Read(tsPack, 0, packSize) == packSize)
                {
                    if (tsPack[stride] != Constants.SYNC_BYTE)
                    {
                        i++;
                        i -= packSize;
                        continue;
                    }
                    ts.SetData(tsPack, stride);
                    if (ts.HasPcr && pcrPID == ts.PID)
                    {
                        endPcr = ts.Pcr;
                        Seek(-1);
                        return;
                    }
                }
                else
                    break;
            }
            endPcr = startPcr = -1;
        }

        protected override void  GetInitialValues()
        {
            GetTimestamps();
            Seek(-1);
            PatPacket pp = null;
            PmtPacket pm = null;
            bool bluray = false;
            bool validPmt = false;
            ushort pmtPid = 0;
            int i = 0;
            Dictionary<ushort, StreamInfo> streams = new Dictionary<ushort, StreamInfo>();
            while (true)
            {
                for (i = tsior.Read(tsPack, 0, 1 + stride); (i > 0) && (tsPack[stride] != Constants.SYNC_BYTE); i = tsior.Read(tsPack, stride, 1))
                    ; // ensure a good sync byte
                if (i == 0)
                    break; // end of stream
                i = tsior.Read(tsPack, stride + 1, tsPack.Length - (stride + 1));
                if (i < tsPack.Length - (stride + 1))
                    break; // end of stream
                ts.SetData(tsPack, stride);
                if (ts.PID == Constants.PAT_PID)
                {
                    pp = new PatPacket(ts.GetData());
                    ProgramInfo[] pi = pp.Programs;
                    if (null != pi && pi.Length == 2)
                    {
                        if ((pi[0].ProgramNumber == 0 && pi[0].ProgramPID == 0x001f && pi[1].ProgramNumber == 1 && pi[1].ProgramPID == 0x0100)
                            || (pi[1].ProgramNumber == 0 && pi[1].ProgramPID == 0x001f && pi[0].ProgramNumber == 1 && pi[0].ProgramPID == 0x0100))
                        {
                            bluray = true;
                        }
                    }
                    else if (null != pi && pi[0] != null && pi[0].ProgramNumber == 1)
                        pmtPid = pi[0].ProgramPID;
                }
                else if (bluray && ts.PID == 0x0100)
                {
                    if (pm == null)
                        pm = new PmtPacket(ts.GetData());
                    else if (pm.Complete == false)
                        pm.AddData(ts.Payload, 0, ts.Payload.Length);
                    if (pm.Complete)
                    {
                        sis = pm.ElementaryStreams;
                        dt = pm.DtcpInfo;
                        return;
                    }
                }
                else if (ts.PID == pmtPid)
                {
                    pm = new PmtPacket(ts.GetData());
                    sis = pm.ElementaryStreams;
                    dt = pm.DtcpInfo;
                }
                else if (streams.ContainsKey(ts.PID) == false && ts.HasPesHeader)
                {
                    byte[] payload = ts.Payload;
                    PesHeader ph = new PesHeader(ts.Payload);
                    PesPacket pes = new PesPacket(payload, 0, payload.Length, ts.PID);
                    if (ph.StreamId == Constants.PES_PRIVATE1)
                    {
                        byte[] audio = pes.GetPayload();
                        AC3Info ac3 = new AC3Info(audio, 0);
                        DtsInfo dts = new DtsInfo(audio, 0);
                        MlpInfo mlp = new MlpInfo(audio, 0);
                        if (ac3.Valid)
                        {
                            if (ac3.SyntaxType == Ac3SyntaxType.Standard || ac3.SyntaxType == Ac3SyntaxType.Alternative)
                            {
                                StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3, ts.PID);
                                streams.Add(ts.PID, si);
                            }
                            else if (ac3.SyntaxType == Ac3SyntaxType.Enhanced)
                            {
                                StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS, ts.PID);
                                streams.Add(ts.PID, si);
                            }
                        }
                        else if (dts.Valid)
                        {
                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_DTS_HD, ts.PID);
                            streams.Add(ts.PID, si);
                        }
                        else if (mlp.Valid)
                        {
                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD, ts.PID);
                            streams.Add(ts.PID, si);
                        }
                        else if ((pes.ExtendedId & 0xf8) == Constants.PES_PRIVATE_LPCM)
                        {
                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_LPCM, ts.PID);
                            streams.Add(ts.PID, si);
                        }
                    }
                    else if ((ph.StreamId & 0xe0) == Constants.PES_AUDIO_MPEG)
                    {
                        StreamInfo si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_MPEG2, ts.PID);
                        streams.Add(ts.PID, si);
                    }
                    else if ((ph.StreamId & 0xf0) == Constants.PES_VIDEO)
                    {
                        UInt32 format = pes.ExtendedType;
                        if ((format & 0xffff) == 0x1)
                        {
                        }
                        else if ((format & 0xffffff00) == 0x100)
                        {
                        }

                        H264Info h264 = new H264Info(payload, 0);
                        Mpeg2Info mpg2 = new Mpeg2Info(payload, 0);
                        if (h264.Valid)
                        {
                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.VIDEO_STREAM_H264, ts.PID);
                            streams.Add(ts.PID, si);
                        }
                        else if (mpg2.Valid)
                        {
                            StreamInfo si = new StreamInfo(ElementaryStreamTypes.VIDEO_STREAM_MPEG2, ts.PID);
                            streams.Add(ts.PID, si);
                        }
                    }
                    else if (ph.StreamId == Constants.PES_VIDEO_VC1)
                    {
                        StreamInfo si = new StreamInfo(ElementaryStreamTypes.VIDEO_STREAM_VC1, ts.PID);
                        streams.Add(ts.PID, si);
                    }
                }
                if (sis != null)
                {
                    validPmt = true;
                    foreach (StreamInfo si in sis)
                    {
                        if (streams.ContainsKey(si.ElementaryPID) == false)
                        {
                            validPmt = false;
                            break;
                        }
                    }
                    if (validPmt)
                        return;
                }
                if (fs.Position > (Constants.DISK_BUFFER << 1))
                    break;
            }
            sis = new StreamInfo[streams.Values.Count];
            streams.Values.CopyTo(sis, 0);
        }

        private Int64 GetCurrentPcrFromFile()
        {
            int packetSize = 0;
            if (fileType == TsFileType.TS)
                packetSize = Constants.TS_SIZE;
            else if (fileType == TsFileType.M2TS)
                packetSize = Constants.STRIDE_SIZE;
            byte[] inBuff = new byte[packetSize];
            TsPacket tp = new TsPacket();
            Int64 filePos = fs.Position;

            while (fs.Read(inBuff, 0, packetSize) == packetSize && fs.Position < (filePos + 1000000))
            {
                if (packetSize == Constants.STRIDE_SIZE)
                    tp.SetData(inBuff, 4);
                else if (packetSize == Constants.TS_SIZE)
                    tp.SetData(inBuff, 0);
                if (tp.HasPcr)
                {
                    fs.Seek(filePos, SeekOrigin.Begin);
                    return tp.Pcr;
                }
            }
            return -1;
        }

        public override void Seek(Int64 pcr)
        {
            tsior.Clear();
            PesPackets.Clear();

            if (pcr == -1)
            {
                fs.Seek(0, SeekOrigin.Begin);
                return;
            }

            int packetSize = 0;
            if (fileType == TsFileType.TS)
                packetSize = Constants.TS_SIZE;
            else if (fileType == TsFileType.M2TS)
                packetSize = Constants.STRIDE_SIZE;
            else
                packetSize = sizeof(byte);
            Int64 span = endPcr - startPcr;
            if (span < 0)
                span += Constants.MAX_MPEG2TS_CLOCK;
            Int64 pcrPerByte = span / fs.Length;
            Int64 seek = pcr - startPcr;
            if (seek < 0)
                seek += Constants.MAX_MPEG2TS_CLOCK;
            Int64 length = seek / pcrPerByte;
            length /= packetSize;
            length *= packetSize;
            if (length > fs.Length)
                length = fs.Length;
            fs.Seek(length, SeekOrigin.Begin);

            for(int i = 0; i < 100; i++)
            {
                Int64 offset = GetCurrentPcrFromFile();
                if (-1 == offset)
                    offset = endPcr;
                offset -= pcr;
                if (offset > Constants.MAX_MPEG2TS_CLOCK / 2)
                    offset -= Constants.MAX_MPEG2TS_CLOCK;
                else if (offset < (0 - (Constants.MAX_MPEG2TS_CLOCK / 2)))
                    offset += Constants.MAX_MPEG2TS_CLOCK;
                Int64 target = offset / Constants.MPEG2TS_CLOCK_RATE;
                if (target > 0)
                {
                    offset /= pcrPerByte;
                    offset >>= 2;
                    offset /= packetSize;
                    offset *= packetSize;
                    fs.Seek(0 - offset, SeekOrigin.Current);
                }
                else if (target < 0)
                {
                    offset /= pcrPerByte;
                    offset >>= 2;
                    offset /= packetSize;
                    offset *= packetSize;
                    fs.Seek(0 - offset, SeekOrigin.Current);
                }
                else
                    break;
            }
        }
    }

    public class TsIo
    {
        private FileStream fsr;
        private FileStream fsw;
        private byte[] ReadBuffer1;
        private byte[] ReadBuffer2;
        private byte[] ReadBuffer;
        private byte[] WriteBuffer1;
        private byte[] WriteBuffer2;
        private byte[] WriteBuffer;
        private Int64 readIndex;
        private Int64 readSize;
        private Int64 writeIndex;
        IAsyncResult asyncRead;
        IAsyncResult asyncWrite;

        public TsIo(FileStream read, FileStream write)
        {
            init(read, write, Constants.DISK_BUFFER);
        }

        public TsIo(FileStream read, FileStream write, int buffsize)
        {
            init(read, write, buffsize);
        }

        public void init(FileStream read, FileStream write, int buffsize)
        {
            fsr = read;
            fsw = write;
            if (fsr != null)
            {
                ReadBuffer1 = new byte[buffsize];
                ReadBuffer2 = new byte[buffsize];
                ReadBuffer = ReadBuffer1;
            }
            else
            {
                ReadBuffer = ReadBuffer1 = ReadBuffer2 = null;
            }
            if (fsw != null)
            {
                WriteBuffer1 = new byte[buffsize];
                WriteBuffer2 = new byte[buffsize];
                WriteBuffer = WriteBuffer1;
            }
            else
            {
                WriteBuffer = WriteBuffer1 = WriteBuffer2 = null;
            }
            readIndex = 0;
            readSize = 0;
            writeIndex = 0;
            asyncRead = null;
            asyncWrite = null;
        }

        public int Read(byte[] target, int offset, int count)
        {
            if (readIndex == readSize)
            {
                // this buffer is finished, get next one
                if (asyncRead != null)
                {
                    if (asyncRead.IsCompleted == false)
                        GC.Collect(); // garbage collection during spare cycles
                    readSize = fsr.EndRead(asyncRead);
                    if (ReadBuffer == ReadBuffer1)
                        ReadBuffer = ReadBuffer2;
                    else ReadBuffer = ReadBuffer1;
                    readIndex = 0;
                    if (readSize == 0)
                    {
                        asyncRead = null;
                        return 0; // end of file
                    }
                }
                else
                {
                    // one time init - prepopulate buffer
                    readSize = fsr.Read(ReadBuffer, 0, ReadBuffer.Length);
                    if (readSize == 0)
                        return 0;
                    readIndex = 0;
                }
                if (ReadBuffer == ReadBuffer1)
                    asyncRead = fsr.BeginRead(ReadBuffer2, 0, ReadBuffer2.Length, null, null);
                else
                    asyncRead = fsr.BeginRead(ReadBuffer1, 0, ReadBuffer1.Length, null, null);
            }
            if (readIndex < readSize)
            {
                int len = count;
                while (len > (readSize - readIndex))
                {
                    int ret = Read(target, offset, (int)(readSize - readIndex));
                    offset += ret;
                    len -= ret;
                    if (ret == 0)
                        break;
                }
                if (len <= (readSize - readIndex))
                {
                    // this optimization saves a LOT of time in PS files
                    if (len == 1)
                        target[offset] = ReadBuffer[readIndex];
                    else
                        Array.Copy(ReadBuffer, readIndex, target, offset, len);
                    readIndex += len;
                    return count;
                }
                else
                {
                    return count - len;
                }
            }
            throw new ArgumentException("read index out of bounds");
        }

        public void Write(byte[] target, int offset, int count)
        {
            if (writeIndex == WriteBuffer.Length)
            {
                // this buffer is finished, get next one
                if (asyncWrite != null)
                {
                    if (asyncWrite.IsCompleted == false)
                        GC.Collect(); // garbage collection during spare cycles
                    fsw.EndWrite(asyncWrite);
                }
                asyncWrite = fsw.BeginWrite(WriteBuffer, 0, WriteBuffer.Length, null, null);
                if (WriteBuffer == WriteBuffer1)
                    WriteBuffer = WriteBuffer2;
                else
                    WriteBuffer = WriteBuffer1;
                writeIndex = 0;
            }
            if (writeIndex < WriteBuffer.Length)
            {
                while (count > (int)(WriteBuffer.Length - writeIndex))
                {
                    int diff = (int)(WriteBuffer.Length - writeIndex);
                    Write(target, offset, diff);
                    offset += diff;
                    count -= diff;
                }
                Array.Copy(target, offset, WriteBuffer, writeIndex, count);
                writeIndex += count;
                return;
            }
            throw new ArgumentException("write index out of bounds");
        }

        public void Flush()
        {
            if (asyncWrite != null)
            {
                fsw.EndWrite(asyncWrite);
                asyncWrite = null;
            }
            if (writeIndex > 0)
            {
                fsw.Write(WriteBuffer, 0, (int)writeIndex);
                writeIndex = 0;
            }
        }

        public void Clear()
        {
            if (asyncRead != null)
            {
                fsr.EndRead(asyncRead);
                asyncRead = null;
            }
            readIndex = readSize = 0;
        }
    }
}
