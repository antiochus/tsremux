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

namespace TsRemux
{
    public enum TsFileType
    {
        UNKNOWN = 0,
        TS,
        M2TS,
        EVOB,
        ELEMENTARY,
        PES_ELEMENTARY,
        SUP_ELEMENTARY,
        BLU_RAY,
        MKV,
        DEMUX
    }

    public enum DtcpCci
    {
        CopyFree = 0,
        NoMoreCopies,
        CopyOnce,
        CopyNever
    }

    public enum ElementaryStreamTypes
    {
        INVALID = 0,
        VIDEO_STREAM_MPEG1 = 0x01,
        VIDEO_STREAM_MPEG2 = 0x02,
        AUDIO_STREAM_MPEG1 = 0x03, // all layers including mp3
        AUDIO_STREAM_MPEG2 = 0x04,
        VIDEO_STREAM_H264 = 0x1b,
        AUDIO_STREAM_LPCM = 0x80,
        AUDIO_STREAM_AC3 = 0x81,
        AUDIO_STREAM_DTS = 0x82,
        AUDIO_STREAM_AC3_TRUE_HD = 0x83,
        AUDIO_STREAM_AC3_PLUS = 0x84,
        AUDIO_STREAM_DTS_HD = 0x85,
        AUDIO_STREAM_DTS_HD_MASTER_AUDIO = 0x86,
        PRESENTATION_GRAPHICS_STREAM = 0x90,
        INTERACTIVE_GRAPHICS_STREAM = 0x91,
        SUBTITLE_STREAM = 0x92,
        SECONDARY_AUDIO_AC3_PLUS = 0xa1,
        SECONDARY_AUDIO_DTS_HD = 0xa2,
        VIDEO_STREAM_VC1 = 0xea
    }

    public enum VideoFormat
    {
        Reserved = 0,
        i480,
        i576,
        p480,
        i1080,
        p720,
        p1080,
        p576
    }

    public enum FrameRate
    {
        Reserved = 0,
        f23_976,
        f24,
        f25,
        f29_97,
        f50 = 6,
        f59_94
    }

    public enum AspectRatio
    {
        Reserved = 0,
        a4_3 = 2,
        a16_9
    }

    public enum AudioPresentationType
    {
        Reserved = 0,
        mono,
        stereo = 3,
        multi = 6,
        combo = 12
    }

    public enum SamplingFrequency
    {
        Reserved = 0,
        kHz48,
        kHz96 = 4,
        kHz192,
        kHz48_192 = 12,
        kHz48_96 = 14
    }

    public class BluRayOutput
    {
        private string path;
        private TimeSpan chapterLen;
        private static readonly byte[] index_bdmv = new byte[526] {
            0x49, 0x4e, 0x44, 0x58, 0x30, 0x31, 0x30, 0x30, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x00, 0x00, 0x78,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x26, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00,
            0x00, 0x00, 0x40, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x40, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x92, 0x00, 0x00, 0x00, 0x18,
            0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x01, 0x7e,
            0x49, 0x44, 0x45, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x62, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x36, 0x10, 0x13, 0x00, 0x01,
            0x54, 0x52, 0x20, 0x30, 0x2e, 0x30, 0x2e, 0x31, 0x2e, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x06, 0xff, 0xff, 0x42, 0x20, 0x07, 0x08, 0x08, 0x00, 0x54, 0x53, 0x00, 0x90, 0x0a, 0x54,
            0x52, 0x20, 0x30, 0x2e, 0x30, 0x2e, 0x31, 0x2e, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
            0x00, 0x00, 0x00, 0x01, 0x30, 0x30, 0x30, 0x30, 0x30, 0x01, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] MovieObject_bdmv = new byte[278] {
            0x4d, 0x4f, 0x42, 0x4a, 0x30, 0x31, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xea, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x03, 0x80, 0x00, 0x00, 0x04, 0x50, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00,
            0x00, 0x00, 0x50, 0x40, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x82,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x21, 0x81, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x09, 0x50, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x0a, 0x00, 0x00, 0x00, 0x03, 0x50, 0x40, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00,
            0xff, 0xff, 0x48, 0x40, 0x03, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0xff, 0xff, 0x22, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x50, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x0a, 0x00, 0x00, 0x00, 0x04, 0x50, 0x40, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00,
            0x00, 0x00, 0x48, 0x40, 0x03, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x21, 0x01,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x21, 0x81, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x05, 0x50, 0x40, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x40, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
            0x00, 0x01, 0x50, 0x40, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0xff, 0xff, 0x50, 0x40,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x21, 0x81, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] PlayList_00000_mpls = new byte[8] { 0x4d, 0x50, 0x4c, 0x53, 0x30, 0x31, 0x30, 0x30 };
        private static readonly byte[] AppInfoPlayList = new byte[18] {
            0x00, 0x00, 0x00, 0x0e, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00 };
        private static readonly byte[] PgStreamAttributes = new byte[6] { 0x05, 0x90, 0x65, 0x6e, 0x67, 0x00 };
        private static readonly byte[] ClipInfo_0000_clpi = new byte[8] { 0x48, 0x44, 0x4d, 0x56, 0x30, 0x31, 0x30, 0x30 };
        private static readonly byte[] TsTypeInfoBlock = new byte[32] {
            0x00, 0x1e, 0x80, 0x48, 0x44, 0x4d, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public BluRayOutput(string path, TimeSpan chapterLen)
        {
            this.path = path;
            this.chapterLen = chapterLen;
            if (File.Exists(path))
                File.Delete(path);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, @"BDMV"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\AUXDATA"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\BACKUP"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\BACKUP\BDJO"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\BACKUP\CLIPINF"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\BACKUP\PLAYLIST"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\BDJO"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\CLIPINF"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\JAR"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\META"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\PLAYLIST"));
            Directory.CreateDirectory(Path.Combine(path, @"BDMV\STREAM"));
            Directory.CreateDirectory(Path.Combine(path, @"CERTIFICATE"));
            Directory.CreateDirectory(Path.Combine(path, @"CERTIFICATE\BACKUP"));
            File.WriteAllBytes(Path.Combine(path, @"BDMV\index.bdmv"), index_bdmv);
            File.Copy(Path.Combine(path, @"BDMV\index.bdmv"), Path.Combine(path, @"BDMV\BACKUP\index.bdmv"),true);
            File.WriteAllBytes(Path.Combine(path, @"BDMV\MovieObject.bdmv"), MovieObject_bdmv);
            File.Copy(Path.Combine(path, @"BDMV\MovieObject.bdmv"), Path.Combine(path, @"BDMV\BACKUP\MovieObject.bdmv"),true);
        }

        public void Author(EpElement[] EpInfo, StreamInfo[] sis, UInt32 numOfSourcePackets)
        {
            List<ushort> Pids = new List<ushort>();
            List<byte[]> StreamCodingInfos = new List<byte[]>();
            List<byte[]> AudioEntries = new List<byte[]>();
            List<byte[]> AudioAttributes = new List<byte[]>();
            List<byte[]> PgEntries = new List<byte[]>();
            List<byte[]> PgAttributes = new List<byte[]>();
            byte[] VideoEntry = null;
            byte[] VideoAttribute = null;
            ElementaryStreamTypes VideoType = ElementaryStreamTypes.INVALID;
            foreach(StreamInfo si in sis)
            {
                switch (si.StreamType)
                {
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3:
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS:
                    case ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD:
                    case ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO:
                    case ElementaryStreamTypes.AUDIO_STREAM_LPCM:
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG1:
                    case ElementaryStreamTypes.AUDIO_STREAM_MPEG2:
                        byte[] AudioEntry = BuildStreamEntry(si.ElementaryPID);
                        byte[] AudioAttribute = BuildAudioStreamAttributes((byte)si.StreamType,si.AudioPresentationType,si.SamplingFrequency);
                        AudioEntries.Add(AudioEntry);
                        AudioAttributes.Add(AudioAttribute);
                        byte[] AudioCodingInfo = BuildAudioStreamCodingInfo(si.StreamType, si.AudioPresentationType, si.SamplingFrequency);
                        Pids.Add(si.ElementaryPID);
                        StreamCodingInfos.Add(AudioCodingInfo);
                        break;
                    case ElementaryStreamTypes.VIDEO_STREAM_H264:
                    case ElementaryStreamTypes.VIDEO_STREAM_MPEG1:
                    case ElementaryStreamTypes.VIDEO_STREAM_MPEG2:
                    case ElementaryStreamTypes.VIDEO_STREAM_VC1:
                        VideoType = si.StreamType;
                        VideoEntry = BuildStreamEntry(si.ElementaryPID);
                        VideoAttribute = BuildVideoStreamAttributes((byte)si.StreamType,si.VideoFormat,si.FrameRate);
                        byte[] VideoCodingInfo = BuildVideoStreamCodingInfo(si.StreamType, si.VideoFormat, si.FrameRate, si.AspectRatio);
                        Pids.Add(si.ElementaryPID);
                        StreamCodingInfos.Add(VideoCodingInfo);
                        break;
                    case ElementaryStreamTypes.PRESENTATION_GRAPHICS_STREAM:
                        byte[] PgEntry = BuildStreamEntry(si.ElementaryPID);
                        PgEntries.Add(PgEntry);
                        PgAttributes.Add(PgStreamAttributes);
                        byte[] PgCodingInfo = BuildPgStreamCodingInfo();
                        Pids.Add(si.ElementaryPID);
                        StreamCodingInfos.Add(PgCodingInfo);
                        break;
                }
            }
            byte[][] PlayItems = new byte[1][];
            UInt32 Start = (UInt32)((EpInfo[0].PTS >> 1) & 0xffffffff);
            UInt32 End = (UInt32)((EpInfo[EpInfo.Length - 1].PTS >> 1) & 0xffffffff);
            UInt32 Interval = ((UInt32)(chapterLen.TotalMinutes)) * 2700000;
            byte[] StnTable = BuildStnTable(VideoEntry, VideoAttribute, AudioEntries.ToArray(), AudioAttributes.ToArray(), PgEntries.ToArray(), PgAttributes.ToArray());
            PlayItems[0] = BuildFirstPlayItem(0, Start, End, StnTable);
            byte[] PlayList = BuildPlayList(PlayItems);
            byte[] PlayListMark = BuildFirstPlayMarks(Start, End, Interval);
            byte[] mlps = Build_mlps(PlayList, PlayListMark);
            File.WriteAllBytes(Path.Combine(path, @"BDMV\PLAYLIST\00000.mpls"), mlps);
            File.Copy(Path.Combine(path, @"BDMV\PLAYLIST\00000.mpls"), Path.Combine(path, @"BDMV\BACKUP\PLAYLIST\00000.mpls"),true);

            byte[] ClipInfo = BuildClipInfo(numOfSourcePackets,EpInfo);
            byte[] SequenceInfo = BuildSequenceInfo(Start, End);
            byte[] ProgramInf = BuildProgramInfo(Pids.ToArray(), StreamCodingInfos.ToArray());
            byte[] EpMap = BuildEpMap(EpInfo);
            byte[] CPI = BuildCpi(EpMap);
            byte[] clpi = Build_clpi(ClipInfo, SequenceInfo, ProgramInf, CPI);
            File.WriteAllBytes(Path.Combine(path, @"BDMV\CLIPINF\00001.clpi"), clpi);
            File.Copy(Path.Combine(path, @"BDMV\CLIPINF\00001.clpi"), Path.Combine(path, @"BDMV\BACKUP\CLIPINF\00001.clpi"),true);
        }

