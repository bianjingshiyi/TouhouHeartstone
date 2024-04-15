using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityOpus;
using Decoder = UnityOpus.Decoder;
using Encoder = UnityOpus.Encoder;

namespace Game
{
    public partial class SEManager
    {
        #region 公有方法

        #region 编码
        public byte[] EncodeOpus(AudioClip clip)
        {
            using var stream = new MemoryStream();
            EncodeOpusToStream(clip, stream);
            return stream.ToArray();
        }
        public void EncodeOpusToStream(AudioClip clip, Stream stream)
        {
            // 读取Audio Clip的频率和声道数量。
            var frequency = clip.frequency;
            var channels = clip.channels;

            // 读取Audio Clip的采样。
            var pcmSamples = new float[clip.samples * channels];
            clip.GetData(pcmSamples, 0);

            // Opus不支持的采样率，重新采样。
            if (!supportedFrequencies.Contains(frequency))
            {
                var newFrequency = supportedFrequencies.FirstOrDefault(f => f > frequency);
                if (newFrequency == 0)
                    newFrequency = defaultFrequency;

                pcmSamples = ConvertFrequency(pcmSamples, channels, frequency, newFrequency);
                frequency = newFrequency;
            }

            // 新建编码器。
            using var encoder = new Encoder((SamplingFrequency)frequency, (NumChannels)channels, OpusApplication.Audio)
            {
                Bitrate = bitrate,
                Complexity = 10,
                Signal = OpusSignal.Music
            };

            writeOpusHeaderPage(frequency, channels, stream);
            writeOpusCommentPage(stream);

            // 创建采样和字节缓冲区。
            var sampleBufferSize = getSampleBufferSize(frequency, channels, frameSizeLevel);
            var byteBufferSize = getByteBufferSize(sampleBufferSize);
            var sampleBuffer = new float[sampleBufferSize];
            var byteBuffer = new byte[byteBufferSize];

            int head = 0;
            uint sequenceNumber = 2;
            while (head < pcmSamples.Length)
            {
                // 将AudioClip中的采样复制到采样缓冲区内。
                for (int i = 0; i < sampleBufferSize; i++)
                {
                    var index = i + head;
                    sampleBuffer[i] = index >= pcmSamples.Length ? 0 : pcmSamples[index];
                }

                head += sampleBufferSize;
                // 编码缓冲区内的采样为一个音频帧。
                var encodedLength = encoder.Encode(sampleBuffer, byteBuffer);

                // 获取Ogg页面头。
                var segmentCount = Mathf.FloorToInt(encodedLength / 255f) + 1;
                byte lastSegmentLength = (byte)(encodedLength % 255);
                var segmentTable = new byte[segmentCount];
                for (int seg = 0; seg < segmentCount; seg++)
                {
                    segmentTable[seg] = seg == segmentCount - 1 ? lastSegmentLength : (byte)255;
                }
                var pageData = new OggPageData()
                {
                    firstPacketInStream = false,
                    continuedPacket = false,
                    lastPacketInStream = head >= pcmSamples.Length,
                    bitstreamSerialNumber = 0,
                    granulePosition = Mathf.Min(head, pcmSamples.Length) / channels,
                    pageSequenceNumber = sequenceNumber,
                    pageSegments = (byte)segmentCount,
                    segmentTable = segmentTable,
                    content = byteBuffer.Take(encodedLength).ToArray()
                };
                // 写入该音频帧的数据。
                stream.Write(pageData.getBytes());
                sequenceNumber++;
            }
        }
        #endregion

        #region 解码
        public AudioClip DecodeOpus(byte[] data, string audioName = null)
        {
            using var stream = new MemoryStream(data);
            return DecodeOpusFromStream(stream, audioName);
        }
        public AudioClip DecodeOpusFromStream(Stream stream, string audioName = null)
        {
            var headerPage = OggPageData.read(stream);

            var headerStream = new MemoryStream(headerPage.content);
            var header = OpusHeader.read(headerStream);
            var frequency = (int)header.inputSampleRate;
            var channels = header.channels;

            var commentPage = OggPageData.read(stream);

            // 采样列表。
            var samples = new List<float>();

            // 创建解码器。
            using var decoder = new Decoder((SamplingFrequency)frequency, (NumChannels)channels);

            // 创建缓冲区字节。
            var sampleBufferSize = getSampleBufferSize(frequency, channels, frameSizeLevel);
            var byteBufferSize = getByteBufferSize(sampleBufferSize);
            var sampleBuffer = new float[sampleBufferSize];
            var byteBuffer = new byte[byteBufferSize];

            long lastGranule = 0;
            while (stream.Position < stream.Length)
            {
                var page = OggPageData.read(stream);

                // 将音频帧解码为采样。
                var samplePosition = page.granulePosition * channels;
                var decodedLength = decoder.Decode(page.content, page.content.Length, sampleBuffer);
                for (int i = 0; i < samplePosition - lastGranule; i++)
                {
                    samples.Add(sampleBuffer[i]);
                }
                lastGranule = samplePosition;
            }
            // 生成Audio Clip并设置采样。
            var clip = AudioClip.Create(audioName ?? "DecodedAudio", samples.Count / channels, channels, frequency, false);
            clip.SetData(samples.ToArray(), 0);
            return clip;
        }
        #endregion

