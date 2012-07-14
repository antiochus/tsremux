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
using System.Globalization;

namespace TsRemux
{
    public enum LacingType
    {
        NoLacing = 0,
        XiphLacing,
        FixedSizeLacing,
        EbmlLacing
    }

    public class EbmlElement
    {
        protected Int64 id;
        protected Int64 size;
        protected Int64 pos;
        protected Stream fs;

        public static EbmlElement ParseEbml(Stream fs)
        {
            Int64 pos = fs.Position;
            EbmlElement ebml = null;
            if (null == fs || fs.Length == 0)
                return null;
            int b = fs.ReadByte();
            if (b == -1)
                goto cleanup;
            int len = VintLength((byte)b);
            if ((fs.Length - fs.Position) < (Int64)len)
                goto cleanup;
            fs.Position -= 1;
            Int64 id = 0;
            for (int i = 0; i < len; i++)
            {
                id <<= 8;
                id |= (byte)fs.ReadByte();
            }
            b = fs.ReadByte();
            if (b == -1)
                goto cleanup;
            len = VintLength((byte)b);
            if ((fs.Length - fs.Position) < (Int64)len)
                goto cleanup;
            fs.Position -= 1;
            Int64 size = VintToInt64(fs);
            if ((fs.Length - fs.Position) < size)
                goto cleanup;
            ebml = new EbmlElement(id, size, fs.Position, fs);

        cleanup:
            fs.Position = pos;
            return ebml;
        }

        public Int64 Id
        {
            get
            {
                return id;
            }
        }

        public Int64 Size
        {
            get
            {
                return size;
            }
        }

        public Stream DataStream
        {
            get
            {
                fs.Position = pos;
                return fs;
            }
        }

        protected Int64 Position
        {
            get
            {
                return pos;
            }
        }

        public EbmlElement[] Children
        {
            get
            {
                List<EbmlElement> kids = new List<EbmlElement>();
                EbmlElement eb = null;
                Stream fs = DataStream;
                do
                {
                    eb = ParseEbml(fs);
                    if (null != eb)
                    {
                        kids.Add(eb);
                        Int64 p = eb.Position + eb.Size;
                        fs.Position = p;
                    }
                } while (eb != null && fs.Position < (pos + size));
                return kids.ToArray();
            }
        }

        protected EbmlElement(Int64 id, Int64 size, Int64 pos, Stream fs)
        {
            this.id = id;
            this.size = size;
            this.pos = pos;
            this.fs = fs;
        }

        public static byte VintLength(byte vint)
        {
            byte len = 1;
            for (int i = 7; (((vint >> i) & 0x01) == 0) && i >= 0; i--)
                len++;
            return len;
        }

        public static Int64 VintToInt64(Stream fs)
        {
            byte b = (byte)fs.ReadByte();
            int len = VintLength(b);
            Int64 ret = (((1 << (8 - len)) - 1) & b) << (8 * (len - 1));
            for (int i = 1; i < len; i++)
                ret += (((byte)fs.ReadByte()) << (8 * (len - 1 - i)));
            return ret;
        }

        private static readonly Int64[] vsint_subtr = new Int64[8] {
            0x3f, 0x1ffff, 0xfffff, 0x7ffffff, 0x3ffffffff,
            0x1ffffffffff, 0xffffffffffffff, 0x7fffffffffffff };

        protected static Int64 VsintToInt64(Stream fs)
        {
            int b = fs.ReadByte();
            int len = VintLength((byte)b);
            fs.Position -= 1;
            Int64 ret = VintToInt64(fs);
            return ret - vsint_subtr[len - 1];
        }
    }

    public struct TrackInfo
    {
        public TrackInfo(ushort pid, string codec, byte[] data, EbmlElement info)
        {
            this.pid = pid;
            this.codec = codec;
            this.data = data;
            this.info = info;
        }
        public ushort pid;
        public string codec;
        public byte[] data;
        public EbmlElement info;
    }

    public class MkvPesFile : PesFile
    {
        private SortedList<Int64,EbmlElement> Clusters;
        private int CurrentIndex;
        private Dictionary<ushort, TrackInfo> TrackList;

