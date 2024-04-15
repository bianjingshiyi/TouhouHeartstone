using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Game
{
    public partial class SEManager
    {
        private class OggPageData
        {
            /// <summary>
            /// 是否是从上一个页面内的包延续而来。
            /// </summary>
            public bool continuedPacket;
            /// <summary>
            /// 是否是逻辑流中的第一个包。
            /// </summary>
            public bool firstPacketInStream;
            /// <summary>
            /// 是否是逻辑流中的最后一个包。
            /// </summary>
            public bool lastPacketInStream;
            /// <summary>
            /// 位置数，页内最后一个包的结尾，距离逻辑流的开始位置的长度。
            /// 如果业内没有完整的包，为-1。
            /// </summary>
            public long granulePosition;
            /// <summary>
            /// 当多个逻辑流复合在一个Ogg物理流里面时，每个逻辑流都会有一个独特的编号，其对应的Ogg页都打上对应编号，用于区分这个Ogg页属于哪一个逻辑流。
            /// </summary>
            public uint bitstreamSerialNumber;
            /// <summary>
            /// 该Ogg页的序号。
            /// 如果一个Ogg物理流里面有多个逻辑流，那么每个逻辑流的Ogg页序列号是独立增长的。
            /// </summary>
            public uint pageSequenceNumber;
            /// <summary>
            /// 该页面内存在多少个页面片段。
            /// </summary>
            public byte pageSegments;
            /// <summary>
            /// 片段表，记录所有页面片段的长度。
            /// </summary>
            public byte[] segmentTable;
            public byte[] content;

            public byte[] getBytes()
            {
                bool littleEndian = BitConverter.IsLittleEndian;
                List<byte> list = new List<byte>();
                // 0-3：固定是OggS
                list.Add((byte)'O');
                list.Add((byte)'g');
                list.Add((byte)'g');
                list.Add((byte)'S');

                // 4：版本号，固定为0
                list.Add(0);

                // 5：头类型标志
                // 1<<0: 置0 = 从新的包开始
                //       置1 = 这个页里面的包接着上一个页里面的包
                // 1<<1: 置0 = 不是逻辑流的第一个Ogg页
                //       置1 = 逻辑流的第一个Ogg页（bos）
                // 1<<2: 置0 = 不是逻辑流的最后一个Ogg页
                //       置1 = 逻辑流的最后一个Ogg页（eos）
                byte headerFlags = 0;
                headerFlags = (byte)(headerFlags | (continuedPacket ? 1 << 0 : 0));
                headerFlags = (byte)(headerFlags | (firstPacketInStream ? 1 << 1 : 0));
                headerFlags = (byte)(headerFlags | (lastPacketInStream ? 1 << 2 : 0));
                list.Add(headerFlags);

                // 6-13：位置数
                var granuleBytes = BitConverter.GetBytes(granulePosition);
                list.AddRange(littleEndian ? granuleBytes : granuleBytes.Reverse());

                // 14-17：逻辑流编号
                var serialNumberBytes = BitConverter.GetBytes(bitstreamSerialNumber);
                list.AddRange(littleEndian ? serialNumberBytes : serialNumberBytes.Reverse());

                // 18-21：页序列号
                var pageNumberBytes = BitConverter.GetBytes(pageSequenceNumber);
                list.AddRange(littleEndian ? pageNumberBytes : pageNumberBytes.Reverse());

                // 21-25：校验和（最后设置）
                list.AddRange(BitConverter.GetBytes(0));

                // 26：分段数量
                list.Add(pageSegments);

                // 27+：分段表
                list.AddRange(segmentTable);

                // 内容
                list.AddRange(content);

                var bytes = list.ToArray();
                var checksum = Crc23(bytes);
                for (int i = 0; i < checksum.Length; i++)
                {
                    bytes[22 + i] = checksum[i];
                }
                return bytes;
            }
            public static OggPageData read(Stream stream)
            {
                var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

                // 0-3: OggS魔数。
                var magicBytes = reader.ReadBytes(4);
                if (Encoding.ASCII.GetString(magicBytes) != "OggS")
                    return null;

                // 4：版本号，固定为0
                byte version = reader.ReadByte();

                // 5：头类型标志
                // 1<<0: 置0 = 从新的包开始
                //       置1 = 这个页里面的包接着上一个页里面的包
                // 1<<1: 置0 = 不是逻辑流的第一个Ogg页
                //       置1 = 逻辑流的第一个Ogg页（bos）
                // 1<<2: 置0 = 不是逻辑流的最后一个Ogg页
                //       置1 = 逻辑流的最后一个Ogg页（eos）
                byte headerFlags = reader.ReadByte();
                bool continuedPacket = (headerFlags & (1 << 0)) > 0;
                bool firstPacketInStream = (headerFlags & (1 << 1)) > 0;
                bool lastPacketInStream = (headerFlags & (1 << 2)) > 0;

                // 6-13：位置数
                long granuleNumber = reader.ReadInt64();

                // 14-17：逻辑流编号
                uint serialNumber = reader.ReadUInt32();

                // 18-21：页序列号
                uint pageSequenceNumber = reader.ReadUInt32();

                // 21-25：校验和（最后设置）
                uint checksum = reader.ReadUInt32();

                // 26：分段数量
                byte segements = reader.ReadByte();

                // 27+：分段表
                byte[] segementTable = reader.ReadBytes(segements);

                // 内容
                int contentLength = segementTable.Sum(b => b);
                byte[] content = reader.ReadBytes(contentLength);

                return new OggPageData()
                {
                    continuedPacket = continuedPacket,
                    firstPacketInStream = firstPacketInStream,
                    lastPacketInStream = lastPacketInStream,
                    bitstreamSerialNumber = serialNumber,
                    granulePosition = granuleNumber,
                    pageSequenceNumber = pageSequenceNumber,
                    pageSegments = segements,
                    segmentTable = segementTable,
                    content = content
                };
            }
            public static byte[] Crc23(byte[] buffer, int start = 0, int len = 0)
            {
                if (buffer == null || buffer.Length == 0) return null;

                if (start < 0) return null;

                if (len == 0) len = buffer.Length - start;

                int end = start + len;
                if (end > buffer.Length) return null;

                uint initialValue = 0;
                uint poly = 0x04C11DB7;
                uint crc = initialValue;// Initial value
                for (int i = start; i < end; i++)
                {
                    crc ^= (uint)(buffer[i] << 24);
                    for (int j = 0; j < 8; j++)
                    {
                        if ((crc & 0x80000000) > 0)
                            crc = (crc << 1) ^ poly;
                        else
                            crc = crc << 1;
                    }
                }
                byte[] ret = BitConverter.GetBytes(crc);
                return ret;
            }
        }
    }
}