        #endregion

        #region 私有方法
        private static int getSampleBufferSize(int frequency, int channels, int frameSizeLevel)
        {
            // Opus支持的采样缓冲区大小为：
            // 频率（hz） * 通道数量 * 音频帧长度（s）。

            // 其中frameSizeLevel为0-3时，音频帧长度代表2.5ms，5ms，10ms，20ms。
            return frequency * channels * (int)Mathf.Pow(2, frameSizeLevel) / 400;
        }
        private static int getByteBufferSize(int sampleSize)
        {
            // 采样缓冲区大小 * 4字节。
            return sampleSize * sizeof(int);
        }
        private static void writeOpusHeaderPage(int frequency, int channels, Stream stream)
        {
            var header = new OpusHeader()
            {
                channels = (byte)channels,
                inputSampleRate = (ushort)frequency,
            };
            var bytes = header.getBytes();
            // 获取Ogg页面头。
            var pageData = new OggPageData()
            {
                firstPacketInStream = true,
                continuedPacket = false,
                lastPacketInStream = false,
                bitstreamSerialNumber = 0,
                granulePosition = 0,
                pageSequenceNumber = 0,
                pageSegments = 1,
                segmentTable = new byte[] { (byte)bytes.Length },
                content = bytes
            };
            stream.Write(pageData.getBytes());
        }
        private static void writeOpusCommentPage(Stream stream)
        {
            var header = new OpusComment()
            {
                ventorString = string.Empty,
                userComments = new string[0]
            };
            var bytes = header.getBytes();
            // 获取Ogg页面头。
            var pageData = new OggPageData()
            {
                firstPacketInStream = false,
                continuedPacket = false,
                lastPacketInStream = false,
                bitstreamSerialNumber = 0,
                granulePosition = 0,
                pageSequenceNumber = 1,
                pageSegments = 1,
                segmentTable = new byte[] { (byte)bytes.Length },
                content = bytes
            };
            stream.Write(pageData.getBytes());
        }

        #region 目录字节解析
        private static void parseConfigBits(byte configBits, out bool isCELT, out int frequency, out int frameSizeLevel)
        {
            isCELT = (configBits >> 4) > 0;
            var frequencyBits = (configBits >> 2) & 3;
            if (isCELT)
            {
                switch (frequencyBits)
                {
                    default:
                        frequency = 8000;
                        break;
                    case 1:
                        frequency = 16000;
                        break;
                    case 2:
                        frequency = 24000;
                        break;
                    case 3: // 48000
                        frequency = 48000;
                        break;
                }
                frameSizeLevel = configBits & 3;
            }
            else
            {
                int frameLevelBits = configBits & 3;
                switch (frequencyBits)
                {
                    default:
                        frequency = 8000;
                        frameSizeLevel = frameLevelBits + 2;
                        break;
                    case 1:
                        frequency = 12000;
                        frameSizeLevel = frameLevelBits + 2;
                        break;
                    case 2:
                        frequency = 16000;
                        frameSizeLevel = frameLevelBits + 2;
                        break;
                    case 3:
                        if (frameLevelBits < 2)
                        {
                            frequency = 24000;
                            frameSizeLevel = frameLevelBits + 2;
                        }
                        else
                        {
                            frequency = 48000;
                            frameSizeLevel = frameLevelBits;
                        }
                        break;
                }
            }
        }
        private static void parseTOCByte(byte tocByte, out bool isCELT, out int frequency, out int frameSizeLevel, out int channels)
        {
            var configBits = tocByte >> 3;
            parseConfigBits(tocByte, out isCELT, out frequency, out frameSizeLevel);
            channels = (tocByte & (1 << 2)) > 0 ? 2 : 1;
        }
        #endregion

        #endregion

        #region 属性字段
        private const int defaultFrequency = 48000;
        private static readonly int[] supportedFrequencies =
        {
            8000,
            12000,
            16000,
            24000,
            48000
        };

