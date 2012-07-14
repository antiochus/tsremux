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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Collections;

namespace TsRemux
{
    public struct EpElement
    {
        private Int64 pts;
        private UInt32 spn;

        public EpElement(Int64 pts, UInt32 spn)
        {
            this.pts = pts;
            this.spn = spn;
        }

        public Int64 PTS
        {
            get { return pts; }
        }

        public UInt32 SPN
        {
            get { return spn; }
        }
    }

    // Base class for data muxer.
    abstract public class Muxer
    {
        // Called with every used / valid PesPacket from source stream.
        public abstract void MuxPacket(PesPacket pp);

        // Finish muxing.
        public abstract void Close();

        // Blu-Ray authoring only.
        public abstract EpElement[] GetEpData();
        // Blu-Ray authoring only.
        public abstract StreamInfo[] GetPsi();
        // Blu-Ray authoring only.
        public abstract UInt32 GetCurrentPacketNumber();

        // Called if source changes PCR.
        public abstract void PcrChanged(Int64 pcr);

        public StreamInfo[] Psi
        {
            get { return GetPsi(); }
        }

        public EpElement[] EpData
        {
            get { return GetEpData(); }
        }

        public UInt32 CurrentPacketNumber
        {
            get { return GetCurrentPacketNumber(); }
        }
    }

    // Write stream to MKV directly, just experimenting atm.
    public class MkvMux : Muxer
    {
        private FileStream _FileStream;
        private StreamWriter _StreamWriter;
        private List<StreamInfo> _StreamsToKeep;

        public MkvMux(string fileName, List<StreamInfo> StreamsToKeep, bool fAsync, bool fProcessAudio)
        {
            _FileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, Constants.DISK_BUFFER, fAsync);
            _StreamWriter = new StreamWriter(_FileStream);
            _StreamsToKeep = StreamsToKeep;
        }

        public override void MuxPacket(PesPacket pp)
        {
            // TODO: Fix, just QnD ...
            bool bKeep = false;
            foreach (StreamInfo si in _StreamsToKeep)
            {
                if (si.ElementaryPID == pp.PID)
                    bKeep = true;
            }

            if (bKeep)
            {
                byte[] data = pp.GetData();
                _FileStream.Write(data, 0, data.Length);
            }
        }

        public override void Close()
        {
            _StreamWriter.Close();
            _FileStream.Close();
            _FileStream = null;
        }

        public override void PcrChanged(Int64 pcr)
        {
        }

        public override EpElement[] GetEpData()
        {
            throw new NotSupportedException("Not MKV compatible.");
        }

        public override StreamInfo[] GetPsi()
        {
            throw new NotSupportedException("Not MKV compatible.");
        }