        private byte[] BuildStreamEntry(ushort pid)
        {
            byte[] StreamEntry = new byte[10] {
                0x09, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            StreamEntry[2] = (byte)((pid >> 8) & 0xff);
            StreamEntry[3] = (byte)(pid & 0xff);
            return StreamEntry;
        }

        private byte[] BuildVideoStreamAttributes(byte type, VideoFormat vf, FrameRate fr)
        {
            if (type != (byte)ElementaryStreamTypes.VIDEO_STREAM_VC1
                && type != (byte)ElementaryStreamTypes.VIDEO_STREAM_MPEG2
                && type != (byte)ElementaryStreamTypes.VIDEO_STREAM_H264)
                throw new FormatException(String.Format("Video stream of type {0} is not supported by Blu Ray", type));
            byte[] attributes = new byte[6];
            attributes[0] = 0x05;
            attributes[1] = type;
            attributes[2] = (byte)((((byte)vf) << 4) & 0xf0);
            attributes[2] |= (byte)(((byte)fr) & 0x0f);
            attributes[3] = attributes[4] = attributes[5] = 0;
            return attributes;
        }

        private byte[] BuildAudioStreamAttributes(byte type, AudioPresentationType vf, SamplingFrequency fr)
        {
            if (type != (byte)ElementaryStreamTypes.AUDIO_STREAM_AC3
                && type != (byte)ElementaryStreamTypes.AUDIO_STREAM_AC3_PLUS
                && type != (byte)ElementaryStreamTypes.AUDIO_STREAM_AC3_TRUE_HD
                && type != (byte)ElementaryStreamTypes.AUDIO_STREAM_DTS
                && type != (byte)ElementaryStreamTypes.AUDIO_STREAM_DTS_HD
                && type != (byte)ElementaryStreamTypes.AUDIO_STREAM_DTS_HD_MASTER_AUDIO
                && type != (byte)ElementaryStreamTypes.AUDIO_STREAM_LPCM)
                throw new FormatException(String.Format("Audio stream of type {0} is not supported by Blu Ray", type));
            byte[] attributes = new byte[6];
            attributes[0] = 0x05;
            attributes[1] = type;
            attributes[2] = (byte)((((byte)vf) << 4) & 0xf0);
            attributes[2] |= (byte)(((byte)fr) & 0x0f);
            attributes[3] = 0x65;
            attributes[4] = 0x6e;
            attributes[5] = 0x67;
            return attributes;
        }

        private byte[] BuildStnTable(byte[] VideoEntry, byte[] VideoAttributes, byte[][] AudioEntries, byte[][] AudioAttributes, byte[][] PgEntries, byte[][] PgAttributes)
        {
            List<byte> table = new List<byte>();
            List<byte> temp = new List<byte>();
            table.Add(0x00);
            table.Add(0x00);
            table.Add(0x01);
            if (AudioEntries.Length > 0)
                table.Add((byte)AudioEntries.Length);
            else
                table.Add(0x00);
            if (PgEntries.Length > 0)
                table.Add((byte)PgEntries.Length);
            else
                table.Add(0x00);
            for (int i = 0; i < 9; i++)
                table.Add(0x00);
            table.AddRange(VideoEntry);
            table.AddRange(VideoAttributes);
            for (int i = 0; i < AudioEntries.Length; i++)
            {
                table.AddRange(AudioEntries[i]);
                table.AddRange(AudioAttributes[i]);
            }
            for (int i = 0; i < PgEntries.Length; i++)
            {
                table.AddRange(PgEntries[i]);
                table.AddRange(PgAttributes[i]);
            }
            UInt32 len = (UInt32)table.Count;
            temp.Add((byte)((len >> 8) & 0xff));
            temp.Add((byte)(len & 0xff));
            temp.AddRange(table);
            return temp.ToArray();
        }

        private byte[] UintToByteArraryNetwork(UInt32 value)
        {
            byte[] ret = new byte[4];
            ret[0] = (byte)((value >> 24) & 0xff);
            ret[1] = (byte)((value >> 16) & 0xff);
            ret[2] = (byte)((value >> 8) & 0xff);
            ret[3] = (byte)(value & 0xff);
            return ret;
        }

        private byte[] Build_mlps(byte[] PlayList, byte[] PlayListMark)
        {
            List<byte> mlps = new List<byte>();
            mlps.AddRange(PlayList_00000_mpls);
            UInt32 temp = (UInt32)AppInfoPlayList.Length;
            temp += 40;
            mlps.AddRange(UintToByteArraryNetwork(temp));
            temp += (UInt32)PlayList.Length;
            mlps.AddRange(UintToByteArraryNetwork(temp));
            for (int i = 0; i < 24; i++)
                mlps.Add(0x00);
            mlps.AddRange(AppInfoPlayList);
            mlps.AddRange(PlayList);
            mlps.AddRange(PlayListMark);
            return mlps.ToArray();
        }

        byte[] BuildPlayList(byte[][] PlayItems)
        {
            List<byte> playList = new List<byte>();
            UInt32 playItemNum = (UInt32)PlayItems.Length;
            playItemNum <<= 16;
            playItemNum &= 0xffff0000;
            UInt32 len = (UInt32)PlayItems.Length + 6;
            playList.AddRange(UintToByteArraryNetwork(len));
            playList.Add(0x00);
            playList.Add(0x00);
            playList.AddRange(UintToByteArraryNetwork(playItemNum));
            for (int i = 0; i < PlayItems.Length; i++)
                playList.AddRange(PlayItems[i]);
            return playList.ToArray();
        }

        byte[] BuildFirstPlayItem(byte stc_id, UInt32 start, UInt32 end, byte[] StnTable)
        {
            List<byte> playItem = new List<byte>();
            List<byte> playTemp = new List<byte>();
            for (int i = 0; i < 4; i++)
                playItem.Add(0x30);
            playItem.Add(0x31);
            playItem.Add(0x4d);
            playItem.Add(0x32);
            playItem.Add(0x54);
            playItem.Add(0x53);
            playItem.Add(0x00);
            playItem.Add(0x01);
            playItem.Add(stc_id);
            playItem.AddRange(UintToByteArraryNetwork(start));
            playItem.AddRange(UintToByteArraryNetwork(end));
            for (int i = 0; i < 12; i++)
                playItem.Add(0x00);
            playItem.AddRange(StnTable);
            UInt32 len = (UInt32)playItem.Count;
            playTemp.Add((byte)((len >> 8) & 0xff));
            playTemp.Add((byte)(len & 0xff));
            playTemp.AddRange(playItem);
            return playTemp.ToArray();
        }

        byte[] BuildFirstPlayMarks(UInt32 start, UInt32 end, UInt32 interval)
        {
            List<byte> marks = new List<byte>();
            List<byte> temp = new List<byte>();
            UInt32 num = 0;
            for (UInt32 i = start; i < end; i += interval, num++)
            {
                marks.Add(0x00);
                marks.Add(0x01);
                marks.Add(0x00);
                marks.Add(0x00);
                UInt32 time = i;
                marks.AddRange(UintToByteArraryNetwork(time));
                marks.Add(0xff);
                marks.Add(0xff);
                time = 0;
                marks.AddRange(UintToByteArraryNetwork(time));
            }
            UInt32 len = (UInt32)marks.Count;
            len += 2;
            temp.AddRange(UintToByteArraryNetwork(len));
            temp.Add((byte)((num >> 8) & 0xff));
            temp.Add((byte)(num & 0xff));
            temp.AddRange(marks);
            return temp.ToArray();
        }

        byte[] Build_clpi(byte[] ClipInfo, byte[] SequenceInfo, byte[] ProgramInfo, byte[] CPI)
        {
            List<byte> clpi = new List<byte>();
            clpi.AddRange(ClipInfo_0000_clpi);
            UInt32 len = 40;
            len += (UInt32)ClipInfo.Length;
            clpi.AddRange(UintToByteArraryNetwork(len));
            len += (UInt32)SequenceInfo.Length;
            clpi.AddRange(UintToByteArraryNetwork(len));
            len += (UInt32)ProgramInfo.Length;
            clpi.AddRange(UintToByteArraryNetwork(len));
            len += (UInt32)CPI.Length;
            clpi.AddRange(UintToByteArraryNetwork(len));
            for (int i = 0; i < 16; i++)
                clpi.Add(0x00);
            clpi.AddRange(ClipInfo);
            clpi.AddRange(SequenceInfo);
            clpi.AddRange(ProgramInfo);
            clpi.AddRange(CPI);
            clpi.AddRange(UintToByteArraryNetwork(0x00000000));
            return clpi.ToArray();
        }

        byte[] BuildClipInfo(UInt32 numOfSourcePackets, EpElement[] EpInfo)
        {
            List<byte> clip = new List<byte>();
            List<byte> temp = new List<byte>();

            clip.Add(0x00);
            clip.Add(0x00);
            clip.Add(0x01);
            clip.Add(0x01);
            clip.Add(0x00);
            clip.Add(0x00);
            clip.Add(0x00);
            clip.Add(0x00);
            Int64 rate = EpInfo[EpInfo.Length - 1].SPN - EpInfo[0].SPN;
            rate *= 192;
            rate /= ((EpInfo[EpInfo.Length - 1].PTS - EpInfo[0].PTS) / 90000);
            clip.AddRange(UintToByteArraryNetwork((UInt32)rate));
            clip.AddRange(UintToByteArraryNetwork(numOfSourcePackets));
            for (int i = 0; i < 128; i++)
                clip.Add(0x00);
            clip.AddRange(TsTypeInfoBlock);
            UInt32 len = (UInt32)clip.Count;
            temp.AddRange(UintToByteArraryNetwork(len));
            temp.AddRange(clip);
            return temp.ToArray();
        }

        byte[] BuildSequenceInfo(UInt32 start, UInt32 end)
        {
            List<byte> seq = new List<byte>();
            List<byte> temp = new List<byte>();

            seq.Add(0x00);
            seq.Add(0x01);
            seq.Add(0x00);
            seq.Add(0x00);
            seq.Add(0x00);
            seq.Add(0x00);
            seq.Add(0x01);
            seq.Add(0x00);
            seq.Add((byte)((Constants.DEFAULT_PCR_PID >> 8) & 0xff));
            seq.Add((byte)(Constants.DEFAULT_PCR_PID & 0xff));
            seq.Add(0x00);
            seq.Add(0x00);
            seq.Add(0x00);
            seq.Add(0x00);
            seq.AddRange(UintToByteArraryNetwork(start));
            seq.AddRange(UintToByteArraryNetwork(end));
            UInt32 len = (UInt32)seq.Count;
            temp.AddRange(UintToByteArraryNetwork(len));
            temp.AddRange(seq);
            return temp.ToArray();
        }

        byte[] BuildProgramInfo(ushort[] pids, byte[][] StreamCodingInfos)
        {
            List<byte> info = new List<byte>();
            List<byte> temp = new List<byte>();
            info.Add(0x00);
            info.Add(0x01);
            info.Add(0x00);
            info.Add(0x00);
            info.Add(0x00);
            info.Add(0x00);
            info.Add((byte)((Constants.DEFAULT_PMT_PID >> 8) & 0xff));
            info.Add((byte)(Constants.DEFAULT_PMT_PID & 0xff));
            info.Add((byte)pids.Length);
            info.Add(0x00);
            for (int i = 0; i < pids.Length; i++)
            {
                info.Add((byte)((pids[i] >> 8) & 0xff));
                info.Add((byte)(pids[i] & 0xff));
                info.AddRange(StreamCodingInfos[i]);
            }

            UInt32 len = (UInt32)info.Count;
            temp.AddRange(UintToByteArraryNetwork(len));
            temp.AddRange(info);
            return temp.ToArray();
        }

        byte[] BuildVideoStreamCodingInfo(ElementaryStreamTypes type, VideoFormat format, FrameRate rate, AspectRatio ratio)
        {
            List<byte> info = new List<byte>();
            info.Add(0x15);
            info.Add((byte)type);
            info.Add((byte)((((byte)format) << 4) | (byte)rate));
            info.Add((byte)(((byte)(ratio)) << 4));
            for(int i = 0; i < 18; i++)
                info.Add(0x00);
            return info.ToArray();
        }

        byte[] BuildAudioStreamCodingInfo(ElementaryStreamTypes type, AudioPresentationType format, SamplingFrequency rate)
        {
            List<byte> info = new List<byte>();
            info.Add(0x15);
            info.Add((byte)type);
            info.Add((byte)((((byte)format) << 4) | (byte)rate));
            info.Add(0x65);
            info.Add(0x6e);
            info.Add(0x67);
            for (int i = 0; i < 16; i++)
                info.Add(0x00);
            return info.ToArray();
        }

        byte[] BuildPgStreamCodingInfo()
        {
            List<byte> info = new List<byte>();
            info.Add(0x15);
            info.Add(0x90);
            info.Add(0x65);
            info.Add(0x6e);
            info.Add(0x67);
            for (int i = 0; i < 17; i++)
                info.Add(0x00);
            return info.ToArray();
        }

        byte[] BuildCpi(byte[] EpMap)
        {
            List<byte> info = new List<byte>();
            UInt32 len = (UInt32)EpMap.Length + 2;
            info.AddRange(UintToByteArraryNetwork(len));
            info.Add(0x00);
            info.Add(0x01);
            info.AddRange(EpMap);
            return info.ToArray();
        }

        byte[] BuildEpMap(EpElement[] EpInfo)
        {
            UInt32 lastepfine = 0x7ff;
            UInt32 lastspnfine = 0x1ffff;
            UInt32 numofcoarse = 0;
            List<byte> coarseloop = new List<byte>();
            List<byte> fineloop = new List<byte>(EpInfo.Length);
            List<byte> EpMap = new List<byte>();

            for (int i = 0; i < EpInfo.Length; i++)
            {
                UInt32 epfine = (UInt32)((EpInfo[i].PTS >> 9) % 0x800);
                UInt32 epcoarse = (UInt32)((EpInfo[i].PTS >> 19) % 0x4000);
                UInt32 spnfine = EpInfo[i].SPN % 0x20000;
                if (lastepfine > epfine || lastspnfine > spnfine)
                {
                    UInt32 reftofine = (UInt32)i;
                    reftofine <<= 14;
                    reftofine |= epcoarse;
                    coarseloop.AddRange(UintToByteArraryNetwork(reftofine));
                    coarseloop.AddRange(UintToByteArraryNetwork(EpInfo[i].SPN));
                    numofcoarse++;
                }
                UInt32 value = 0x1000;
                value |= (epfine << 17);
                value |= spnfine;
                fineloop.AddRange(UintToByteArraryNetwork(value));
                lastepfine = epfine;
                lastspnfine = spnfine;
            }

            EpMap.Add(0x00);
            EpMap.Add(0x01);
            EpMap.Add((byte)((Constants.DEFAULT_VIDEO_PID >> 8) & 0xff));
            EpMap.Add((byte)(Constants.DEFAULT_VIDEO_PID & 0xff));
            EpMap.Add(0x00);
            byte btemp = 4;
            btemp |= (byte)((numofcoarse >> 14) & 0xff);
            EpMap.Add(btemp);
            btemp = (byte)((numofcoarse >> 6) & 0xff);
            EpMap.Add(btemp);
            btemp = (byte)((numofcoarse & 0x3f) << 2);
            btemp |= (byte)((EpInfo.Length >> 16) & 0x03);
            EpMap.Add(btemp);
            btemp = (byte)((EpInfo.Length >> 8) & 0xff);
            EpMap.Add(btemp);
            btemp = (byte)(EpInfo.Length& 0xff);
            EpMap.Add(btemp);
            UInt32 count = 4 + (UInt32)EpMap.Count;
            EpMap.AddRange(UintToByteArraryNetwork(count));
            UInt32 start = 4 + (UInt32)coarseloop.Count;
            EpMap.AddRange(UintToByteArraryNetwork(start));
            EpMap.AddRange(coarseloop);
            EpMap.AddRange(fineloop);
            return EpMap.ToArray();
        }
    }