        public MkvPesFile(BackgroundWorker bw)
            : base(bw)
        {
            Clusters = new SortedList<long, EbmlElement>();
            TrackList = new Dictionary<ushort, TrackInfo>();
            CurrentIndex = -1;
        }

        public override DTCP_Descriptor DtcpInfo
        {
            get { return null; }
        }

        protected override void GetInitialValues()
        {
            fs.Seek(0, SeekOrigin.Begin);
            UInt32 header = 0xffffffff;
            EbmlElement mkv = null;
            byte b = 0;
            for (int i = 0; i < Constants.DISK_BUFFER; i++)
            {
                int ib = fs.ReadByte();
                if (ib == -1)
                    throw new FormatException("The specified file is too short");
                b = (byte)(ib & 0xff);
                header <<= 8;
                header |= b;
                if (header == Constants.MKVSEGMENT_START)
                {
                    fs.Seek(-4, SeekOrigin.Current);
                    mkv = EbmlElement.ParseEbml(fs);
                    if (null == mkv)
                        throw new FormatException("Invalid/Corrupted MKV file");
                    break;
                }
            }
            EbmlElement[] clusts = mkv.Children;
            foreach (EbmlElement clust in clusts)
            {
                ReportProgress((int)(50 * clust.DataStream.Position / clust.DataStream.Length));
                if (clust.Id == Constants.MKVCLUSTER_START)
                {
                    Clusters.Add(GetClusterClock(clust), clust);
                }
                else if (clust.Id == Constants.MKVTRACKINFO_START)
                {
                    EbmlElement[] tracks = clust.Children;
                    ushort id = 0;
                    List<StreamInfo> mkvStreams = new List<StreamInfo>();
                    foreach (EbmlElement track in tracks)
                    {
                        if (track.Id == 0xae)
                        {
                            EbmlElement[] trackAttributes = track.Children;
                            string codec = null;
                            ushort pid = 0;
                            UInt64 uid = 0;
                            byte[] data = null;
                            EbmlElement elementaryInfo = null;
                            foreach (EbmlElement trackAtt in trackAttributes)
                            {
                                switch (trackAtt.Id)
                                {
                                    case 0xd7: // Track number
                                        for (Int64 i = 0; i < trackAtt.Size; i++)
                                        {
                                            pid <<= 8;
                                            pid |= (byte)fs.ReadByte();
                                        }
                                        break;
                                    case 0x86: // Codec ID
                                        byte[] cid = new byte[(int)trackAtt.Size];
                                        trackAtt.DataStream.Read(cid, 0, (int)trackAtt.Size);
                                        codec = Encoding.ASCII.GetString(cid);
                                        break;
                                    case 0x63a2: // Coded Private
                                        data = new byte[(int)trackAtt.Size];
                                        trackAtt.DataStream.Read(data, 0, (int)trackAtt.Size);
                                        break;
                                    case 0x73c5: // Track UID
                                        for (Int64 i = 0; i < trackAtt.Size; i++)
                                        {
                                            uid <<= 8;
                                            uid |= (byte)fs.ReadByte();
                                        }
                                        break;
                                    case 0xe0: // Video elementary stream info
                                        elementaryInfo = trackAtt;
                                        break;
                                    case 0xe1: // Audio elementary stream info
                                        elementaryInfo = trackAtt;
                                        break;
                                }
                            }
                            if (codec == null || pid == 0)
                                throw new FormatException("Track info is invalid");
                            if (0 == string.Compare(codec, "V_MPEG2", true, CultureInfo.InvariantCulture) ||
                                0 == string.Compare(codec, "V_MPEG4/ISO/AVC", true, CultureInfo.InvariantCulture) ||
                                0 == string.Compare(codec, "V_MS/VFW/FOURCC", true, CultureInfo.InvariantCulture) ||
                                0 == string.Compare(codec, "A_AC3", true, CultureInfo.InvariantCulture) ||
                                0 == string.Compare(codec, "A_DTS", true, CultureInfo.InvariantCulture))
                            {
                                id++;
                                TrackInfo ti = new TrackInfo(id, codec, data, elementaryInfo);
                                TrackList.Add(id, ti);
                                StreamInfo si = null;
                                if (0 == string.Compare(codec, "V_MPEG2", true, CultureInfo.InvariantCulture))
                                    si = new StreamInfo(ElementaryStreamTypes.VIDEO_STREAM_MPEG2, id);
                                else if (0 == string.Compare(codec, "V_MPEG4/ISO/AVC", true, CultureInfo.InvariantCulture))
                                    si = new StreamInfo(ElementaryStreamTypes.VIDEO_STREAM_H264, id);
                                else if (0 == string.Compare(codec, "V_MS/VFW/FOURCC", true, CultureInfo.InvariantCulture))
                                    si = new StreamInfo(ElementaryStreamTypes.VIDEO_STREAM_VC1, id);
                                else if (0 == string.Compare(codec, "A_AC3", true, CultureInfo.InvariantCulture))
                                    si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_AC3, id);
                                else if (0 == string.Compare(codec, "A_DTS", true, CultureInfo.InvariantCulture))
                                    si = new StreamInfo(ElementaryStreamTypes.AUDIO_STREAM_DTS, id);
                                mkvStreams.Add(si);
                            }
                        }
                    }
                    if (mkvStreams.Count > 0)
                        sis = mkvStreams.ToArray();
                }
            }
            GetTimeStamps();
            CurrentIndex = 0;
        }