        public override UInt32 GetCurrentPacketNumber()
        {
            throw new NotSupportedException("Not MKV compatible.");
        }
    }

    // Simple stream demuxer, writes every stream to one file.
    public class DeMux : Muxer
    {
        // Represents one used stream.
        private struct DeMuxStream
        {
            private FileStream _FileStream;
            private int _PID;

            public FileStream FileStream
            {
                get { return _FileStream; }
            }

            public int PID
            {
                get { return _PID;  }
            }

            public DeMuxStream(FileStream FS, int PID)
            {
                _FileStream = FS;
                _PID = PID;
            }
        }

        // Used streams.
        private List<DeMuxStream> _Streams;

        // CTor, create list of stream including their stream write instances.
        public DeMux(string fileName, List<StreamInfo> StreamsToKeep, bool fAsync, bool fProcessAudio)
        {
            _Streams = new List<DeMuxStream>();

            foreach (StreamInfo si in StreamsToKeep)
            {
                // Give stream a useful extension.
                string sExt = "unknown";
                switch(si.StreamType)
                {
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3:
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS:
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD:
                    case ElementaryStreamTypes.SECONDARY_AUDIO_AC3_PLUS:
                        sExt = "ac3";
                        break;

                    case ElementaryStreamTypes.AUDIO_STREAM_DTS:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO:
                    case ElementaryStreamTypes.SECONDARY_AUDIO_DTS_HD:
                        sExt = "dts";
                        break;

                    case ElementaryStreamTypes.AUDIO_STREAM_LPCM:
                        sExt = "pcm";
                        break;

                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG1:
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG2:
                        sExt = "mpa";
                        break;

                    case ElementaryStreamTypes.VIDEO_STREAM_H264:
                        sExt = "264";
                        break;

                    case ElementaryStreamTypes.VIDEO_STREAM_MPEG1:
                        sExt = "m1v";
                        break;

                    case ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                        sExt = "m2v";
                        break;

                    case ElementaryStreamTypes.VIDEO_STREAM_VC1:
                        sExt = "vc1";
                        break;
                }

                string sFileName = string.Format("{0}_pid{1}.{2}", fileName, si.ElementaryPID, sExt);

                FileStream FS = new FileStream(sFileName, FileMode.Create, FileAccess.Write, FileShare.None, Constants.DISK_BUFFER, fAsync);
                _Streams.Add(new DeMuxStream(FS, si.ElementaryPID));
            }
        }

        // If PesPacket contains valid stream data, store it
        // in the respective output stream.
        public override void MuxPacket(PesPacket pp)
        {
            foreach (DeMuxStream S in _Streams)
            {
                if (pp.PID == S.PID)
                {
                    byte[] data = pp.GetData();
                    S.FileStream.Write(data, 0, data.Length);
                }
            }
        }

        // Finish (De)Muxing.
        public override void Close()
        {
            foreach (DeMuxStream S in _Streams)
                S.FileStream.Close();
            _Streams.Clear();
        }

        // Ignore atm.
        public override void PcrChanged(Int64 pcr)
        {
        }

        public override EpElement[] GetEpData()
        {
            throw new NotSupportedException("Not MKV compatible.");
        }

        public override StreamInfo[] GetPsi()
        {
            throw new NotSupportedException("Not MKV compatible.");
        }

        public override UInt32 GetCurrentPacketNumber()
        {
            throw new NotSupportedException("Not MKV compatible.");
        }
    }

    // Original Muxer by dmz / spacecat56 ?.
    public class BlueMux : Muxer
    {
        private TsFileType fileType;
        private List<StreamInfo> StreamsToKeep;
        private TsIo tsiow;
        private List<TsPacket> buffer;
        private Int64 currentPcr;
        private Int64 lastPcr;
        private Int64 lastDelta;
        private Int64 pcrOffset;
        private Int64 currentPts;
        private Int64 lastPts;
        private Int64 ptsDelta;
        private Int64 ptsOffset;
        private Int64 ptsCount;
        private PatPacket pat;
        private PmtPacket pmt;
        private SitPacket sit;
        private PcrPacket pcrPacket;
        private Dictionary<ushort, ushort> pidMappings;
        private byte[] header;
        private Dictionary<ushort, byte> Continuities;
        private byte[] supHeader;
        private FileStream fsw;
        private byte VideoType;
        private PesPacket lastPacket;
        private byte sitCount;
        private UInt32 packetCount;
        private List<EpElement> epData;
        private List<StreamInfo> pmtStreams;
        private Dictionary<ushort, Int64> lastPtsList;
        private Dictionary<ushort, List<byte>> soundFrames;
        private bool processAudio;
        private bool MlpToAc3;

        public BlueMux(string fileName, TsFileType fileType, List<StreamInfo> StreamsToKeep, bool fAsync, bool fProcessAudio, bool fMlpToAc3)
        {
            fsw = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, Constants.DISK_BUFFER, fAsync);
            this.tsiow = new TsIo(null, fsw, Constants.DISK_BUFFER);
            this.fileType = fileType;
            this.StreamsToKeep = StreamsToKeep;
            this.buffer = new List<TsPacket>();
            this.currentPcr = -1;
            this.lastPcr = -1;
            this.lastDelta = -1;
            this.pcrOffset = -1;
            this.lastPts = -1;
            this.currentPts = -1;
            this.ptsDelta = -1;
            this.ptsOffset = 0;
            this.ptsCount = -1;
            this.pcrPacket = null;
            this.header = new byte[4];
            this.supHeader = new byte[10];
            this.supHeader[0] = (byte)'P';
            this.supHeader[1] = (byte)'G';
            this.VideoType = 0;
            this.lastPacket = null;
            this.sitCount = 0;
            this.packetCount = 0;
            this.epData = new List<EpElement>();
            this.pmtStreams = null;
            this.lastPtsList = new Dictionary<ushort, long>();
            this.soundFrames = new Dictionary<ushort, List<byte>>();
            this.processAudio = fProcessAudio;
            this.MlpToAc3 = fMlpToAc3;
            CreatePsi();
        }

        public override UInt32 GetCurrentPacketNumber()
        {
            return packetCount + (UInt32)buffer.Count;
        }

        public override EpElement[] GetEpData()
        {
            return epData.ToArray();
        }

        public override StreamInfo[] GetPsi()
        {
            return pmtStreams.ToArray();
        }
        /*
        public UInt32 CurrentPacketNumber
        {
            get { return packetCount + (UInt32)buffer.Count; }
        }

        public EpElement[] EpData
        {
            get
            {
                return epData.ToArray();
            }
        }

        public StreamInfo[] Psi
        {
            get { return pmtStreams.ToArray(); }
        }
        */

        public override void MuxPacket(PesPacket pp)
        {
            if (fileType == TsFileType.TS || fileType == TsFileType.M2TS || fileType == TsFileType.BLU_RAY)
                MuxTsPacket(pp);
            else if (fileType == TsFileType.SUP_ELEMENTARY && pidMappings.ContainsKey(pp.PID))
            {
                byte[] data = pp.GetData();
                PesHeader ph = pp.GetHeader();
                if (ph.HasPts)
                {
                    Int64 Pts = ph.Pts;
                    Int64 Dts = 0;
                    if (ph.HasDts)
                        Dts = ph.Dts;
                    supHeader[2] = (byte)((Pts >> 24) & 0xff);
                    supHeader[3] = (byte)((Pts >> 16) & 0xff);
                    supHeader[4] = (byte)((Pts >> 8) & 0xff);
                    supHeader[5] = (byte)(Pts & 0xff);
                    supHeader[6] = (byte)((Dts >> 24) & 0xff);
                    supHeader[7] = (byte)((Dts >> 16) & 0xff);
                    supHeader[8] = (byte)((Dts >> 8) & 0xff);
                    supHeader[9] = (byte)(Dts & 0xff);
                    tsiow.Write(supHeader, 0, supHeader.Length);
                }
                tsiow.Write(data, ph.HeaderLength + 9, data.Length - (ph.HeaderLength + 9));
            }
            else if (fileType == TsFileType.PES_ELEMENTARY && pidMappings.ContainsKey(pp.PID))
            {
                byte[] data = pp.GetData();
                tsiow.Write(data, 0, data.Length);
            }
            else if (fileType == TsFileType.ELEMENTARY && pidMappings.ContainsKey(pp.PID))
            {
                byte[] data = pp.GetData();
                PesHeader ph = pp.GetHeader();
                tsiow.Write(data, ph.HeaderLength + 9, data.Length - (ph.HeaderLength + 9));
            }
        }

        public override void PcrChanged(Int64 pcr)
        {
            lastPcr = currentPcr;
            currentPcr = pcr;
            Int64 newPcr = -1;
            if (lastPcr == -1)
            {
                pcrOffset = currentPcr - Constants.MPEG2TS_CLOCK_RATE;
                newPcr = currentPcr - pcrOffset;
                if (newPcr < 0)
                    newPcr += Constants.MAX_MPEG2TS_CLOCK;
                if (newPcr >= Constants.MAX_MPEG2TS_CLOCK)
                    newPcr -= Constants.MAX_MPEG2TS_CLOCK;
                pcrPacket = new PcrPacket(newPcr,0,Constants.DEFAULT_PCR_PID);
                PcrPacket pp = new PcrPacket(newPcr, pcrPacket.ContinuityCounter, Constants.DEFAULT_PCR_PID);
                buffer.Add(pp);
                return;
            }
            Int64 delta = currentPcr - lastPcr;
            if (delta < 0)
                delta += Constants.MAX_MPEG2TS_CLOCK;
            if (delta > Constants.MPEG2TS_CLOCK_RATE)
            {
                // discontinuity - adjust offset
                Int64 predictedPcr = lastPcr + (buffer.Count * lastDelta);
                if (predictedPcr >= Constants.MAX_MPEG2TS_CLOCK)
                    predictedPcr -= Constants.MAX_MPEG2TS_CLOCK;
                pcrOffset += currentPcr - predictedPcr;
                pcrPacket.IncrementContinuityCounter();
                PcrPacket pp = new PcrPacket(newPcr, pcrPacket.ContinuityCounter, Constants.DEFAULT_PCR_PID);
                buffer.Add(pp);
                return;
            }
            newPcr = currentPcr - pcrOffset;
            if (newPcr < 0)
                newPcr += Constants.MAX_MPEG2TS_CLOCK;
            if (newPcr >= Constants.MAX_MPEG2TS_CLOCK)
                newPcr -= Constants.MAX_MPEG2TS_CLOCK;
            pat.IncrementContinuityCounter();
            buffer.Add(new PatPacket(pat.GetData()));
            pmt.IncrementContinuityCounter();
            buffer.Add(new PmtPacket(pmt.GetData()));
            sitCount += 1;
            sitCount %= 10;
            if (sitCount == 0)
            {   // one SIT for every 10 PMT/PAT
                sit.IncrementContinuityCounter();
                buffer.Add(new SitPacket(sit.GetData()));
            }
            pcrPacket.IncrementContinuityCounter();
            PcrPacket p = new PcrPacket(newPcr, pcrPacket.ContinuityCounter, Constants.DEFAULT_PCR_PID);
            buffer.Add(p);
            lastDelta = delta / buffer.Count;
            Int64 stamp = lastPcr % Constants.MAX_BLURAY_CLOCK;
            for (int i = 0; i < buffer.Count; i++)
            {
                stamp += lastDelta;
                stamp %= Constants.MAX_BLURAY_CLOCK;
                header[0] = (byte)((stamp >> 24) & 0x3f);
                header[1] = (byte)((stamp >> 16) & 0xff);
                header[2] = (byte)((stamp >> 8) & 0xff);
                header[3] = (byte)(stamp & 0xff);
                if (fileType == TsFileType.M2TS || fileType == TsFileType.BLU_RAY)
                {
                    tsiow.Write(header, 0, 4);
                    tsiow.Write(buffer[i].GetData(), 0, Constants.TS_SIZE);
                }
                else if (fileType == TsFileType.TS)
                {
                    tsiow.Write(buffer[i].GetData(), 0, Constants.TS_SIZE);
                }
            }
            packetCount += (UInt32)buffer.Count;
            buffer.Clear();
        }

        public override void Close()
        {
            if (fileType == TsFileType.BLU_RAY)
            {
                if ((CurrentPacketNumber % 32) > 0)
                {
                    for (UInt32 nullPacketNum = 32 - (CurrentPacketNumber % 32); nullPacketNum > 0; nullPacketNum--)
                    {
                        buffer.Add(new TsPacket());
                    }
                }
            }
            if (buffer.Count > 0)
            {
                Int64 stamp = currentPcr % Constants.MAX_BLURAY_CLOCK;
                for (int i = 0; i < buffer.Count; i++)
                {
                    stamp += lastDelta;
                    stamp %= Constants.MAX_BLURAY_CLOCK;
                    header[0] = (byte)((stamp >> 24) & 0x3f);
                    header[1] = (byte)((stamp >> 16) & 0xff);
                    header[2] = (byte)((stamp >> 8) & 0xff);
                    header[3] = (byte)(stamp & 0xff);
                    if (fileType == TsFileType.M2TS || fileType == TsFileType.BLU_RAY)
                    {
                        tsiow.Write(header, 0, 4);
                        tsiow.Write(buffer[i].GetData(), 0, Constants.TS_SIZE);
                    }
                    else if (fileType == TsFileType.TS)
                    {
                        tsiow.Write(buffer[i].GetData(), 0, Constants.TS_SIZE);
                    }
                }
                packetCount += (UInt32)buffer.Count;
                buffer.Clear();
            }
            tsiow.Flush();
            fsw.Close();
        }

        private void CreatePsi()
        {
            pidMappings = new Dictionary<ushort, ushort>();
            Continuities = new Dictionary<ushort, byte>();
            ushort currentVideoPid = Constants.DEFAULT_VIDEO_PID;
            ushort currentAudioPid = Constants.DEFAULT_AUDIO_PID;
            ushort currentSubPid = Constants.DEFAULT_SUBTITLE_PID;
            ushort currentPresPid = Constants.DEFAULT_PRESENTATION_GRAPHICS_PID;
            ushort currentIntPid = Constants.DEFAULT_INTERACTIVE_GRAPHICS_PID;
            pmtStreams = new List<StreamInfo>();
            StreamInfo sitemp = null;
            foreach (StreamInfo si in StreamsToKeep)
            {
                if (pidMappings.ContainsKey(si.ElementaryPID))
                    throw new ArgumentException(String.Format("Invalid/Duplicate StreamInfo with pid {0}", si.ElementaryPID));
                switch (si.StreamType)
                {
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3:
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS:
                        pidMappings.Add(si.ElementaryPID, currentAudioPid);
                        if(this.fileType == TsFileType.BLU_RAY)
                            sitemp = new StreamInfo(si.StreamType, currentAudioPid);
                        else
                            sitemp = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3, currentAudioPid);
                        Continuities.Add(currentAudioPid, 0);
                        currentAudioPid++;
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD:
                        pidMappings.Add(si.ElementaryPID, currentAudioPid);
                        if(MlpToAc3)
                            sitemp = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3, currentAudioPid);
                        else
                            sitemp = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD, currentAudioPid);
                        Continuities.Add(currentAudioPid, 0);
                        currentAudioPid++;
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO:
                        pidMappings.Add(si.ElementaryPID, currentAudioPid);
                        if (MlpToAc3)
                            sitemp = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_DTS, currentAudioPid);
                        else
                            sitemp = new StreamInfo(si.StreamType, currentAudioPid);
                        Continuities.Add(currentAudioPid, 0);
                        currentAudioPid++;
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS:
                    case ElementaryStreamTypes.AUDIO_STREAM_LPCM:
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG1:
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG2:
                        pidMappings.Add(si.ElementaryPID, currentAudioPid);
                        sitemp = new StreamInfo(si.StreamType, currentAudioPid);
                        Continuities.Add(currentAudioPid, 0);
                        currentAudioPid++;
                        break;
                    case ElementaryStreamTypes.SUBTITLE_STREAM:
                        pidMappings.Add(si.ElementaryPID, currentSubPid);
                        sitemp = new StreamInfo(si.StreamType, currentSubPid);
                        Continuities.Add(currentSubPid, 0);
                        currentSubPid++;
                        break;
                    case ElementaryStreamTypes.PRESENTATION_GRAPHICS_STREAM:
                        pidMappings.Add(si.ElementaryPID, currentPresPid);
                        sitemp = new StreamInfo(si.StreamType, currentPresPid);
                        Continuities.Add(currentPresPid, 0);
                        currentPresPid++;
                        break;
                    case ElementaryStreamTypes.INTERACTIVE_GRAPHICS_STREAM:
                        pidMappings.Add(si.ElementaryPID, currentIntPid);
                        sitemp = new StreamInfo(si.StreamType, currentIntPid);
                        Continuities.Add(currentIntPid, 0);
                        currentIntPid++;
                        break;
                    case ElementaryStreamTypes.VIDEO_STREAM_H264:
                    case ElementaryStreamTypes.VIDEO_STREAM_MPEG1:
                    case ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                    case ElementaryStreamTypes.VIDEO_STREAM_VC1:
                        pidMappings.Add(si.ElementaryPID, currentVideoPid);
                        sitemp = new StreamInfo(si.StreamType, currentVideoPid);
                        Continuities.Add(currentVideoPid, 0);
                        currentVideoPid++;
                        if (0 == VideoType)
                            VideoType = (byte)si.StreamType;
                        break;
                }
                sitemp.ElementaryDescriptors = si.ElementaryDescriptors;
                sitemp.AspectRatio = si.AspectRatio;
                sitemp.AudioPresentationType = si.AudioPresentationType;
                sitemp.FrameRate = si.FrameRate;
                sitemp.SamplingFrequency = si.SamplingFrequency;
                sitemp.VideoFormat = si.VideoFormat;
                pmtStreams.Add(sitemp);
            }
            pat = new PatPacket();
            ProgramInfo[] pi = new ProgramInfo[2];
            pi[0] = new ProgramInfo(0, Constants.SIT_PID);
            pi[1] = new ProgramInfo(1, Constants.DEFAULT_PMT_PID);
            pat.Programs = pi;
            pmt = new PmtPacket();
            pmt.PID = Constants.DEFAULT_PMT_PID;
            pmt.ElementaryStreams = pmtStreams.ToArray();
            List<byte> programDescriptors = new List<byte>();
            programDescriptors.AddRange(Constants.copy_control_descriptor);
            programDescriptors.AddRange(Constants.hdmv_registration_descriptor);
            pmt.ProgramDescriptorsData = programDescriptors.ToArray();
            sit = new SitPacket();
            buffer.Add(pat);
            pat.IncrementContinuityCounter();
            buffer.Add(pmt);
            pmt.IncrementContinuityCounter();
            buffer.Add(sit);
            sit.IncrementContinuityCounter();
        }

        private PesPacket CheckAndFixDiscontinuities(PesPacket pp)
        {
            // checks for PTS/DTS discontinuities
            PesHeader ph = pp.GetHeader();
            byte[] data = pp.GetData();
            int len = ph.TotalHeaderLength;
            int i = len;
            UInt32 marker = 0xffffffff;
            if (Constants.DEFAULT_VIDEO_PID == pidMappings[pp.PID])
            {
                // checks for end of stream headers
                switch (VideoType)
                {
                    case (byte)ElementaryStreamTypes.VIDEO_STREAM_VC1:
                        for (; i < data.Length; i++)
                        {
                            marker <<= 8;
                            marker |= data[i];
                            if (marker == Constants.VC1_END_OF_STREAM)
                                break;
                        }
                        if (i < data.Length)
                        {
                            // we have an end of stream marker
                            i -= 3;
                            PesPacket pnew = new PesPacket(data, 0, i, pp.PID);
                            i += 4;
                            pnew.AddData(data, i, data.Length - i);
                            pp = pnew;
                            ph = pp.GetHeader();
                            data = pp.GetData();
                        }
                        break;
                    case (byte)ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                        for (; i < data.Length; i++)
                        {
                            marker <<= 8;
                            marker |= data[i];
                            if (marker == Constants.MPEG2_SEQ_END)
                                break;
                        }
                        if (i < data.Length)
                        {
                            // we have an end of stream marker
                            i -= 3;
                            PesPacket pnew = new PesPacket(data, 0, i, pp.PID);
                            i += 4;
                            pnew.AddData(data, i, data.Length - i);
                            pp = pnew;
                            ph = pp.GetHeader();
                            data = pp.GetData();
                        }
                        break;
                    case (byte)ElementaryStreamTypes.VIDEO_STREAM_H264:
                        for (; i < data.Length; i++)
                        {
                            marker <<= 8;
                            marker |= data[i];
                            if ((marker & 0xffffff9f) == Constants.H264_END_OF_STREAM)
                                break;
                        }
                        if (i < data.Length)
                        {
                            // we have an end of stream marker
                            i -= 3;
                            PesPacket pnew = new PesPacket(data, 0, i, pp.PID);
                            i += 4;
                            pnew.AddData(data, i, data.Length - i);
                            pp = pnew;
                            ph = pp.GetHeader();
                            data = pp.GetData();
                        }
                        break;
                }

                if (ph.HasPts)
                {
                    lastPts = currentPts;
                    currentPts = ph.Pts;
                    ptsCount = ptsDelta;
                    ptsDelta = currentPts - lastPts;
                    if (lastPts != -1)
                    {
                        if (ptsDelta < (0 - (Constants.PTS_CLOCK_RATE << 2)) ||
                            ptsDelta > (Constants.PTS_CLOCK_RATE << 2))
                        {
                            ptsOffset += (lastPts + ptsCount - currentPts);
                        }
                    }
                }

                // build EP Map info
                marker = 0xffffffff;
                switch (VideoType)
                {
                    case (byte)ElementaryStreamTypes.VIDEO_STREAM_VC1:
                        for (i = ph.TotalHeaderLength; i < data.Length; i++)
                        {
                            marker <<= 8;
                            marker |= data[i];
                            if (marker == Constants.VC1_SEQ_SC && ph.HasPts)
                            {
                                EpElement ep = new EpElement(ph.Pts, this.CurrentPacketNumber);
                                epData.Add(ep);
                                break;
                            }
                        }
                        break;
                    case (byte)ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                        for (i = ph.TotalHeaderLength; i < data.Length; i++)
                        {
                            marker <<= 8;
                            marker |= data[i];
                            if (marker == Constants.MPEG2_SEQ_CODE && ph.HasPts)
                            {
                                EpElement ep = new EpElement(ph.Pts, this.CurrentPacketNumber);
                                epData.Add(ep);
                                break;
                            }
                        }
                        break;
                    case (byte)ElementaryStreamTypes.VIDEO_STREAM_H264:
                        for (i = ph.TotalHeaderLength; i < data.Length; i++)
                        {
                            marker <<= 8;
                            marker |= data[i];
                            if ((marker & 0xffffff9f) == Constants.H264_PREFIX && ph.HasPts)
                            {
                                EpElement ep = new EpElement(ph.Pts, this.CurrentPacketNumber);
                                epData.Add(ep);
                                break;
                            }
                        }
                        break;
                }

            }
            if (ph.HasPts && ptsOffset != 0)
            {
                Int64 time = ph.Pts + ptsOffset;
                if (time < 0)
                    time += Constants.MAX_PTS_CLOCK;
                else if (time > Constants.MAX_PTS_CLOCK)
                    time -= Constants.MAX_PTS_CLOCK;
                ph.Pts = time;
                for (i = 9; i < 14; i++)
                    pp[i] = ph[i]; // copy PTS
                if (ph.HasDts)
                {
                    time = ph.Dts + ptsOffset;
                    if (time < 0)
                        time += Constants.MAX_PTS_CLOCK;
                    else if (time > Constants.MAX_PTS_CLOCK)
                        time -= Constants.MAX_PTS_CLOCK;
                    ph.Dts = time;
                    for (i = 14; i < 19; i++)
                        pp[i] = ph[i]; // copy DTS
                }
            }

            lastPacket = pp;
            return pp;
        }

        private void MuxPesPacketToTs(PesPacket pp, bool priority)
        {
            byte[] data = new byte[Constants.TS_SIZE];
            int j = 0;
            int i = 0;
            byte[] pes = pp.GetData();

            // take care of the first packet
            data[0] = Constants.SYNC_BYTE;
            data[1] = 0x40; // payload start
            data[2] = 0;
            if (pes.Length < Constants.TS_PAYLOAD_SIZE)
            {
                data[3] = 0x30; // adaptation and payload
                int stufLength = Constants.TS_PAYLOAD_SIZE - pes.Length - 1;
                data[4] = (byte)stufLength;
                i = 5;
                if (stufLength > 0)
                {
                    data[i] = 0;
                    i++;
                    stufLength--;
                }
                for (; i < (6 + stufLength); i++)
                    data[i] = 0xff;
                for (; i < Constants.TS_SIZE; i++)
                {
                    data[i] = pes[j];
                    j++;
                }
            }
            else
            {
                data[3] = 0x10; // no adaptation, payload only
                for (i = 4; i < data.Length; i++)
                {
                    data[i] = pes[j];
                    j++;
                }
            }
            TsPacket ts = new TsPacket();
            ts.SetData(data, 0);
            ushort pid = pidMappings[pp.PID];
            ts.PID = pid;
            ts.Priority = priority;
            ts.ContinuityCounter = Continuities[pid];
            ts.IncrementContinuityCounter();
            Continuities[pid] = ts.ContinuityCounter;
            buffer.Add(ts);
            while (j < ((pes.Length / Constants.TS_PAYLOAD_SIZE) * Constants.TS_PAYLOAD_SIZE))
            {
                // take care of the other packets
                data[0] = Constants.SYNC_BYTE;
                data[1] = 0x00; // no payload start
                data[2] = 0;
                data[3] = 0x10; // no adaptation, payload only
                for (i = 4; i < data.Length; i++)
                {
                    data[i] = pes[j];
                    j++;
                }
                ts = new TsPacket();
                ts.SetData(data, 0);
                ts.PID = pid;
                ts.Priority = priority;
                ts.ContinuityCounter = Continuities[pid];
                ts.IncrementContinuityCounter();
                Continuities[pid] = ts.ContinuityCounter;
                buffer.Add(ts);
            }
            // take care of the last packet
            if (j < pes.Length)
            {
                data[0] = Constants.SYNC_BYTE;
                data[1] = 0x00; // no payload start
                data[2] = 0;
                data[3] = 0x30; // adaptation and payload
                int stufLength = Constants.TS_PAYLOAD_SIZE - (pes.Length - j) - 1;
                data[4] = (byte)stufLength;
                i = 5;
                if (stufLength > 0)
                {
                    data[i] = 0;
                    i++;
                    stufLength--;
                    for (; i < (6 + stufLength); i++)
                        data[i] = 0xff;
                }
                for (; i < Constants.TS_SIZE; i++)
                {
                    data[i] = pes[j];
                    j++;
                }
                ts = new TsPacket();
                ts.SetData(data, 0);
                ts.PID = pid;
                ts.Priority = priority;
                ts.ContinuityCounter = Continuities[pid];
                ts.IncrementContinuityCounter();
                Continuities[pid] = ts.ContinuityCounter;
                buffer.Add(ts);
            }
        }

        private void MuxTsPacket(PesPacket pp)
        {
            if (pidMappings.ContainsKey(pp.PID))
            {
                pp = CheckAndFixDiscontinuities(pp);
                ElementaryStreamTypes type = GetStreamType(pidMappings[pp.PID]);
                switch (type)
                {
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3:
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS:
                        if (MlpToAc3 == false || pp.Priority)
                            MuxAc3ToTs(pp, type);
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD:
                        MuxMlpToTs(pp, type);
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO:
                        if (MlpToAc3 = false || pp.Priority)
                            MuxDtsToTs(pp, type);
                        break;
                    case ElementaryStreamTypes.AUDIO_STREAM_LPCM:
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG1:
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG2:
                        MuxPesPacketToTs(pp, true);
                        break;
                    default:
                        MuxPesPacketToTs(pp, pp.Priority);
                        break;
                }
            }
        }

        private void MuxAc3ToTs(PesPacket pp, ElementaryStreamTypes type)
        {
            Int64 pts = 0;
            PesHeader ph = pp.GetHeader();
            if (ph.HasPts == false)
            {
                if (lastPtsList.ContainsKey(pp.PID))
                    pts = lastPtsList[pp.PID];
            }
            else
            {
                pts = ph.Pts;
                lastPtsList[pp.PID] = pts;
            }
            byte[] payload = pp.GetPayload();
            AC3Info ac3 = new AC3Info(payload, 0);
            if (this.processAudio || (ac3.Valid && ac3.FrameLength == payload.Length))
                MuxSingleAc3ToTs(payload, 0, payload.Length, pts, type, pp.PID);
            else
            {
                if (soundFrames.ContainsKey(pp.PID) == false)
                {
                    // skip to the good part.
                    ushort mk2 = 0xffff;
                    int index = 0;
                    for (index = 0; index < payload.Length; index++)
                    {
                        mk2 <<= 8;
                        mk2 |= payload[index];
                        if (mk2 == Constants.AC3_SYNC)
                            break;
                    }
                    if (index == payload.Length)
                        MuxSingleAc3ToTs(payload, 0, payload.Length, pts, type, pp.PID);
                    else
                    {
                        index--;
                        List<byte> framelist = new List<byte>();
                        for (; index < payload.Length; index++)
                            framelist.Add(payload[index]);
                        soundFrames.Add(pp.PID, framelist);
                    }
                }
                else
                    soundFrames[pp.PID].AddRange(payload);
                if (soundFrames.ContainsKey(pp.PID))
                {
                    payload = soundFrames[pp.PID].ToArray();
                    ac3 = new AC3Info(payload, 0);
                    int len = 0;
                    if (payload.Length > ac3.MaxFrameLength)
                    {
                        // resync ac3
                        ushort mk2 = 0xffff;
                        int index = 0;
                        for (index = 2; index < payload.Length; index++)
                        {
                            mk2 <<= 8;
                            mk2 |= payload[index];
                            if (mk2 == Constants.AC3_SYNC)
                            {
                                ac3 = new AC3Info(payload, index - 1);
                                if (ac3.Valid && ac3.FrameLength > 0)
                                    break;
                            }
                        }
                        if (index == payload.Length)
                            len = payload.Length;
                        else
                        {
                            index -= 1;
                            len = index;
                            ac3 = new AC3Info(payload, len);
                            while (ac3.Valid && ac3.FrameLength > 0 && ac3.FrameLength + len <= payload.Length)
                            {
                                len += ac3.FrameLength;
                                ac3 = new AC3Info(payload, len);
                            }
                        }
                    }
                    else while (ac3.Valid && ac3.FrameLength > 0 && ac3.FrameLength + len <= payload.Length)
                    {
                        len += ac3.FrameLength;
                        ac3 = new AC3Info(payload, len);
                    }
                    if (len > 0)
                    {
                        MuxSingleAc3ToTs(payload, 0, len, pts, type, pp.PID);
                        soundFrames[pp.PID].RemoveRange(0, len);
                    }
                }
            }
        }

        private void MuxMlpToTs(PesPacket pp, ElementaryStreamTypes type)
        {
            Int64 pts = 0;
            PesHeader ph = pp.GetHeader();
            if (ph.HasPts == false)
            {
                if (lastPtsList.ContainsKey(pp.PID))
                    pts = lastPtsList[pp.PID];
            }
            else
            {
                pts = ph.Pts;
                lastPtsList[pp.PID] = pts;
            }
            byte[] payload = pp.GetPayload();
            /*
            int index = 0;
            int len = 0;
            len = payload.Length - index;
            */
            MuxSingleAc3ToTs(payload, 0, payload.Length, pts, type, pp.PID);
        }

        private void MuxDtsToTs(PesPacket pp, ElementaryStreamTypes type)
        {
            Int64 pts = 0;
            PesHeader ph = pp.GetHeader();
            if (ph.HasPts == false)
            {
                if (lastPtsList.ContainsKey(pp.PID))
                    pts = lastPtsList[pp.PID];
            }
            else
            {
                pts = ph.Pts;
                lastPtsList[pp.PID] = pts;
            }
            byte[] payload = pp.GetPayload();
            DtsInfo dts = new DtsInfo(payload, 0);
            if (this.processAudio || (dts.Valid && dts.FrameSize == payload.Length))
                MuxSingleDtsToTs(payload, 0, payload.Length, pts, type, pp.PID);
            else
            {
                if (soundFrames.ContainsKey(pp.PID) == false)
                {
                    // skip to the good part.
                    UInt32 mk2 = 0xffffffff;
                    int index = 0;
                    for (index = 0; index < payload.Length; index++)
                    {
                        mk2 <<= 8;
                        mk2 |= payload[index];
                        if (mk2 == Constants.DTS_SYNC)
                            break;
                    }
                    if (index == payload.Length)
                        MuxSingleDtsToTs(payload, 0, payload.Length, pts, type, pp.PID);
                    else
                    {
                        index--;
                        List<byte> framelist = new List<byte>();
                        for (; index < payload.Length; index++)
                            framelist.Add(payload[index]);
                        soundFrames.Add(pp.PID, framelist);
                    }
                }
                else
                    soundFrames[pp.PID].AddRange(payload);
                if (soundFrames.ContainsKey(pp.PID))
                {
                    payload = soundFrames[pp.PID].ToArray();
                    dts = new DtsInfo(payload, 0);
                    int len = 0;
                    while (dts.Valid && dts.FrameSize > 0 && dts.FrameSize + len <= payload.Length)
                    {
                        len += dts.FrameSize;
                        dts = new DtsInfo(payload, len);
                    }
                    if (len > 0)
                    {
                        MuxSingleDtsToTs(payload, 0, len, pts, type, pp.PID);
                        soundFrames[pp.PID].RemoveRange(0, len);
                    }
                }
            }
        }

        private void MuxSingleAc3ToTs(byte[] payload, int offset, int len, Int64 pts, ElementaryStreamTypes type, ushort pid)
        {
            List<byte> pes = new List<byte>();
            bool priority = false;
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x01);
            if (this.fileType == TsFileType.BLU_RAY)
                pes.Add(0xfd);
            else
                pes.Add(0xbd);
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x84);
            pes.Add(0x81);
            pes.Add(0x08);
            pes.Add(0x21);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(0x01);
            pes.Add(0x81);
            if ((type == ElementaryStreamTypes.AUDIO_STREAM_AC3) ||
                (type == ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS))
            {
                AC3Info info = new AC3Info(payload, offset);
                if (this.MlpToAc3 || (info.Valid && info.IndependentStream))
                {
                    pes.Add(0x71);
                    priority = true;
                }
                else
                {
                    pes.Add(0x72);
                    priority = false;
                }
            }
            else if (type == ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD)
            {
                if (payload.Length - offset > 1 &&
                    payload[offset] == (byte)(Constants.AC3_SYNC >> 8) &&
                    payload[offset + 1] == (byte)(Constants.AC3_SYNC & 0xff))
                {
                    pes.Add(0x76);
                    priority = true;
                }
                else
                {
                    pes.Add(0x72);
                    priority = false;
                }
            }
            PesHeader ph = new PesHeader(pes.ToArray());
            ph.Pts = pts;
            PesPacket pp = new PesPacket(ph.Data, 0, ph.Data.Length, pid);
            pp.AddData(payload, offset, len);
            pp.Complete = true;
            MuxPesPacketToTs(pp, priority);
        }

        private void MuxSingleDtsToTs(byte[] payload, int offset, int len, Int64 pts, ElementaryStreamTypes type, ushort pid)
        {
            List<byte> pes = new List<byte>();
            bool priority = false;
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x01);
            if (this.fileType == TsFileType.BLU_RAY)
                pes.Add(0xfd);
            else
                pes.Add(0xbd);
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x84);
            pes.Add(0x81);
            pes.Add(0x08);
            pes.Add(0x21);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(0x01);
            pes.Add(0x81);
            DtsInfo info = new DtsInfo(payload, offset);
            if (this.MlpToAc3 || (info.Valid && info.FrameSize == len))
            {
                pes.Add(0x71);
                priority = true;
            }
            else
            {
                pes.Add(0x72);
                priority = false;
            }
            PesHeader ph = new PesHeader(pes.ToArray());
            ph.Pts = pts;
            PesPacket pp = new PesPacket(ph.Data, 0, ph.Data.Length, pid);
            pp.AddData(payload, offset, len);
            pp.Complete = true;
            MuxPesPacketToTs(pp, priority);
        }

        private ElementaryStreamTypes GetStreamType(ushort pid)
        {
            foreach (StreamInfo si in Psi)
                if (si.ElementaryPID == pid)
                    return si.StreamType;
            return ElementaryStreamTypes.INVALID;
        }
    }

    public class Coordinator
    {
        private PesFile inFile;
        private Muxer outFile;
        Int64 endTrim;
        BackgroundWorker bw;
        bool done;
        Int64 seconds;
        bool subtitles;
        Int64 startOffset;

        public Coordinator()
        {
            inFile = null;
            outFile = null;
            done = false;
            endTrim = -1;
            bw = null;
            seconds = 0;
            subtitles = false;
            startOffset = 0;
        }

        public void StartMuxing(string outPath, BackgroundWorker worker, TsFileType outType, List<ushort> pidsToKeep, TimeSpan ts, TimeSpan te, bool useAsync, PesFile input)
        {
            StartMuxing(outPath, worker, outType, pidsToKeep, ts, te, useAsync, false, false, input, null, TimeSpan.Zero, TimeSpan.Zero);
        }

        public void StartMuxing(string outPath, BackgroundWorker worker, TsFileType outType, List<ushort> pidsToKeep, TimeSpan ts, TimeSpan te, bool useAsync, bool processAudio, bool MlpToAc3, PesFile input, PesFile secondary, TimeSpan offset, TimeSpan chapterLen)
        {
            inFile = input;
            bw = worker;
            List<StreamInfo> sis = new List<StreamInfo>();
            PesPacket[] ppa = null;
            PesPacket[] spa = null;
            BluRayOutput bo = null;

            Int64 startTrim = inFile.StartPcr + ((Int64)ts.TotalSeconds * (Int64)Constants.MPEG2TS_CLOCK_RATE);
            if (startTrim > Constants.MAX_MPEG2TS_CLOCK)
                startTrim -= Constants.MAX_MPEG2TS_CLOCK;
            foreach (StreamInfo si in inFile.StreamInfos)
                if (pidsToKeep.Contains(si.ElementaryPID))
                    sis.Add(si);
            if(null != secondary && pidsToKeep.Contains(secondary.StreamInfos[0].ElementaryPID))
                sis.Add(secondary.StreamInfos[0]);

            switch(outType)
            {
                case TsFileType.BLU_RAY:
                    bo = new BluRayOutput(outPath, chapterLen);
                    outFile = new BlueMux(Path.Combine(outPath, @"BDMV\STREAM\00001.m2ts"), outType, sis, useAsync, processAudio, MlpToAc3);
                    break;

                case TsFileType.MKV:
                    outFile = new MkvMux(outPath, sis, useAsync, processAudio);
                    break;

                case TsFileType.DEMUX:
                    outFile = new DeMux(outPath, sis, useAsync, processAudio);
                    break;

                default:
                    outFile = new BlueMux(outPath, outType, sis, useAsync, processAudio, MlpToAc3);
                    break;
            }

            if (ts.TotalSeconds == 0)
                inFile.Seek(-1);
            else
                inFile.Seek(startTrim);

            if (te.TotalSeconds > 0)
            {
                endTrim = inFile.EndPcr - ((Int64)te.TotalSeconds * (Int64)Constants.MPEG2TS_CLOCK_RATE);
                if (endTrim < 0)
                    endTrim += Constants.MAX_MPEG2TS_CLOCK;
            }
            inFile.SetPcrDelegate(new PcrChanged(UpdatePcr));
            inFile.SetPtsDelegate(new PtsChanged(UpdatePts));
            startOffset = startTrim + ((Int64)offset.TotalSeconds * (Int64)Constants.MPEG2TS_CLOCK_RATE);
            if (startOffset > Constants.MAX_MPEG2TS_CLOCK)
                startOffset -= Constants.MAX_MPEG2TS_CLOCK;
            for (ppa = inFile.GetNextPesPackets(); ppa != null && ppa.Length > 0 && subtitles == false; ppa = inFile.GetNextPesPackets())
            {
                if (worker.CancellationPending || done)
                    goto leave_routine;
                foreach (PesPacket pp in ppa)
                    if (null != pp)
                        outFile.MuxPacket(pp);
                    else
                        goto leave_routine;
            }
            if (subtitles)
            {
                if (null != secondary && ppa != null && ppa.Length > 0)
                {
                    secondary.Seek(-1);
                    spa = secondary.GetNextPesPackets();
                    PesPacket sp = spa[0];
                    PesHeader sh = sp.GetHeader();
                    PesHeader ph = ppa[0].GetHeader();
                    while (ph == null || ph.HasPts == false)
                    {
                        if (worker.CancellationPending || done)
                            goto leave_routine;
                        foreach (PesPacket pp in ppa)
                            if (null != pp)
                                outFile.MuxPacket(pp);
                        ppa = inFile.GetNextPesPackets();
                        if (ppa == null || ppa.Length == 0 || ppa[0] == null)
                            goto leave_routine;
                        ph = ppa[0].GetHeader();
                    }
                    Int64 ptsOffset = ph.Pts - sh.Pts;
                    bool clock = true;
                    for (; ppa != null && ppa.Length > 0 && ppa[0] != null; ppa = inFile.GetNextPesPackets())
                    {
                        foreach (PesPacket pp in ppa)
                        {
                            ph = pp.GetHeader();
                            if (sh != null && ph != null && ph.HasPts)
                            {
                                if (clock)
                                {
                                    Int64 time = sh.Pts + ptsOffset;
                                    if (time < 0)
                                        time += Constants.MAX_PTS_CLOCK;
                                    else if (time > Constants.MAX_PTS_CLOCK)
                                        time -= Constants.MAX_PTS_CLOCK;
                                    sh.Pts = time;
                                    for (int i = 9; i < 14; i++)
                                        sp[i] = sh[i]; // copy PTS
                                    if (sh.HasDts)
                                    {
                                        time = sh.Dts + ptsOffset;
                                        if (time < 0)
                                            time += Constants.MAX_PTS_CLOCK;
                                        else if (time > Constants.MAX_PTS_CLOCK)
                                            time -= Constants.MAX_PTS_CLOCK;
                                        sh.Dts = time;
                                        for (int i = 14; i < 19; i++)
                                            sp[i] = sh[i]; // copy DTS
                                    }
                                    clock = false;
                                }
                                Int64 delta = sh.Pts - ph.Pts;
                                if (delta > (0 - Constants.PTS_CLOCK_RATE) && delta < Constants.PTS_CLOCK_RATE)
                                {
                                    outFile.MuxPacket(sp);
                                    spa = secondary.GetNextPesPackets();
                                    if (spa != null && spa.Length > 0 && spa[0] != null)
                                    {
                                        sp = spa[0];
                                        sh = sp.GetHeader();
                                        clock = true;
                                    }
                                }
                            }
                            outFile.MuxPacket(pp);
                        }
                    }
                }
                else
                {
                    for (; ppa != null && ppa.Length > 0; ppa = inFile.GetNextPesPackets())
                    {
                        if (worker.CancellationPending || done)
                            goto leave_routine;
                        foreach (PesPacket pp in ppa)
                            if (null != pp)
                                outFile.MuxPacket(pp);
                            else
                                goto leave_routine;
                    }
                }
            }
        leave_routine:
            outFile.Close();
            if (outType == TsFileType.BLU_RAY)
            {
                bo.Author(outFile.EpData, outFile.Psi, outFile.CurrentPacketNumber);
            }
        }

        public void UpdatePcr(Int64 pcr)
        {
            if (subtitles == false)
            {
                Int64 delta = startOffset - pcr;
                if (delta > (0 - Constants.MPEG2TS_CLOCK_RATE) && delta < Constants.MPEG2TS_CLOCK_RATE)
                    subtitles = true;
            }
            outFile.PcrChanged(pcr);
            if (endTrim != -1)
            {
                Int64 span = endTrim - pcr;
                if (span < 0)
                    span += Constants.MAX_MPEG2TS_CLOCK;
                if ((span > Constants.MAX_MPEG2TS_CLOCK / 2))
                {
                    // trim end reached
                    done = true;
                }
                span /= Constants.MPEG2TS_CLOCK_RATE;
                if (span != seconds)
                {
                    seconds = span;
                    bw.ReportProgress(0, new TimeSpan(span * 10000000));
                }
            }
            else
            {
                Int64 span = inFile.EndPcr - pcr;
                if (span < 0)
                    span += Constants.MAX_MPEG2TS_CLOCK;
                span /= Constants.MPEG2TS_CLOCK_RATE;
                if (span != seconds)
                {
                    seconds = span;
                    bw.ReportProgress(0, new TimeSpan(span * 10000000));
                }
            }
        }

        public void UpdatePts(Int64 pts, ushort pid)
        {
        }
    }
}