    public class Descriptor
    {
        protected byte[] mdata;
        public Descriptor(byte[] data, int startIndex)
        {
            if (data.Length < 2)
                throw new ArgumentException("Invalid descriptor");
            if (startIndex + 2 + data[startIndex + 1] > data.Length)
                throw new ArgumentException("Invalid descriptor");
            mdata = new byte[2 + data[startIndex + 1]];
            for (int i = 0; i < mdata.Length; i++)
                mdata[i] = data[i + startIndex];
        }
        public byte Tag
        {
            get { return mdata[0]; }
        }
        public byte Length
        {
            get { return mdata[1]; }
        }
        public byte[] Data
        {
            get { return mdata; }
        }
    }

    public class DTCP_Descriptor : Descriptor
    {
        public DTCP_Descriptor(byte[] data, int startIndex)
            : base(data, startIndex)
        {
            if (data.Length < 6)
                throw new ArgumentException("Invalid DTCP descriptor");
            if (Tag != Constants.DTCP_DESCRIPTOR_TAG)
                throw new ArgumentException("Invalid DTCP descriptor tag");
            if (Length < 4)
                throw new ArgumentException("Invalid DTCP descriptor length");
            if (data[startIndex + 2] != 0x0f || data[startIndex + 3] != 0xff)
                throw new ArgumentException("Invalid DTCP descriptor CA system ID");
        }

        public DtcpCci CopyStatus
        {
            get
            {
                return (DtcpCci)(mdata[4] & 0x3);
            }
        }

        public bool AnalogConstrain
        {
            get
            {
                return ((mdata[5] & 0x4) == 0);
            }
        }

        public bool Macrovision
        {
            get
            {
                return ((mdata[5] & 0x3) > 0);
            }
        }
    }
    class Constants
    {
        public const int TS_PAYLOAD_SIZE = 184;
        public const int TS_SIZE = 188;
        public const int STRIDE_SIZE = 192;

        public const int DISK_BUFFER = 0x8D000 << 5;  

        public const byte SYNC_BYTE = 0x47;
        public const byte PAT_PID = 0x00;
        public const byte SIT_PID = 0x1f;
        public const byte PAT_TABLE_ID = 0x00;
        public const byte PMT_TABLE_ID = 0x02;
        public const byte DTCP_DESCRIPTOR_TAG = 0x88;
        public const byte END_ID = 0xb9;
        public const byte PACK_ID = 0xba;
        public const byte SYS_ID = 0xbb;
        public const byte MAP_ID = 0xbc;
        public const byte DIR_ID = 0xff;
        public const byte PAD_ID = 0xbe;

        // defaults
        public const ushort DEFAULT_PMT_PID = 0x0100;
        public const ushort DEFAULT_VIDEO_PID = 0x1011;
        public const ushort MAX_VIDEO_PID = 0x1019;
        public const ushort DEFAULT_AUDIO_PID = 0x1100;
        public const ushort MAX_AUDIO_PID = 0x111f;
        public const ushort DEFAULT_PCR_PID = 0x1001;
        public const ushort DEFAULT_SUBTITLE_PID = 0x1800;
        public const ushort DEFAULT_PRESENTATION_GRAPHICS_PID = 0x1200;
        public const ushort DEFAULT_INTERACTIVE_GRAPHICS_PID = 0x1400;
        public const ushort DEFAULT_PROGRAM_NUMBER = 0x01;
        public const int MAX_BUFFER_COUNT = 0xff;
        public const int MIN_BUFFER_COUNT = 0x02;
        public const Int64 AUDIO_DELAY = 30000;
        public const UInt32 MKVCLUSTER_START = 0x1f43b675;
        public const UInt32 MKVFILE_START = 0x1a45dfa3;
        public const UInt32 MKVSEGMENT_START = 0x18538067;
        public const UInt32 MKVTRACKINFO_START = 0x1654AE6B;
        public const byte MKVTIMECODE_START = 0xe7;

        // stream types
        public const byte PES_VIDEO = 0xe0;
        public const byte PES_AUDIO_MPEG = 0xc0;
        public const byte PES_PRIVATE1 = 0xbd;
        public const byte PES_PADDING = 0xbe;
        public const byte PES_PRIVATE2 = 0xbf;
        public const byte PES_VIDEO_VC1 = 0xfd;
        public const byte PES_PRIVATE_SUBTITLE = 0x20;
        public const byte PES_PRIVATE_AC3 = 0x80;
        public const byte PES_PRIVATE_AC3_PLUS = 0xc0;
        public const byte PES_PRIVATE_DTS_HD = 0x88;
        public const byte PES_PRIVATE_LPCM = 0xa0;
        public const byte PES_PRIVATE_AC3_TRUE_HD = 0xb0;

        public const UInt32 VC1_SEQ_SC = 0x0000010f;
        public const UInt32 VC1_END_OF_STREAM = 0x0000010a;
        public const ushort AC3_SYNC = 0x0b77;
        public const UInt32 H264_PREFIX = 0x00000107;
        public const UInt32 H264_END_OF_STREAM = 0x0000010b;
        public const UInt32 DTS_SYNC = 0x7ffe8001;
        public const UInt32 DTS_EXT_SYNC = 0x64582025;
        public const UInt32 MLP_SYNC = 0xF8726FBA;
        public const UInt32 MPEG2_SEQ_CODE = 0x000001b3;
        public const UInt32 MPEG2_SEQ_EXT = 0x000001b5;
        public const UInt32 MPEG2_SEQ_END = 0x000001b7;

        // clocks
        public const Int64 MPEG2TS_CLOCK_RATE = 27000000;
        public const Int64 MAX_MPEG2TS_CLOCK = 0x25800000000;
        public const Int64 MAX_BLURAY_CLOCK = 0x40000000;
        public const Int64 MAX_FIREWIRE_CLOCK = 24576000;
        public const Int64 MAX_PTS_CLOCK = 0x200000000;
        public const Int64 PTS_CLOCK_RATE = 90000;
        public const int MAX_OFFSET = 3072;
        public const int MAX_COUNT = 8000;

        // descriptors
        public static readonly byte[] hdmv_registration_descriptor = new byte[6] {
            0x05, 0x04, 0x48, 0x44, 0x4d, 0x56 };
        public static readonly byte[] copy_control_descriptor = new byte[6] {
            0x88, 0x04, 0x0f, 0xff, 0x84, 0xfc };
        public static readonly byte[] vc1_descriptor = new byte[7] {
            0x05, 0x05, 0x56, 0x43, 0x2d, 0x31, 0xff };
        public static readonly byte[] ac3_registration_descriptor = new byte[6] {
            0x05, 0x04, 0x41, 0x43, 0x2d, 0x33 };

        public static readonly byte[] DefaultSitTableOne = new byte[Constants.TS_SIZE] {
            0x47, 0x40, 0x1f, 0x10, 0x00, 0x7f, 0xf0, 0x19, 0xff, 0xff, 0xc1, 0x00, 0x00, 0xf0, 0x0a, 0x63,
            0x08, 0xc1, 0x5a, 0xae, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x01, 0x80, 0x00, 0x34, 0x1e, 0xe7,
            0x4e, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        };