        private Int64 GetClusterClock(EbmlElement cluster)
        {
            Int64 clock = -1;
            if(cluster.Id != Constants.MKVCLUSTER_START)
                throw new FormatException("Bad cluster ID - Invalid/Corrupted MKV file");
            EbmlElement[] children = cluster.Children;
            foreach (EbmlElement time in children)
            {
                if (time.Id == Constants.MKVTIMECODE_START)
                {
                    clock = 0;
                    Stream s = time.DataStream;
                    for (Int64 i = 0; i < time.Size; i++)
                    {
                        clock <<= 8;
                        clock |= (byte)fs.ReadByte();
                    }
                    break;
                }
            }
            if (-1 == clock)
                throw new FormatException("Could not find timecode in cluster.");
            return clock;
        }

        private PesPacket BuildAc3Pes(Int64 timestamp, byte[] data, ushort pid)
        {
            List<byte> pes = new List<byte>();
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(Constants.PES_PRIVATE1);
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
            pes.Add(0x71);
            PesHeader ph = new PesHeader(pes.ToArray());
            ph.Pts = timestamp;
            PesPacket pp = new PesPacket(ph.Data, 0, ph.Data.Length, pid);
            pp.AddData(data, 0, data.Length);
            pp.Complete = true;
            return pp;
        }

