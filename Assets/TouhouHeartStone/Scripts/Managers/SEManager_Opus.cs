using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityOpus;

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

            // 写入文件版本字节。
            stream.WriteByte(audioVersion);

            // 写入目录字节。
            var tocByte = getTOCByte(true, frequency, frameSizeLevel, channels);
            stream.WriteByte(tocByte);

            // 创建采样和字节缓冲区。
            var sampleBufferSize = getSampleBufferSize(frequency, channels, frameSizeLevel);
            var byteBufferSize = getByteBufferSize(sampleBufferSize);
            var sampleBuffer = new float[sampleBufferSize];
            var byteBuffer = new byte[byteBufferSize];

            int head = 0;
            while (head < pcmSamples.Length)
            {
                // 将AudioClip中的采样复制到采样缓冲区内。
                for (int i = 0; i < sampleBufferSize; i++)
                {
                    var index = i + head;
                    sampleBuffer[i] = index >= pcmSamples.Length ? 0 : pcmSamples[index];
                }

                // 编码缓冲区内的采样为一个音频帧。
                var encodedLength = encoder.Encode(sampleBuffer, byteBuffer);

                // 写入该音频帧的长度。
                // 1-251表示该帧的长度。
                // 若为252-255，则使用第二个字节同时表示长度，为（第二字节）*4+第一字节。
                if (encodedLength < 252)
                {
                    stream.WriteByte((byte)encodedLength);
                }
                else
                {
                    var length = encodedLength - 252;
                    byte secByte = (byte)(length / 4);
                    byte firstByte = (byte)(length % 4 + 252);
                    stream.WriteByte(firstByte);
                    stream.WriteByte(secByte);
                }

                // 写入该音频帧的数据。
                stream.Write(byteBuffer, 0, encodedLength);
                head += sampleBufferSize;
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
            // 读取版本号字节。
            var versionByte = (byte)stream.ReadByte();

            // 读取包括音频信息的目录字节。
            byte tocByte = (byte)stream.ReadByte();

            // 解析音频目录字节。
            parseTOCByte(tocByte, out var _, out var frequency, out var frameSizeLevel, out var channels);

            // 创建缓冲区字节。
            var sampleBufferSize = getSampleBufferSize(frequency, channels, frameSizeLevel);
            var byteBufferSize = getByteBufferSize(sampleBufferSize);
            var sampleBuffer = new float[sampleBufferSize];
            var byteBuffer = new byte[byteBufferSize];

            // 采样列表。
            var samples = new List<float>();

            // 创建解码器。
            using var decoder = new Decoder((SamplingFrequency)frequency, (NumChannels)channels);

            while (stream.Position < stream.Length)
            {
                // 读取下一个音频帧的长度。
                var frameByteLength = 0;
                var framelengthByte1 = stream.ReadByte();
                if (framelengthByte1 >= 252)
                {
                    var frameLengthByte2 = stream.ReadByte();
                    frameByteLength = frameLengthByte2 * 4 + framelengthByte1;
                }
                else
                {
                    frameByteLength = framelengthByte1;
                }

                // 根据读取到的长度，读取音频帧数据。
                stream.Read(byteBuffer, 0, frameByteLength);

                // 将音频帧解码为采样。
                var decodedLength = decoder.Decode(byteBuffer, frameByteLength, sampleBuffer);
                for (int i = 0; i < decodedLength * channels; i++)
                {
                    samples.Add(sampleBuffer[i]);
                }
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
            // 频率（hz） * 通道数量 * 音频帧长度（ms）。

            // 其中frameSizeLevel为0-3时，音频帧长度代表2.5ms，5ms，10ms，20ms。
            return frequency * channels * (int)Mathf.Pow(2, frameSizeLevel) / 400;
        }
        private static int getByteBufferSize(int sampleSize)
        {
            // 采样缓冲区大小 * 4字节。
            return sampleSize * sizeof(int);
        }

        #region 目录字节生成/解析
        private static byte getConfigBits(bool isCELT, int frequency, int frameSizeLevel)
        {
            byte celtBits = (byte)(isCELT ? 1 << 4 : 0);
            byte bandBits = 0;
            byte frameSizeLevelBits = 0;
            if (isCELT)
            {
                switch (frequency)
                {
                    case 8000:
                        bandBits = 0 << 2;
                        break;
                    case 16000:
                        bandBits = 1 << 2;
                        break;
                    case 24000:
                        bandBits = 2 << 2;
                        break;
                    default: // 48000
                        bandBits = 3 << 2;
                        break;
                }
                frameSizeLevelBits = (byte)(frameSizeLevel & 3);
            }
            else
            {
                int frameLevelByte = frameSizeLevel;
                switch (frequency)
                {
                    case 8000:
                        bandBits = 0 << 2;
                        frameLevelByte = frameLevelByte - 2;
                        break;
                    case 12000:
                        bandBits = 1 << 2;
                        frameLevelByte = frameLevelByte - 2;
                        break;
                    case 16000:
                        bandBits = 2 << 2;
                        frameLevelByte = frameLevelByte - 2;
                        break;
                    case 24000:
                        bandBits = 3 << 2;
                        frameLevelByte = frameLevelByte - 2;
                        break;
                    case 48000:
                        bandBits = 3 << 2;
                        break;
                }
                frameSizeLevelBits = (byte)(frameLevelByte & 3);
            }
            return (byte)(celtBits | bandBits | frameSizeLevelBits);
        }
        private static byte getTOCByte(bool isCELT, int frequency, int frameSizeLevel, int channels)
        {
            var configBits = getConfigBits(isCELT, frequency, frameSizeLevel);
            return (byte)((configBits << 3) | ((channels - 1) << 2) | 3);
        }
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
        private const int audioVersion = 1;
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
    }
}