        private static readonly uint[] crc_table = new uint[256] {
            0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b,
            0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
            0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 0x4c11db70, 0x48d0c6c7,
            0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
            0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3,
            0x709f7b7a, 0x745e66cd, 0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
            0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef,
            0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
            0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb,
            0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
            0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 0x34867077, 0x30476dc0,
            0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
            0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4,
            0x0808d07d, 0x0cc9cdca, 0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
            0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08,
            0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
            0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc,
            0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
            0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 0xe0b41de7, 0xe4750050,
            0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
            0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34,
            0xdc3abded, 0xd8fba05a, 0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
            0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1,
            0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
            0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5,
            0x3f9b762c, 0x3b5a6b9b, 0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
            0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 0xf12f560e, 0xf5ee4bb9,
            0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
            0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd,
            0xcda1f604, 0xc960ebb3, 0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
            0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71,
            0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
            0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2,
            0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
            0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 0x119b4be9, 0x155a565e,
            0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
            0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a,
            0x2d15ebe3, 0x29d4f654, 0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
            0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676,
            0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
            0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662,
            0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
            0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4
        };
        public static uint ComputeCrc(byte[] data)
        {
            return ComputeCrc(data, data.Length);
        }
        public static uint ComputeCrc(byte[] data, int length)
        {
            return ComputeCrc(data, length, 0);
        }
        public static uint ComputeCrc(byte[] data, int length, int startIndex)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < length; i++)
                crc = (crc << 8) ^ crc_table[((crc >> 24) ^ data[i + startIndex]) & 0xff];
            return crc;
        }
    }

    class ProgramInfo
    {
        private byte[] mData;

        public ProgramInfo(byte[] data, int index)
        {
            if (null == data)
                throw new ArgumentException("program data is null");
            if (data.Length + index < 4)
                throw new ArgumentException("program data too short");
            mData = new byte[4];
            for (int i = 0; i < mData.Length; i++)
            {
                mData[i] = data[i + index];
            }
        }

        public ProgramInfo(ushort programNumber, ushort programPid)
        {
            mData = new byte[4];
            ProgramNumber = programNumber;
            ProgramPID = programPid;
        }

        public ushort ProgramNumber
        {
            get
            {
                return (ushort)((mData[0] << 8) + mData[1]);
            }
            set
            {
                mData[0] = (byte)((value >> 8) & 0xff);
                mData[1] = (byte)(value & 0xff);
            }
        }

        public ushort ProgramPID
        {
            get
            {
                return (ushort)(((mData[2] & 0x1f) << 8) + mData[3]);
            }
            set
            {
                mData[2] = (byte)(((value >> 8) & 0x1f) | 0xe0);
                mData[3] = (byte)(value & 0xff);
            }
        }

        public byte[] Data
        {
            get
            {
                return mData;
            }
        }
    }

    class TsPacket
    {
        protected byte[] mData;

        public TsPacket()
        {
            // initialize the packet as a null packet
            mData = new byte[Constants.TS_SIZE];
            mData[0] = Constants.SYNC_BYTE; // sync byte
            mData[1] = 0x1f; // PID = 0x1FFF
            mData[2] = 0xff; // PID == 0x1FFF
            mData[3] = 0x10; // no adaptation field
            for (int i = 4; i < Constants.TS_SIZE; i++)
                mData[i] = 0xff;
        }

        public bool Priority
        {
            get
            {
                if ((mData[1] & 0x20) > 0)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    mData[1] |= 0x20;
                else
                    mData[1] &= 0xdf;
            }
        }

        public ushort PID
        {
            get
            {
                return (ushort)(((mData[1] & 0x1f) << 8) + mData[2]);
            }
            set
            {
                byte b = (byte)(mData[1] & 0xE0);
                b += (byte)((value >> 8) & 0x1f);
                mData[1] = b;
                mData[2] = (byte)(value & 0xff);
            }
        }
        public byte PointerSize
        {
            get
            {
                if ((mData[3] & 0x20) == 0) // No adaptation field present
                    return mData[4];
                return (byte)(mData[4] + 1 + mData[mData[4] + 5]);
            }
        }
        public byte[] GetData()
        {
            return mData;
        }
        public void SetData(byte[] data, int startIndex)
        {
            if (null == data)
                throw new ArgumentException("null packet");
            else if (Constants.TS_SIZE > data.Length - startIndex)
                throw new ArgumentException("small packet");
            else if (Constants.SYNC_BYTE != data[0 + startIndex])
                throw new ArgumentException("sync byte missing");
            for (int i = 0; i < Constants.TS_SIZE; i++)
                mData[i] = data[i + startIndex];
        }
        public bool HasPcr
        {
            get
            {
                if ((mData[3] & 0x20) > 0 // Adaptation field present
                    && (mData[4] > 0) // length > 0
                    && (mData[5] & 0x10) > 0) // and a PCR
                    return true;
                return false;
            }
        }
        public Int64 Pcr
        {
            get
            {
                if (false == HasPcr)
                    throw new FormatException("PCR not present in this packet");
                Int64 mpeg2tsClock = mData[6];
                mpeg2tsClock <<= 25;
                mpeg2tsClock += (mData[7] << 17);
                mpeg2tsClock += (mData[8] << 9);
                mpeg2tsClock += (mData[9] << 1);
                mpeg2tsClock += ((mData[10] & 0x80) >> 7);
                mpeg2tsClock *= 300;
                mpeg2tsClock += ((mData[10] & 0x1) << 8);
                mpeg2tsClock += (mData[11]);
                return mpeg2tsClock;
            }
        }

        public bool HasPesHeader
        {
            get
            {
                if ((mData[1] & 0x40) == 0)
                    return false;
                int offset = 4;
                if ((mData[3] & 0x20) > 0)
                {
                    // adaptation field present
                    offset += (1 + mData[4]);
                    if (offset >= Constants.TS_SIZE)
                        return false;
                }
                if ((mData[3] & 0x10) > 0)
                {
                    // payload present
                    int len = Constants.TS_SIZE - offset;
                    if (len < 10)
                        return false;
                    if (mData[offset] == 0 && mData[offset + 1] == 0 && mData[offset + 2] == 1
                        && len >= 9 + mData[offset + 8])
                        return true;
                    else if(mData[offset] == 0 && mData[offset + 1] == 0 && mData[offset + 2] == 1)
                        return false;
                }
                return false;
            }
        }
        public byte[] Payload
        {
            get
            {
                int offset = 4;
                if ((mData[3] & 0x20) > 0)
                {
                    // adaptation field present
                    offset += (1 + mData[4]);
                    if (offset >= Constants.TS_SIZE)
                        return null;
                }
                if ((mData[3] & 0x10) > 0)
                {
                    // payload present
                    byte[] ret = new byte[Constants.TS_SIZE - offset];
                    for (int i = 0; i < ret.Length; i++)
                        ret[i] = mData[i + offset];
                    return ret;
                }
                return null;
            }
        }
        public void IncrementContinuityCounter()
        {
            if (0xf == (mData[3] & 0xf))
                mData[3] &= 0xf0;
            else
                mData[3]++;
        }
        public byte ContinuityCounter
        {
            get
            {
                return (byte)(mData[3] & 0xf);
            }
            set
            {
                if (value > 0x0f)
                    throw new ArgumentException("Invalid continuity counter");
                mData[3] &= 0xf0;
                mData[3] |= value;
            }
        }
    }

    class PcrPacket : TsPacket
    {
        public PcrPacket(Int64 pcr, byte counter, ushort pid)
            : base()
        {
            this.PID = pid;
            this.ContinuityCounter = counter;
            mData[3] &= 0x0f; // adaptation field only, no payload
            mData[3] |= 0x20; // adaptation field only, no payload
            mData[4] = 183; // length
            mData[5] = 0x10; // only PCR present
            Int64 tsClockValue = pcr / 300;
            Int64 tsOffsetValue = pcr % 300;
            mData[6] = (byte)((tsClockValue & 0x1fe000000) >> 25);
            mData[7] = (byte)((tsClockValue & 0x1fe0000) >> 17);
            mData[8] = (byte)((tsClockValue & 0x1fe00) >> 9);
            mData[9] = (byte)((tsClockValue & 0x1fe) >> 1);
            if ((tsClockValue & 0x1) == 0)
                mData[10] &= 0x7f;
            if ((tsOffsetValue & 0x100) == 0)
                mData[10] &= 0xfe;
            mData[11] = (byte)(tsOffsetValue & 0xff);
            for (int i = 12; i < Constants.TS_SIZE; i++)
                mData[i] = 0xff;
        }
    }

    class TsTable : TsPacket
    {
        public TsTable()
            : base()
        {
            // error = 0, payload = 1, priority = 0
            mData[1] = 0x40;
            // scrambling = 0, adaptation = 01, continuity = 0
            mData[3] = 0x10;
            // pointer = 00
            mData[4] = 0x0;
            // reserved, version, current/next
            mData[10] = 0xc1;
            // section
            mData[11] = 0x0;
            // last section
            mData[12] = 0x0;
        }

        public TsTable(byte[] data)
            : base()
        {
            SetData(data, 0);
        }

        public void AddData(byte[] data, int offset, int len)
        {
            List<byte> newData = new List<byte>();
            newData.AddRange(mData);
            for (int i = offset; i < len; i++)
                newData.Add(data[i]);
            mData = newData.ToArray();
        }

        public bool Complete
        {
            get
            {
                int currentLen = mData.Length - (PointerSize + 8);
                if (Length > currentLen)
                    return false;
                return true;
            }
        }

        public byte TableId
        {
            get
            {
                return mData[5 + PointerSize];
            }
            set
            {
                mData[5 + PointerSize] = value;
            }
        }

        protected ushort NumberId
        {
            get
            {
                return (ushort)((mData[8 + PointerSize] << 8) + mData[9 + PointerSize]);
            }
            set
            {
                mData[8 + PointerSize] = (byte)((value >> 8) & 0xff);
                mData[9 + PointerSize] = (byte)(value & 0xff);
            }
        }

        protected ushort Length
        {
            get
            {
                return (ushort)(((mData[6 + PointerSize] & 0x0f) << 8) + mData[7 + PointerSize]);
            }
            set
            {
                // syntax, reserved, length
                mData[6 + PointerSize] = (byte)(0xb0 | (byte)((value >> 8) & 0x0f));
                mData[7 + PointerSize] = (byte)(value & 0xff);
            }
        }

        protected void RefreshCrc()
        {
            uint crc = Constants.ComputeCrc(mData, Length - 1, 5 + PointerSize);
            mData[Length + 4 + PointerSize] = (byte)((crc >> 24) & 0xff);
            mData[Length + 5 + PointerSize] = (byte)((crc >> 16) & 0xff);
            mData[Length + 6 + PointerSize] = (byte)((crc >> 8) & 0xff);
            mData[Length + 7 + PointerSize] = (byte)(crc & 0xff);
            for (int i = Length + 8 + PointerSize; i < Constants.TS_SIZE; i++)
                mData[i] = 0xff;
        }
    }

    class PatPacket : TsTable
    {
        public PatPacket()
            : base()
        {
            PID = Constants.PAT_PID;
            TableId = Constants.PAT_TABLE_ID;
            Length = 9;
            TransportStreamId = 1;
            RefreshCrc();
        }

        public PatPacket(byte[] data)
            : base(data)
        {
            if (TableId != Constants.PAT_TABLE_ID)
                throw new ArgumentException("packet does not contain a valid PAT table ID");
            if (0 != PID)
                throw new ArgumentException("packet does not contain a valid PAT PID");
        }

        public ushort TransportStreamId
        {
            get
            {
                return NumberId;
            }
            set
            {
                NumberId = value;
                RefreshCrc();
            }
        }

        public ProgramInfo[] Programs
        {
            get
            {
                if (ProgramInfoLength == 0)
                    return null;
                ProgramInfo[] programs = new ProgramInfo[ProgramInfoLength / 4];
                for (int i = 0; i < ProgramInfoLength; i += 4)
                    programs[i / 4] = new ProgramInfo(mData, 13 + PointerSize + i);
                return programs;
            }
            set
            {
                if (null == value || value.Length == 0)
                {
                    if (ProgramInfoLength == 0)
                        return;
                    Length = 9;
                    RefreshCrc();
                }
                else
                {
                    if ((value.Length * 4) + 17 + PointerSize > Constants.TS_SIZE)
                        throw new ArgumentException("program info data too long");
                    Length = (ushort)(9 + (value.Length * 4));
                    int index = 13 + PointerSize;
                    foreach (ProgramInfo pi in value)
                    {
                        for (int i = 0; i < 4; i++)
                            mData[index + i] = pi.Data[i];
                        index += 4;
                    }
                    RefreshCrc();
                }
            }
        }

        private ushort ProgramInfoLength
        {
            get
            {
                return (ushort)(Length - 9);
            }
        }
    }

    class SitPacket : TsTable
    {
        public SitPacket()
            : base(Constants.DefaultSitTableOne)
        {
        }

        public SitPacket(byte[] data)
            : base(data)
        {
        }
    }

    class PmtPacket : TsTable
    {
        public PmtPacket()
            : base()
        {
            // table id = 02
            TableId = Constants.PMT_TABLE_ID;
            Length = 13;
            // program id = 1
            ProgramNumber = 1;
            // reserved, PcrPID
            PcrPID = Constants.DEFAULT_PCR_PID;
            // reserved, program info length
            ProgramDescriptorsLength = 0;
            PID = Constants.DEFAULT_PMT_PID;
            RefreshCrc();
        }

        public PmtPacket(byte[] data)
            : base(data)
        {
            if (TableId != Constants.PMT_TABLE_ID)
                throw new ArgumentException("packet does not contain a valid PMT table ID");
        }

        public DTCP_Descriptor DtcpInfo
        {
            get
            {
                byte[] descriptors = ProgramDescriptorsData;
                if (null == descriptors)
                    return null;
                DTCP_Descriptor dt = null;

                for (int i = 0; i < descriptors.Length; )
                {
                    try
                    {
                        dt = new DTCP_Descriptor(descriptors, i);
                        break;
                    }
                    catch (ArgumentException)
                    {
                        i += (2 + descriptors[i + 1]);
                    }
                }

                return dt;
            }
        }

        public byte[] ProgramDescriptorsData
        {
            get
            {
                if (ProgramDescriptorsLength == 0)
                    return null;
                byte[] descriptors = new byte[ProgramDescriptorsLength];
                for (int i = 0; i < descriptors.Length; i++)
                {
                    descriptors[i] = mData[i + 17 + PointerSize];
                }
                return descriptors;
            }
            set
            {
                if (null == value || 0 == value.Length)
                {
                    if (ProgramDescriptorsLength > 0)
                    {
                        // need to remove existing descriptors
                        byte[] data = new byte[StreamInfoLength];
                        int index = 17 + ProgramDescriptorsLength + PointerSize;
                        // copy data between descriptor and crc
                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = mData[index + i];
                            mData[17 + i + PointerSize] = data[i];
                        }
                        Length -= ProgramDescriptorsLength;
                        ProgramDescriptorsLength = 0;
                        RefreshCrc();
                    }
                    else
                    {
                        // nothing to do
                        return;
                    }
                }
                else
                {
                    if (value.Length + Length + PointerSize + 5 - ProgramDescriptorsLength > Constants.TS_SIZE)
                        throw new ArgumentException("program descriptors data too long");
                    // need to remove existing descriptors
                    byte[] data = new byte[StreamInfoLength];
                    int index = 17 + ProgramDescriptorsLength + PointerSize;
                    // copy data between descriptor and crc
                    for (int i = 0; i < data.Length; i++)
                        data[i] = mData[index + i];
                    Length -= ProgramDescriptorsLength;
                    Length += (ushort)value.Length;
                    ProgramDescriptorsLength = (ushort)value.Length;
                    // copy the new descriptor
                    for (int i = 0; i < value.Length; i++)
                        mData[17 + i + PointerSize] = value[i];
                    // recover data between descriptor and crc
                    for (int i = 0; i < data.Length; i++)
                        mData[17 + value.Length + i + PointerSize] = data[i];
                    RefreshCrc();
                }
            }
        }

        public StreamInfo[] ElementaryStreams
        {
            get
            {
                if (0 == StreamInfoLength)
                    return null;
                List<StreamInfo> streams = new List<StreamInfo>();
                for (int i = 0; i < StreamInfoLength; )
                {
                    StreamInfo si = new StreamInfo(mData, 17 + PointerSize + ProgramDescriptorsLength + i);
                    streams.Add(si);
                    i += si.Data.Length;
                }
                return streams.ToArray();
            }
            set
            {
                Length -= StreamInfoLength;
                if (null == value || value.Length == 0)
                {
                    if (0 == StreamInfoLength)
                        return; // do nothing
                }
                else
                {
                    int index = ProgramDescriptorsLength + 17 + PointerSize;
                    foreach (StreamInfo si in value)
                    {
                        if (index + si.Data.Length > Constants.TS_SIZE)
                            throw new ArgumentException("elementary stream descriptors data too long");
                        for (int i = 0; i < si.Data.Length; i++)
                            mData[index + i] = si.Data[i];
                        index += si.Data.Length;
                    }
                    Length += (ushort)(index - 17 - ProgramDescriptorsLength - PointerSize);
                }
                RefreshCrc();
            }
        }

        public ushort ProgramNumber
        {
            get
            {
                return NumberId;
            }
            set
            {
                NumberId = value;
                RefreshCrc();
            }
        }

        public ushort PcrPID
        {
            get
            {
                return (ushort)(((mData[13 + PointerSize] & 0x1f) << 8) + mData[14 + PointerSize]);
            }
            set
            {
                mData[13 + PointerSize] = (byte)(((value >> 8) & 0x1f) | 0xe0);
                mData[14 + PointerSize] = (byte)(value & 0xff);
                RefreshCrc();
            }
        }

        private ushort ProgramDescriptorsLength
        {
            get
            {
                return (ushort)(((mData[15 + PointerSize] & 0x0f) << 8) + mData[16 + PointerSize]);
            }
            set
            {
                mData[15 + PointerSize] = (byte)(0xf0 | (byte)((value >> 8) & 0x0f));
                mData[16 + PointerSize] = (byte)(value & 0xff);
            }
        }

        private ushort StreamInfoLength
        {
            get
            {
                return (ushort)(Length - 13 - ProgramDescriptorsLength);
            }
        }
    }

    public class PesPacket
    {
        List<byte> data;
        ushort pid;
        bool priority;

        public PesPacket(byte[] buff, int offset, int length, ushort pid)
        {
            this.data = new List<byte>(length);
            this.pid = pid;
            this.AddData(buff, offset, length);
            this.priority = false;
        }

        public bool Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public byte[] GetData()
        {
            return data.ToArray();
        }

        public byte[] GetPayload()
        {
            PesHeader ph = GetHeader();
            if (ph == null)
                return GetData();
            return data.GetRange(9 + ph.HeaderLength, data.Count - (9 + ph.HeaderLength)).ToArray();
        }

        public byte this[int i]
        {
            get { return data[i]; }
            set { data[i] = value; }
        }

        public ushort PID
        {
            get { return pid; }
            set { pid = value; }
        }

        public bool Complete
        {
            get
            {
                if (data.Count < 6)
                    return false;
                ushort len = (ushort)((data[4] << 8) + data[5]);
                if (len == 0)
                    return false;
                if (data.Count != len + 6)
                    return false;
                return true;
            }
            set
            {
                if (value)
                {

                    ushort len = (ushort)(data.Count - 6);
                    if (data.Count > (0xffff - 6))
                        len = 0;
                    data[4] = (byte)((len >> 8) & 0xff);
                    data[5] = (byte)(len & 0xff);
                }
            }
        }

        public PesHeader GetHeader()
        {
            try
            {
                PesHeader ph = new PesHeader(data.ToArray());
                return ph;
            }
            catch (FormatException)
            {
                // no valid header (yet)
                return null;
            }
        }

        public void AddData(List<byte> moredata)
        {
            data.AddRange(moredata);
        }


        public void AddData(byte[] buff, int offset, int length)
        {
            for (int i = offset; i < length + offset; i++)
                data.Add(buff[i]);
        }

        public byte BaseId
        {
            get
            {
                if (data.Count > 3)
                    return data[3];
                return 0;
            }
        }

        public byte ExtendedId
        {
            get
            {
                if ((data.Count > 8) && data.Count > (8 + data[8]))
                    return data[9 + data[8]];
                return 0;
            }
        }

        public UInt32 ExtendedType
        {
            get
            {
                if ((data.Count > 8) && data.Count > (11 + data[8]))
                {
                    UInt32 format = (UInt32)data[9 + data[8]] << 24;
                    format += (UInt32)data[10 + data[8]] << 16;
                    format += (UInt32)data[11 + data[8]] << 8;
                    format += (UInt32)data[12 + data[8]];
                    return format;
                }
                return 0;
            }
        }
    }

    public class PesHeader
    {
        private byte[] mData;

        public PesHeader(byte[] data)
        {
            if (data.Length < 9)
                throw new FormatException("Invalid PES header length");
            if (data[0] != 0x00 || data[1] != 0x00 || data[2] != 0x01)
                throw new FormatException("Invalid PES prefix");
            int hlen = 9 + data[8];
            int plen = 6 + (data[4] << 8) + data[5];
            if (plen != 6 && hlen > plen)
                throw new FormatException("Invalid PES header/packet length");
            if (data.Length < hlen)
                throw new FormatException("PES Header too short");
            mData = new byte[hlen];
            for (int i = 0; i < hlen; i++)
                mData[i] = data[i];
        }

        public byte StreamId
        {
            get { return mData[3]; }
        }

        public byte this[int i]
        {
            get { return mData[i]; }
            set { mData[i] = value; }
        }

        public byte HeaderLength
        {
            get { return mData[8]; }
        }

        public int TotalHeaderLength
        {
            get { return 9 + HeaderLength; }
        }

        public ushort PacketLength
        {
            get { return (ushort)((mData[4] << 8) + mData[5]); }
        }

        public bool HasPts
        {
            get
            {
                if(mData.Length > 13)
                    return (mData[7] & 0x80) > 0;
                return false;
            }
        }

        public bool HasDts
        {
            get
            {
                if(mData.Length > 18 )
                    return (mData[7] & 0x40) > 0;
                return false;
            }
        }

        public Int64 Pts
        {
            get
            {
                if (HasPts == false)
                    throw new ArgumentException("No Pts available");
                Int64 ret = 0;
                ret += ((Int64)(mData[9] & 0x0e)) << 29;
                ret += ((Int64)(mData[10])) << 22;
                ret += ((Int64)(mData[11] & 0xfe)) << 14;
                ret += ((Int64)(mData[12])) << 7;
                ret += ((Int64)(mData[13] & 0xfe)) >> 1;
                return ret;
            }
            set
            {
                if (HasPts == false)
                    throw new ArgumentException("No Pts available");
                byte old = (byte)(mData[9] & 0xf1);
                mData[9] = (byte)(((value & 0x1C0000000) >> 29) | old);
                mData[10] = (byte)((value & 0x3fC00000) >> 22);
                mData[11] = (byte)(((value & 0x3f8000) >> 14) | 0x01);
                mData[12] = (byte)((value & 0x7f80) >> 7);
                mData[13] = (byte)(((value & 0x7f) << 1) | 0x01);
            }
        }

        public Int64 Dts
        {
            get
            {
                if (HasDts == false)
                    throw new ArgumentException("No Dts available");
                Int64 ret = 0;
                ret += ((Int64)(mData[14] & 0x0e)) << 29;
                ret += ((Int64)(mData[15])) << 22;
                ret += ((Int64)(mData[16] & 0xfe)) << 14;
                ret += ((Int64)(mData[17])) << 7;
                ret += ((Int64)(mData[18] & 0xfe)) >> 1;
                return ret;
            }
            set
            {
                if (HasDts == false)
                    throw new ArgumentException("No Dts available");
                byte old = (byte)(mData[14] & 0xf1);
                mData[14] = (byte)(((value & 0x1C0000000) >> 29) | old);
                mData[15] = (byte)((value & 0x3fC00000) >> 22);
                mData[16] = (byte)(((value & 0x3f8000) >> 14) | 0x01);
                mData[17] = (byte)((value & 0x7f80) >> 7);
                mData[18] = (byte)(((value & 0x7f) << 1) | 0x01);
            }
        }

        public byte Extention2
        {
            get
            {
                int offset = 6;
                if ((mData[offset] & 0xc0) != 0x80)
                    return 0; // first two bits must be '10'
                byte PTS_DTS_flags = (byte)(mData[offset + 1] & 0xc0);
                byte ESCR_flag = (byte)(mData[offset + 1] & 0x20);
                byte ES_rate_flag = (byte)(mData[offset + 1] & 0x10);
                byte DSM_trick_mode_flag = (byte)(mData[offset + 1] & 0x08);
                byte additional_copy_info_flag = (byte)(mData[offset + 1] & 0x04);
                byte PES_CRC_flag = (byte)(mData[offset + 1] & 0x02);
                byte PES_extension_flag = (byte)(mData[offset + 1] & 0x01);
                if (mData[offset + 2] == 0)
                    return 0;
                int length = offset + mData[offset + 2] + 3;
                if (mData.Length < length)
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
                byte PES_private_data_flag = (byte)(mData[offset] & 0x80);
                byte pack_header_field_flag = (byte)(mData[offset] & 0x40);
                byte program_packet_sequence_counter_flag = (byte)(mData[offset] & 0x20);
                byte PSTD_mDataer_flag = (byte)(mData[offset] & 0x10);
                byte PES_extension_flag_2 = (byte)(mData[offset] & 0x01);
                offset++;
                if (PES_private_data_flag > 0)
                    offset += 25;
                if (pack_header_field_flag > 0)
                    offset += (mData[offset] + 1);
                if (program_packet_sequence_counter_flag > 0)
                    offset += 2;
                if (PSTD_mDataer_flag > 0)
                    offset += 2;
                if (PES_extension_flag_2 == 0)
                    return 0;
                if (mData[offset] != 0x81)
                    return 0;
                return mData[offset + 1];
            }
        }

        public byte[] Data
        {
            get { return mData; }
        }
    }

    public class StreamInfo
    {
        private byte[] mData;
        private VideoFormat mVideoFormat;
        private AspectRatio mAspectRatio;
        private FrameRate mFrameRate;
        private AudioPresentationType mAudioPresentationType;
        private SamplingFrequency mSamplingFrequency;

        public StreamInfo(byte[] data, int index)
        {
            if (null == data)
                throw new ArgumentException("stream data is null");
            if (data.Length + index < 5)
                throw new ArgumentException("stream data too short");
            uint descLength = (uint)((data[3 + index] & 0x0f) << 8) + data[4 + index];
            if (descLength > Constants.TS_SIZE)
                throw new ArgumentException("descriptors data too long");
            if (5 + descLength > data.Length - index)
                throw new ArgumentException("stream data too short");
            mData = new byte[5 + descLength];
            for (int i = 0; i < mData.Length; i++)
            {
                mData[i] = data[i + index];
            }
            mVideoFormat = VideoFormat.Reserved;
            mAspectRatio = AspectRatio.Reserved;
            mFrameRate = FrameRate.Reserved;
            mAudioPresentationType = AudioPresentationType.Reserved;
            mSamplingFrequency = SamplingFrequency.Reserved;
        }

        public VideoFormat VideoFormat
        {
            get { return mVideoFormat; }
            set { mVideoFormat = value; }
        }

        public AspectRatio AspectRatio
        {
            get { return mAspectRatio; }
            set { mAspectRatio = value; }
        }

        public FrameRate FrameRate
        {
            get { return mFrameRate; }
            set { mFrameRate = value; }
        }

        public AudioPresentationType AudioPresentationType
        {
            get { return mAudioPresentationType; }
            set { mAudioPresentationType = value; }
        }

        public SamplingFrequency SamplingFrequency
        {
            get { return mSamplingFrequency; }
            set { mSamplingFrequency = value; }
        }

        public StreamInfo(ElementaryStreamTypes streamType, ushort elementaryPid)
        {
            mData = new byte[5];
            StreamType = streamType;
            ElementaryPID = elementaryPid;
            // reserved and descriptors length
            mData[3] = 0xf0;
            mData[4] = 0x00;
        }

        public byte[] Data
        {
            get
            {
                return mData;
            }
        }

        public ElementaryStreamTypes StreamType
        {
            get
            {
                return (ElementaryStreamTypes)mData[0];
            }
            set
            {
                mData[0] = (byte)value;
            }
        }

        public ushort ElementaryPID
        {
            get
            {
                return (ushort)(((mData[1] & 0x1f) << 8) + mData[2]);
            }
            set
            {
                mData[1] = (byte)(((value >> 8) & 0x1f) | 0xe0);
                mData[2] = (byte)(value & 0xff);
            }
        }

        public byte[] ElementaryDescriptors
        {
            get
            {
                if (mData.Length == 5)
                    return null;
                byte[] descriptors = new byte[mData.Length - 5];
                for (int i = 0; i < descriptors.Length; i++)
                {
                    descriptors[i] = mData[i + 5];
                }
                return descriptors;
            }
            set
            {
                if (null == value || 0 == value.Length)
                {
                    if (mData.Length > 5)
                    {
                        // need to remove existing descriptors
                        byte[] data = new byte[5];
                        data[0] = mData[0];
                        data[1] = mData[1];
                        data[2] = mData[2];
                        data[3] = 0xf0;
                        data[4] = 0x00;
                        mData = data;
                    }
                    else
                    {
                        // nothing to do
                        return;
                    }
                }
                else
                {
                    if (value.Length > 180)
                        throw new ArgumentException("descriptors data too long");
                    byte[] data = new byte[5 + value.Length];
                    data[0] = mData[0];
                    data[1] = mData[1];
                    data[2] = mData[2];
                    data[3] = (byte)(0xf0 | (byte)((value.Length >> 8) & 0x0f));
                    data[4] = (byte)(value.Length & 0xff);
                    for (int i = 0; i < value.Length; i++)
                    {
                        data[5 + i] = value[i];
                    }
                    mData = data;
                }
            }
        }
    }

    public class VC1SequenceInfo
    {
        private byte[] mData;

        public VC1SequenceInfo(byte[] data, int offset)
        {
            UInt32 marker = 0xffffffff;
            for (; offset < data.Length; offset++)
            {
                marker = marker << 8;
                marker &= 0xffffff00;
                marker += data[offset];
                if (marker == Constants.VC1_SEQ_SC)
                    break;
            }
            offset++;
            if (offset < data.Length)
            {
                // sequence header
                mData = new byte[data.Length - offset];
                for (int i = 0; offset < data.Length; i++, offset++)
                    mData[i] = data[offset];
            }
            else
                mData = null;
        }

        private int Height
        {
            get
            {
                if (mData != null && mData.Length > 4)
                    return ((((mData[3] & 0x0f) << 8) + mData[4]) << 1) + 2;
                else
                    return -1;
            }
        }

        private int Width
        {
            get
            {
                if (mData != null && mData.Length > 3)
                    return (((mData[2] << 4) + ((mData[3] & 0xf0) >> 4)) << 1) + 2;
                else
                    return -1;
            }
        }

        public bool Valid
        {
            get { return mData != null; }
        }

        private bool Interlaced
        {
            get
            {
                if (mData != null && mData.Length > 5)
                    return ((mData[5] & 0x40) > 0);
                return false;
            }
        }

        private bool DisplayExt
        {
            get
            {
                if (mData != null && mData.Length > 5)
                    return ((mData[5] & 0x02) > 0);
                return false;
            }
        }

        private bool AspectFlag
        {
            get
            {
                if (DisplayExt && mData.Length > 9)
                    return ((mData[9] & 0x10) > 0);
                return false;
            }
        }

        private byte Vc1AspectRatio
        {
            get
            {
                if (AspectFlag && mData.Length > 9)
                    return (byte)(mData[9] & 0x0f);
                return 0;
            }
        }

        private bool FrameFlag
        {
            get
            {
                if (AspectFlag)
                {
                    if (Vc1AspectRatio == 15 && mData.Length > 12)
                        return ((mData[12] & 0x80) > 0);
                    else if (Vc1AspectRatio != 15 && mData.Length > 10)
                        return ((mData[10] & 0x80) > 0);
                    else
                        return false;
                }
                else if (mData.Length > 9)
                    return ((mData[9] & 0x08) > 0);
                else
                    return false;
            }
        }

        private bool FrameRateIndicatorFlag
        {
            get
            {
                if (FrameFlag)
                {
                    if (AspectFlag)
                    {
                        if (Vc1AspectRatio == 15 && mData.Length > 12)
                            return ((mData[12] & 0x40) > 0);
                        else if (Vc1AspectRatio != 15 && mData.Length > 10)
                            return ((mData[10] & 0x40) > 0);
                        else
                            return false;
                    }
                    else if (mData.Length > 9)
                        return ((mData[9] & 0x04) > 0);
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public AspectRatio AspectRatio
        {
            get
            {
                if (Vc1AspectRatio == 1)
                {
                    if (Width == 1920 && Height == 1080)
                        return AspectRatio.a16_9;
                    if (Width == 1280 && Height == 720)
                        return AspectRatio.a16_9;
                    if (Width == 640 && Height == 480)
                        return AspectRatio.a4_3;
                }
                if (Vc1AspectRatio >= 2 && Vc1AspectRatio <= 5)
                    return AspectRatio.a4_3;
                if (Vc1AspectRatio >= 6 && Vc1AspectRatio <= 9)
                    return AspectRatio.a16_9;
                if (Vc1AspectRatio >= 10 && Vc1AspectRatio <= 11)
                    return AspectRatio.a4_3;
                if (Vc1AspectRatio >= 12 && Vc1AspectRatio <= 13)
                    return AspectRatio.a16_9;
                return AspectRatio.Reserved;
            }
        }

        public VideoFormat VideoFormat
        {
            get
            {
                if (Height == 480 && Interlaced == true)
                    return VideoFormat.i480;
                else if (Height == 480 && Interlaced == false)
                    return VideoFormat.p480;
                else if (Height == 576 && Interlaced == true)
                    return VideoFormat.i576;
                else if (Height == 576 && Interlaced == false)
                    return VideoFormat.p576;
                else if (Height == 720 && Interlaced == false)
                    return VideoFormat.p720;
                else if (Height == 1080 && Interlaced == true)
                    return VideoFormat.i1080;
                else if (Height == 1080 && Interlaced == false)
                    return VideoFormat.p1080;
                return VideoFormat.Reserved;
            }
        }

        public FrameRate FrameRate
        {
            get
            {
                if (false == FrameFlag)
                    return FrameRate.Reserved;
                if (false == FrameRateIndicatorFlag)
                {
                    byte FrameRateNr = 0;
                    byte FrameRateDr = 0;
                    if (AspectFlag)
                    {
                        if (Vc1AspectRatio == 15 && mData.Length > 13)
                        {
                            FrameRateNr = (byte)(((mData[12] & 0x3f) << 2) + ((mData[13] & 0xc0) >> 6));
                            FrameRateDr = (byte)((mData[13] & 0x3c) >> 2);
                        }
                        else if (Vc1AspectRatio != 15 && mData.Length > 11)
                        {
                            FrameRateNr = (byte)(((mData[10] & 0x3f) << 2) + ((mData[11] & 0xc0) >> 6));
                            FrameRateDr = (byte)((mData[11] & 0x3c) >> 2);
                        }
                    }
                    else if (mData.Length > 11)
                    {
                        FrameRateNr = (byte)(((mData[9] & 0x03) << 6) + ((mData[10] & 0xfc) >> 2));
                        FrameRateDr = (byte)(((mData[10] & 0x03) << 2) + ((mData[11] & 0xc0) >> 6));
                    }
                    if (FrameRateNr == 1 && FrameRateDr == 2)
                        return FrameRate.f23_976;
                    else if (FrameRateNr == 1 && FrameRateDr == 1)
                        return FrameRate.f24;
                    else if (FrameRateNr == 2 && FrameRateDr == 1)
                        return FrameRate.f25;
                    else if (FrameRateNr == 3 && FrameRateDr == 2)
                        return FrameRate.f29_97;
                    else if (FrameRateNr == 4 && FrameRateDr == 1)
                        return FrameRate.f50;
                    else if (FrameRateNr == 5 && FrameRateDr == 2)
                        return FrameRate.f59_94;
                }
                return FrameRate.Reserved;
            }
        }
    }

    public class H264Info : ElementaryParse
    {
        public H264Info(byte[] data, int offset)
            : base()
        {
            UInt32 marker = 0xffffffff;
            for (; offset < data.Length; offset++)
            {
                marker = marker << 8;
                marker &= 0xffffff00;
                marker += data[offset];
                if ((marker & 0xffffff9f) == Constants.H264_PREFIX)
                {
                    break;
                }
            }
            offset++;
            if (offset < data.Length)
            {
                // sequence parameter set
                mData = new byte[data.Length - offset];
                for (int i = 0; offset < data.Length; i++, offset++)
                    mData[i] = data[offset];
            }
            else
                mData = null;
        }

        private UInt32 GetNextExpGolomb()
        {
            int leadingZeroBits = -1;
            byte b = 0;
            for (; b == 0; leadingZeroBits++)
                b = GetNextBit();
            UInt32 codeNum = (UInt32)(1 << leadingZeroBits);
            codeNum -= 1;
            UInt32 part2 = 0;
            for (; leadingZeroBits > 0; leadingZeroBits-- )
            {
                b = GetNextBit();
                part2 = part2 << 1;
                part2 |= b;
            }
            codeNum += part2;
            return codeNum;
        }

        private void ScalingListSkip(int skip)
        {
            int lastScale = 8;
            int nextScale = 8;
            for (int i = 0; i < skip; i++)
            {
                if (nextScale != 0)
                {
                    int deltaScale = (int)GetNextExpGolomb();
                    nextScale = (lastScale + deltaScale) % 256;
                }
            }
        }

        private UInt32 Width
        {
            get
            {
                indicator = 24;
                GetNextExpGolomb();
                if (mData[0] == 100 || mData[0] == 110 || mData[0] == 122 || mData[0] == 144)
                {
                    UInt32 chroma = GetNextExpGolomb();
                    if (chroma == 3)
                        GetNextBit();
                    GetNextExpGolomb();
                    GetNextExpGolomb();
                    GetNextBit();
                    if (GetNextBit() == 1)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if (GetNextBit() == 1)
                            {
                                ScalingListSkip(16);
                            }
                        }
                        for (int i = 6; i < 8; i++)
                        {
                            if (GetNextBit() == 1)
                            {
                                ScalingListSkip(64);
                            }
                        }
                    }
                }
                GetNextExpGolomb();
                UInt32 pic = GetNextExpGolomb();
                if (pic == 0)
                    GetNextExpGolomb();     
                else if (pic == 1)
                {
                    GetNextBit();
                    GetNextExpGolomb();
                    GetNextExpGolomb();
                    UInt32 numFrame = GetNextExpGolomb();
                    for (int i = 0; i < numFrame; i++)
                        GetNextExpGolomb();
                }
                GetNextExpGolomb();
                GetNextBit();
                UInt32 wid = GetNextExpGolomb();
                wid++;
                wid <<= 4;
                return wid;
            }
        }

        private UInt32 Heigth
        {
            get
            {
                UInt32 width =  Width;
                UInt32 height = GetNextExpGolomb();
                height++;
                height <<= 4;
                return height;
            }
        }

        private byte[] HdmvVideoRegistrationDescriptor
        {
            get
            {
                byte[] data = new byte[10];
                data[0] = 0x05;
                data[1] = 0x08;
                data[2] = 0x48;
                data[3] = 0x44;
                data[4] = 0x4d;
                data[5] = 0x56;
                data[6] = 0xff;
                data[7] = 0x1b;
                data[8] = (byte)((((byte)VideoFormat) << 4) | ((byte)FrameRate));
                data[9] = (byte)((((byte)AspectRatio) << 4) | 0x0f);
                return data;
            }
        }

        public override VideoFormat VideoFormat
        {
            get
            {
                UInt32 h = Heigth;
                if (h == 1080 || h == 1088)
                    return VideoFormat.p1080;
                else if (h == 720)
                    return VideoFormat.p720;
                else if (h == 576)
                    return VideoFormat.p576;
                else if (h == 480)
                    return VideoFormat.p480;
                else if (h == 540 || h == 544)
                    return VideoFormat.i1080;
                else if (h == 288)
                    return VideoFormat.i576;
                else if (h == 240)
                    return VideoFormat.i480;
                else
                    return VideoFormat.Reserved;
            }
        }

        public override AspectRatio AspectRatio
        {
            get
            {
                if (VideoFormat == VideoFormat.i480 || VideoFormat == VideoFormat.i576)
                    return AspectRatio.a4_3;
                return AspectRatio.a16_9;
            }
        }

        public override FrameRate FrameRate
        {
            get
            {
                if (VideoFormat == VideoFormat.p720)
                    return FrameRate.f59_94;
                else if (VideoFormat == VideoFormat.p1080 || VideoFormat == VideoFormat.p480)
                    return FrameRate.f23_976;
                else if (VideoFormat == VideoFormat.p576 || VideoFormat == VideoFormat.i576)
                    return FrameRate.f25;
                else
                    return FrameRate.f29_97;
            }
        }

        public override byte[] ElementaryDescriptors
        {
            get { return HdmvVideoRegistrationDescriptor; }
        }

        public override AudioPresentationType AudioPresentationType
        {
            get { return AudioPresentationType.Reserved; }
        }

        public override SamplingFrequency SamplingFrequency
        {
            get { return SamplingFrequency.Reserved; }
        }
    }

    public enum Ac3SyntaxType
    {
        Invalid = 0,
        Standard = 8,
        Alternative = 6,
        Enhanced = 16
    }

    public class AC3Info : ElementaryParse
    {
        private static readonly int[] len48k = new int[38] {
             128,  128,  160,  160,  192,  192,  224,  224,
             256,  256,  320,  320,  384,  384,  448,  448,
             512,  512,  640,  640,  768,  768,  896,  896,
            1024, 1024, 1280, 1280, 1536, 1536, 1792, 1792,
            2048, 2048, 2304, 2304, 2560, 2560 };
        private static readonly int[] len44k = new int[38] {
             138,  140,  174,  176,  208,  210,  242,  244,
             278,  280,  348,  350,  416,  418,  486,  488,
             556,  558,  696,  698,  834,  836,  974,  976,
            1114, 1116, 1392, 1394, 1670, 1672, 1950, 1952,
            2228, 2230, 2506, 2508, 2786, 2788 };
        private static readonly int[] len32k = new int[38] {
             192,  192,  240,  240,  288,  288,  336,  336,
             384,  384,  480,  480,  576,  576,  672,  672,
             768,  768,  960,  960, 1152, 1152, 1344, 1344,
            1536, 1536, 1920, 1920, 2304, 2304, 2688, 2688,
            3072, 3072, 3456, 3456, 3840, 3840 };

        public int MaxFrameLength
        {
            get { return len32k[len32k.Length - 1]; }
        }

        public int FrameLength
        {
            get
            {
                if (SyntaxType == Ac3SyntaxType.Standard || SyntaxType == Ac3SyntaxType.Alternative)
                {
                    byte index = (byte)(mData[2] & 0x3f);
                    if (index < 38)
                    {
                        switch (SampleRateCode)
                        {
                            case 00:
                                return len48k[mData[2] & 0x3f];
                            case 01:
                                return len44k[mData[2] & 0x3f];
                            case 02:
                                return len32k[mData[2] & 0x3f];
                        }
                    }
                }
                else if (SyntaxType == Ac3SyntaxType.Enhanced)
                    return (((mData[0] & 0x03) << 8) + mData[1] + 1) << 1;
                return 0;
            }
        }

        public override bool Valid
        {
            get
            {
                return base.Valid && SyntaxType != Ac3SyntaxType.Invalid;
            }
        }
        private byte SampleRateCode
        {
            get
            {
                if (mData.Length > 2)
                    return (byte)(mData[2] >> 6);
                return 0x03;
            }
        }

        private byte Bsid
        {
            get
            {
                if (mData.Length > 3)
                    return (byte)(mData[3] >> 3);
                return 0;
            }
        }

        private byte Bsmod
        {
            get
            {
                if (mData.Length > 3 && (SyntaxType == Ac3SyntaxType.Standard || SyntaxType == Ac3SyntaxType.Alternative))
                    return (byte)(mData[3] & 0x07);
                return 0;
            }
        }

        private byte Acmod
        {
            get
            {
                if (mData.Length > 4 && (SyntaxType == Ac3SyntaxType.Standard || SyntaxType == Ac3SyntaxType.Alternative))
                    return (byte)(mData[4] >> 5);
                else if (mData.Length > 2 && SyntaxType == Ac3SyntaxType.Enhanced)
                    return (byte)((mData[2] >> 1) & 0x07);
                return 0x07;
            }
        }

        public bool IndependentStream
        {
            get
            {
                if (Ac3SyntaxType.Enhanced != SyntaxType)
                    return true;
                byte res = (byte)(mData[0] >> 6);
                if (res != 1)
                    return true;
                return false;
            }
        }

        public Ac3SyntaxType SyntaxType
        {
            get
            {
                switch (Bsid)
                {
                    case (byte)Ac3SyntaxType.Standard:
                        return Ac3SyntaxType.Standard;
                    case (byte)Ac3SyntaxType.Alternative:
                        return Ac3SyntaxType.Alternative;
                    case (byte)Ac3SyntaxType.Enhanced:
                        return Ac3SyntaxType.Enhanced;
                }
                return Ac3SyntaxType.Invalid;
            }
        }

        private byte[] AC3AudioDescriptor
        {
            get
            {
                List<byte> desc = new List<byte>();
                desc.Add(0x81);
                desc.Add(0x00);
                desc.Add((byte)((SampleRateCode << 5) | Bsid));
                desc.Add(200);
                desc.Add((byte)((Bsmod << 5) | (Acmod << 1) | 1));
                desc[1] = (byte)(desc.Count - 2);
                return desc.ToArray();
            }
        }

        public AC3Info(byte[] data, int offset)
            : base()
        {
            ushort marker = 0xffff;
            for (; offset < data.Length; offset++)
            {
                marker = (ushort)(marker << 8);
                marker &= 0xff00;
                marker += data[offset];
                if (marker == Constants.AC3_SYNC)
                    break;
            }
            offset++;
            if (offset < data.Length)
            {
                // sequence header
                mData = new byte[data.Length - offset];
                for (int i = 0; offset < data.Length; i++, offset++)
                    mData[i] = data[offset];
            }
            else
                mData = null;
        }

        public override AudioPresentationType AudioPresentationType
        {
            get
            {
                switch (Acmod)
                {
                    case 0x00:
                        return AudioPresentationType.stereo;
                    case 0x01:
                        return AudioPresentationType.mono;
                    case 0x02:
                        return AudioPresentationType.stereo;
                    default:
                        return AudioPresentationType.multi;
                }
            }
        }

        public override SamplingFrequency SamplingFrequency
        {
            get
            {
                switch (SampleRateCode)
                {
                    case 0x00:
                        return SamplingFrequency.kHz48;
                }
                return SamplingFrequency.Reserved;
            }
        }

        public override byte[] ElementaryDescriptors
        {
            get
            {
                List<byte> descriptors = new List<byte>();
                descriptors.AddRange(Constants.ac3_registration_descriptor);
                descriptors.AddRange(AC3AudioDescriptor);
                return descriptors.ToArray();
            }
        }

        public override AspectRatio AspectRatio
        {
            get { return AspectRatio.Reserved; }
        }

        public override FrameRate FrameRate
        {
            get { return FrameRate.Reserved; }
        }

        public override VideoFormat VideoFormat
        {
            get { return VideoFormat.Reserved; }
        }
    }

    public abstract class ElementaryParse
    {
        protected byte[] mData;
        protected int indicator;

        public ElementaryParse()
        {
            indicator = 0;
        }

        public virtual bool Valid
        {
            get { return mData != null; }
        }

        public abstract VideoFormat VideoFormat
        {
            get;
        }

        public abstract FrameRate FrameRate
        {
            get;
        }

        public abstract AspectRatio AspectRatio
        {
            get;
        }

        public abstract SamplingFrequency SamplingFrequency
        {
            get;
        }

        public abstract AudioPresentationType AudioPresentationType
        {
            get;
        }

        public abstract byte[] ElementaryDescriptors
        {
            get;
        }

        protected byte GetNextBit()
        {
            byte ret = (byte)(((mData[indicator / 8]) >> (7 - (indicator % 8))) & 1);
            indicator++;
            return ret;
        }
    }

    public class DtsInfo : ElementaryParse
    {
        public DtsInfo(byte[] data, int offset)
            : base()
        {
            UInt32 marker = 0xffffffff;
            for (; offset < data.Length; offset++)
            {
                marker = (UInt32)marker << 8;
                marker &= 0xffffff00;
                marker += data[offset];
                if (marker == Constants.DTS_SYNC)
                    break;
            }
            offset++;
            if (offset < data.Length)
            {
                // sequence header
                mData = new byte[data.Length - offset];
                for (int i = 0; offset < data.Length; i++, offset++)
                    mData[i] = data[offset];
            }
            else
                mData = null;
        }

        public ushort FrameSize
        {
            get
            {
                indicator = 14;
                ushort ret = 0;
                for (int i = 0; i < 14; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                ret++;
                return ret;
            }
        }

        private byte Amode
        {
            get
            {
                indicator = 28;
                byte ret = 0;
                for (int i = 0; i < 6; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                return ret;
            }
        }

        private byte SampleFreq
        {
            get
            {
                indicator = 34;
                byte ret = 0;
                for (int i = 0; i < 4; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                return ret;
            }
        }

        private bool ExtAudio
        {
            get
            {
                indicator = 51;
                if (1 == GetNextBit())
                    return true;
                return false;
            }
        }

        public byte ExtAudioId
        {
            get
            {
                if (false == ExtAudio)
                    return 0xff;
                indicator = 48;
                byte ret = 0;
                for (int i = 0; i < 3; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                return ret;
            }
        }

        public override AudioPresentationType AudioPresentationType
        {
            get
            {
                switch (Amode)
                {
                    case 0x00:
                        return AudioPresentationType.mono;
                    case 0x01:
                    case 0x02:
                    case 0x03:
                    case 0x04:
                        return AudioPresentationType.stereo;
                    default:
                        return AudioPresentationType.multi;
                }
            }
        }

        public override SamplingFrequency SamplingFrequency
        {
            get 
            {
                switch (SampleFreq)
                {
                    case 0xd:
                        return SamplingFrequency.kHz48;
                }
                return SamplingFrequency.Reserved;
            }
        }

        public override byte[] ElementaryDescriptors
        {
            get 
            {
                // DTS registration descriptor
                byte[] regdesc = new byte[6];
                regdesc[0] = 0x05;
                regdesc[1] = 0x04;
                regdesc[2] = 0x44;
                regdesc[3] = 0x54;
                regdesc[4] = 0x53;
                if (mData.Length < 0x400)
                    regdesc[5] = 0x31;
                else if (mData.Length < 0x800)
                    regdesc[5] = 0x32;
                else
                    regdesc[5] = 0x33;
                return regdesc;
            }
        }

        public override AspectRatio AspectRatio
        {
            get { return AspectRatio.Reserved; }
        }

        public override FrameRate FrameRate
        {
            get { return FrameRate.Reserved; }
        }

        public override VideoFormat VideoFormat
        {
            get { return VideoFormat.Reserved; }
        }
    }

    public class MlpInfo : ElementaryParse
    {
        public MlpInfo(byte[] data, int offset)
            : base()
        {
            UInt32 marker = 0xffffffff;
            for (; offset < data.Length; offset++)
            {
                marker = (UInt32)marker << 8;
                marker &= 0xffffff00;
                marker += data[offset];
                if (marker == Constants.MLP_SYNC)
                    break;
            }
            offset++;
            if (offset < data.Length)
            {
                // sequence header
                mData = new byte[data.Length - offset];
                for (int i = 0; offset < data.Length; i++, offset++)
                    mData[i] = data[offset];
            }
            else
                mData = null;
        }

        public override AspectRatio AspectRatio
        {
            get { return AspectRatio.Reserved; }
        }

        public override FrameRate FrameRate
        {
            get { return FrameRate.Reserved; }
        }

        public override VideoFormat VideoFormat
        {
            get { return VideoFormat.Reserved; }
        }

        public override byte[] ElementaryDescriptors
        {
            get 
            {
                List<byte> descriptors = new List<byte>();
                descriptors.AddRange(Constants.ac3_registration_descriptor);
                descriptors.AddRange(AC3AudioDescriptor);
                return descriptors.ToArray();
            }
        }

        public override AudioPresentationType AudioPresentationType
        {
            get
            {
                return AudioPresentationType.multi;
            }
        }

        public override SamplingFrequency SamplingFrequency
        {
            get
            {
                switch (SampleRateCode)
                {
                    case 0:
                        return SamplingFrequency.kHz48;
                    case 1:
                        return SamplingFrequency.kHz96;
                    case 2:
                        return SamplingFrequency.kHz192;
                    case 8: // 44.1kHz
                    case 9: // 88.2kHz
                    case 10: // 176.4kHz
                    default:
                        return SamplingFrequency.Reserved;
                }
            }
        }

        private byte[] AC3AudioDescriptor
        {
            get
            {
                List<byte> desc = new List<byte>();
                desc.Add(0x81);
                desc.Add(0x00);
                desc.Add((byte)((SampleRateCode << 5) | 0x08));
                desc.Add(200);
                desc.Add((byte)(0x0f));
                desc[1] = (byte)(desc.Count - 2);
                return desc.ToArray();
            }
        }

        private byte SampleRateCode
        {
            get
            {
                if (mData.Length > 0)
                    return (byte)(mData[0] >> 4);
                return 0x00;
            }
        }
    }

    public class Mpeg2Info : ElementaryParse
    {
        private Mpeg2Ext mpgext;

        private class Mpeg2Ext : ElementaryParse
        {
            public Mpeg2Ext(byte[] data, int offset)
                : base()
            {
                UInt32 marker = 0xffffffff;
                for (; offset < data.Length - 1; offset++)
                {
                    marker = (UInt32)marker << 8;
                    marker &= 0xffffff00;
                    marker += data[offset];
                    if (marker == Constants.MPEG2_SEQ_EXT)
                    {
                        if((data[offset + 1] & 0xf0) == 0x10)
                            break;
                    }
                }
                offset++;
                if (offset < data.Length)
                {
                    // sequence header
                    mData = new byte[data.Length - offset];
                    for (int i = 0; offset < data.Length; i++, offset++)
                        mData[i] = data[offset];
                }
                else
                    mData = null;
            }

            public override AspectRatio AspectRatio
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }
            public override AudioPresentationType AudioPresentationType
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }
            public override byte[] ElementaryDescriptors
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }
            public override FrameRate FrameRate
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }
            public override SamplingFrequency SamplingFrequency
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }
            public override VideoFormat VideoFormat
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }
            public bool Progressive
            {
                get
                {
                    indicator = 12;
                    if (GetNextBit() == 1)
                        return true;
                    return false;
                }
            }
        }

        public Mpeg2Info(byte[] data, int offset)
            : base()
        {
            UInt32 marker = 0xffffffff;
            int oldOffset = offset;
            for (; offset < data.Length; offset++)
            {
                marker = (UInt32)marker << 8;
                marker &= 0xffffff00;
                marker += data[offset];
                if (marker == Constants.MPEG2_SEQ_CODE)
                    break;
            }
            offset++;
            if (offset < data.Length)
            {
                // sequence header
                mData = new byte[data.Length - offset];
                for (int i = 0; offset < data.Length; i++, offset++)
                    mData[i] = data[offset];
                mpgext = new Mpeg2Ext(data, oldOffset);
            }
            else
                mData = null;
        }

        public override AspectRatio AspectRatio
        {
            get
            {
                switch (Aspect)
                {
                    case 0x01:
                        if (Vertical == 1080 || Vertical == 1088 || Vertical == 720)
                            return AspectRatio.a16_9;
                        else
                            return AspectRatio.a4_3;
                    case 0x02:
                        return AspectRatio.a4_3;
                    case 0x03:
                        return AspectRatio.a16_9;
                    default:
                        return AspectRatio.Reserved;
                }
            }
        }

        public override FrameRate FrameRate
        {
            get
            {
                switch (FrameRateCode)
                {
                    case 0x01:
                        return FrameRate.f23_976;
                    case 0x02:
                        return FrameRate.f24;
                    case 0x03:
                        return FrameRate.f25;
                    case 0x04:
                        return FrameRate.f29_97;
                    case 0x06:
                        return FrameRate.f50;
                    case 0x07:
                        return FrameRate.f59_94;
                    default:
                        return FrameRate.Reserved;
                }
            }
        }

        public override VideoFormat VideoFormat
        {
            get
            {
                if (Vertical == 1080 || Vertical == 1088)
                {
                    if (Progressive)
                        return VideoFormat.p1080;
                    else
                        return VideoFormat.i1080;
                }
                else if (Vertical == 576)
                {
                    if (Progressive)
                        return VideoFormat.p576;
                    else
                        return VideoFormat.i576;
                }
                else if (Vertical == 720)
                    return VideoFormat.p720;
                else if (Vertical == 480)
                {
                    if (Progressive)
                        return VideoFormat.p480;
                    else
                        return VideoFormat.i480;
                }
                return VideoFormat.Reserved;
            }
        }

        public override byte[] ElementaryDescriptors
        {
            get { return Mpeg2VideoRegistrationDescriptor; }
        }

        public override AudioPresentationType AudioPresentationType
        {
            get { return AudioPresentationType.Reserved; }
        }

        public override SamplingFrequency SamplingFrequency
        {
            get { return SamplingFrequency.Reserved; }
        }

        private byte[] Mpeg2VideoRegistrationDescriptor
        {
            get
            {
                byte[] data = new byte[10];
                data[0] = 0x05;
                data[1] = 0x08;
                data[2] = 0x48;
                data[3] = 0x44;
                data[4] = 0x4d;
                data[5] = 0x56;
                data[6] = 0xff;
                data[7] = 0x02;
                data[8] = (byte)((((byte)VideoFormat) << 4) | ((byte)FrameRate));
                data[9] = (byte)((((byte)AspectRatio) << 4) | 0x0f);
                return data;
            }
        }

        private ushort Horizontal
        {
            get
            {
                indicator = 0;
                ushort ret = 0;
                for (int i = 0; i < 12; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                return ret;
            }
        }

        private ushort Vertical
        {
            get
            {
                indicator = 12;
                ushort ret = 0;
                for (int i = 0; i < 12; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                return ret;
            }
        }

        private byte Aspect
        {
            get
            {
                indicator = 24;
                byte ret = 0;
                for (int i = 0; i < 4; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                return ret;
            }
        }

        private byte FrameRateCode
        {
            get
            {
                indicator = 28;
                byte ret = 0;
                for (int i = 0; i < 4; i++)
                {
                    ret <<= 1;
                    ret |= GetNextBit();
                }
                return ret;
            }
        }

        private bool Progressive
        {
            get
            {
                if (mpgext.Valid && mpgext.Progressive)
                    return true;
                return false;
            }
        }
    }
}