        private PesPacket BuildDtsPes(Int64 timestamp, byte[] data, ushort pid)
        {
            return BuildAc3Pes(timestamp, data, pid);
        }
        private PesPacket BuildMpeg2Pes(Int64 timestamp, byte[] data, ushort pid)
        {
            List<byte> pes = new List<byte>();
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(Constants.PES_VIDEO);
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x84);
            pes.Add(0x81);
            pes.Add(0x05);
            pes.Add(0x21);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(0x00);
            pes.Add(0x01);
            PesHeader ph = new PesHeader(pes.ToArray());
            ph.Pts = timestamp;
            PesPacket pp = new PesPacket(ph.Data, 0, ph.Data.Length, pid);
            pp.AddData(data, 0, data.Length);
            pp.Complete = true;
            return pp;
        }
        private PesPacket BuildAvcPes(Int64 timestamp, byte[] data, ushort pid)
        {
            return BuildMpeg2Pes(timestamp, data, pid);
        }
        private PesPacket BuildVc1Pes(Int64 timestamp, byte[] data, ushort pid)
        {
            List<byte> pes = new List<byte>();
            pes.Add(0x00);
            pes.Add(0x00);
            pes.Add(0x01);
            pes.Add(Constants.PES_VIDEO_VC1);
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
            pes.Add(0x55);
            PesHeader ph = new PesHeader(pes.ToArray());
            ph.Pts = timestamp;
            PesPacket pp = new PesPacket(ph.Data, 0, ph.Data.Length, pid);
            pp.AddData(data, 0, data.Length);
            pp.Complete = true;
            return pp;
        }

        public override PesPacket[] GetNextPesPackets()
        {
            if (CurrentIndex >= Clusters.Keys.Count)
                return null;
            EbmlElement cluster = Clusters[Clusters.Keys[CurrentIndex]];
            EbmlElement[] blocks = cluster.Children;
            List<PesPacket> packList = new List<PesPacket>();
            Int64 clock = GetClusterClock(cluster);
            if (pcrDelegate != null)
                pcrDelegate(clock * (Constants.MPEG2TS_CLOCK_RATE / 1000));
            foreach (EbmlElement bl in blocks)
            {
                EbmlElement block = null;
                switch (bl.Id)
                {
                    case 0xa0: // block group
                        EbmlElement[] cbls = bl.Children;
                        foreach (EbmlElement bl2 in cbls)
                            if (bl2.Id == 0xa1)
                            {
                                block = bl2;
                                break;
                            }
                        break;
                    case 0xa3:
                        block = bl;
                        break;
                }
                if (null != block)
                {
                    Stream stm = block.DataStream;
                    Int64 endPos = stm.Position + block.Size;
                    Int64 track = EbmlElement.VintToInt64(stm);
                    Int16 time = (short)stm.ReadByte();
                    time <<= 8;
                    time += (short)stm.ReadByte();
                    Int64 pts = clock + time;
                    pts *= 90;
                    byte flags = (byte)stm.ReadByte();
                    LacingType lacing = (LacingType)((flags >> 1) & 0x03);
                    if (lacing != LacingType.NoLacing && lacing != LacingType.FixedSizeLacing)
                        throw new FormatException("Variable lacing is not yet supported");
                    if (lacing == LacingType.FixedSizeLacing)
                        stm.Position += 1;
                    byte[] data = new byte[endPos - stm.Position];
                    stm.Read(data, 0, (int)(endPos - stm.Position));
                    if (data.Length > 0 && TrackList.ContainsKey((ushort)track))
                    {
                        TrackInfo ti = TrackList[(ushort)track];
                        if (ptsDelegate != null)
                            ptsDelegate(pts, ti.pid);
                        switch (sis[(ushort)track - 1].StreamType)
                        {
                            case ElementaryStreamTypes.AUDIO_STREAM_AC3:
                                packList.Add(BuildAc3Pes(pts, data, ti.pid));
                                break;
                            case ElementaryStreamTypes.AUDIO_STREAM_DTS:
                                packList.Add(BuildDtsPes(pts, data, ti.pid));
                                break;
                            case ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                                packList.Add(BuildMpeg2Pes(pts, data, ti.pid));
                                break;
                            case ElementaryStreamTypes.VIDEO_STREAM_H264:
                                packList.Add(BuildAvcPes(pts, data, ti.pid));
                                break;
                            case ElementaryStreamTypes.VIDEO_STREAM_VC1:
                                packList.Add(BuildVc1Pes(pts, data, ti.pid));
                                break;
                        }
                    }
                }
            }
            CurrentIndex++;
            if(packList.Count == 0)
                return null;
            return packList.ToArray();
        }

        public override void Seek(long pcr)
        {
            if (-1 == pcr)
            {
                CurrentIndex = 0;
                return;
            }
            CurrentIndex = Clusters.IndexOfKey(pcr);
            if (-1 != CurrentIndex)
                return;
            Clusters.Add(pcr, null);
            CurrentIndex = Clusters.IndexOfKey(pcr);
            if (CurrentIndex > 0)
                CurrentIndex--;
            Clusters.Remove(pcr);
        }

        private void GetTimeStamps()
        {
            startPcr = GetClusterClock(Clusters[Clusters.Keys[0]]) * (Constants.MPEG2TS_CLOCK_RATE / 1000);
            endPcr = GetClusterClock(Clusters[Clusters.Keys[Clusters.Keys.Count - 1]]) * (Constants.MPEG2TS_CLOCK_RATE / 1000);
        }
    }
}