        [Header("Opus Compression")]
        [SerializeField]
        private int bitrate = 96000;
        [SerializeField]
        [Range(0, 3)]
        private int frameSizeLevel = 3;
        #endregion

        private class OpusHeader
        {
            public byte channels;
            public ushort preskip;
            public uint inputSampleRate;
            public ushort outputGain;
            public byte mappingFamily;
            public byte[] getBytes()
            {
                using var stream = new MemoryStream();
                write(stream);
                return stream.ToArray();
            }
            public void write(Stream stream)
            {
                var encoding = Encoding.ASCII;
                using var writer = new BinaryWriter(stream, encoding, leaveOpen: true);

                // 0-7: 固定的OpusHead
                writer.Write(encoding.GetBytes("OpusHead"));

                // 8: 版本，固定为1
                writer.Write((byte)1);

                // 9: 输出声道数
                writer.Write(channels);

                // 10-11: 前置插入的无用采样数
                writer.Write(preskip);

                // 12-15: 输入采样率
                writer.Write(inputSampleRate);

                // 16-17: 输出增益
                writer.Write(outputGain);

                // 18: 声道映射类，取值，0，1，255
                writer.Write(mappingFamily);

                // 19+: 声道映射表，声道映射类为0时不存在。
            }
            public static OpusHeader read(Stream stream)
            {
                var encoding = Encoding.ASCII;
                using var reader = new BinaryReader(stream, encoding, leaveOpen: true);

                // 0-7: 固定的OpusHead
                var magic = reader.ReadBytes(8);
                if (encoding.GetString(magic) != "OpusHead")
                    return null;

                // 8: 版本，固定为1
                byte version = reader.ReadByte();

                // 9: 输出声道数
                byte channels = reader.ReadByte();

                // 10-11: 前置插入的无用采样数
                ushort preskip = reader.ReadUInt16();

                // 12-15: 输入采样率
                uint inputSampleRate = reader.ReadUInt32();

                // 16-17: 输出增益
                ushort outputGain = reader.ReadUInt16();

                // 18: 声道映射类，取值，0，1，255
                byte mappingFamily = reader.ReadByte();

                // 19+: 声道映射表，声道映射类为0时不存在。

                return new OpusHeader()
                {
                    channels = channels,
                    inputSampleRate = inputSampleRate,
                    preskip = preskip,
                    mappingFamily = mappingFamily,
                    outputGain = outputGain
                };
            }
        }
        private class OpusComment
        {
            public string ventorString;
            public string[] userComments;
            public byte[] getBytes()
            {
                using var stream = new MemoryStream();
                write(stream);
                return stream.ToArray();
            }
            public void write(Stream stream)
            {
                var encoding = Encoding.ASCII;
                using var writer = new BinaryWriter(stream, encoding, leaveOpen: true);

                // 0-7: 固定的OpusTags
                writer.Write(encoding.GetBytes("OpusTags"));

                // 8-11: 厂商字符串长度
                writer.Write(encoding.GetByteCount(ventorString));
                // 12-n: 厂商字符串
                writer.Write(encoding.GetBytes(ventorString));

                // n-n+4: 注解列表长度
                writer.Write(userComments.Length);
                foreach (var userComment in userComments)
                {
                    writer.Write(encoding.GetByteCount(userComment));
                    writer.Write(encoding.GetBytes(userComment));
                }
            }
            public static OpusComment read(Stream stream)
            {
                var encoding = Encoding.ASCII;
                using var reader = new BinaryReader(stream, encoding, leaveOpen: true);

                // 0-7: 固定的OpusHead
                var magic = reader.ReadBytes(8);
                if (encoding.GetString(magic) != "OpusTags")
                    return null;

                // 8-11: 厂商字符串长度
                var ventorStringLength = reader.ReadInt32();
                // 12-n: 厂商字符串
                var ventorStringBytes = reader.ReadBytes(ventorStringLength);
                var ventorString = encoding.GetString(ventorStringBytes);

                // n-n+4: 注解列表长度
                var userCommentLength = reader.ReadInt32();
                var userComments = new string[userCommentLength];
                for (int i = 0; i < userCommentLength; i++)
                {
                    var commentLength = reader.ReadInt32();
                    var commentStringBytes = reader.ReadBytes(commentLength);
                    var commentString = encoding.GetString(commentStringBytes);
                    userComments[i] = commentString;
                }

                return new OpusComment()
                {
                    ventorString = ventorString,
                    userComments = userComments
                };
            }
        }
    }
}
