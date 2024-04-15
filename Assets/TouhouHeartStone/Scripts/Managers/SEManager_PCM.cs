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

        #region 音频频率转换
        public static float[] ConvertFrequency(float[] samples, int channels, int sourceFrequency, int destFrequency)
        {
            // 重采样率，新的采样率每采样一次，旧的采样率便前进该数值。
            var resampleRate = destFrequency / (float)sourceFrequency;

            // 每个声道的的重采样长度。
            var resampleLengthPerChannel = Mathf.CeilToInt(samples.Length * resampleRate / channels);

            // 重采样过后的采样数据。包括每个声道的采样。
            var convertedSamples = new float[resampleLengthPerChannel * channels];

            // 遍历每个声道的重采样。
            for (int i = 0; i < resampleLengthPerChannel; i++)
            {
                // 采样位置，表示在该重采样位置中，旧的采样率目前处于什么位置。
                var samplePos = i / resampleRate;

                // 该采样位置左右两侧的采样位置。
                var leftSamplePos = Mathf.FloorToInt(samplePos);
                var rightSamplePos = leftSamplePos + 1;

                // 重采样的位置被夹在两个采样位置之间的位置。
                var v = samplePos - leftSamplePos;

                // 为每个声道生成一次重采样。PCM格式中的声道采样是交替进行的。（例：1，2，1，2，1，2）
                for (int channel = 0; channel < channels; channel++)
                {
                    // 重采样的数组索引。
                    var index = i * channels + channel;

                    // 左右两侧的数组索引。
                    var left = leftSamplePos * channels + channel;
                    var right = rightSamplePos * channels + channel;

                    // 左右两侧的采样。
                    float leftSample = left < samples.Length ? samples[left] : 0;
                    float rightSample = right < samples.Length ? samples[right] : leftSample;

                    // 根据左右两侧采样的线性插值重新生成一个采样结果。
                    convertedSamples[index] = Mathf.Lerp(leftSample, rightSample, v);
                }
            }
            return convertedSamples;
        }
        #endregion

        #region 绘制波形
        public static Texture2D GenerateWaveTexture(AudioClip clip, int width = 1024, int height = 256, int lineWeight = 4)
        {
            var samples = new float[clip.samples];
            clip.GetData(samples, 0);
            var channels = clip.channels;
            return GenerateWaveTexture(samples, channels, width, height, lineWeight);
        }
        public static Texture2D GenerateWaveTexture(float[] samples, int channels, int width = 1024, int height = 256, int lineWeight = 4)
        {
            var texture = new Texture2D(width, height);
            Color32[] c = new Color32[width * height];
            Array.Fill(c, Color.clear);
            texture.SetPixels32(c);
            DrawWave(texture, samples, channels, lineWeight);
            return texture;
        }
        public static void DrawWave(Texture2D texture, float[] samples, int channels, int lineWeight = 1)
        {
            // 每过1px宽度要跳过多少采样。
            float drawRate = (samples.Length / channels) / (float)texture.width;
            // 一个通道所占用的贴图高度。
            int channelHeight = texture.height / channels;
            int halfChannelHeight = channelHeight / 2;
            for (int channel = 0; channel < channels; channel++)
            {
                int heightOffset = channel * channelHeight;
                int centerY = heightOffset + halfChannelHeight;
                int lastY = centerY;
                for (int x = 0; x < texture.width; x++)
                {
                    int sampleIndex = Mathf.FloorToInt(x * drawRate) * channels + channel;
                    float sample = samples[sampleIndex];

                    int sampleHeight = -Mathf.FloorToInt(halfChannelHeight * sample);
                    int y = centerY + sampleHeight;
                    int startY = y - lineWeight / 2;
                    int minY = Mathf.Clamp(Math.Min(lastY, startY), heightOffset, heightOffset + channelHeight - 1);
                    int maxY = Mathf.Clamp(Math.Max(lastY, startY) + lineWeight, heightOffset, heightOffset + channelHeight - 1);
                    int lineHeight = maxY - minY;
                    var colors = new Color32[lineHeight];
                    Array.Fill(colors, new Color32(255, 255, 255, 255));
                    texture.SetPixels32(x, minY, 1, lineHeight, colors);
                    lastY = y;
                }
            }
            texture.Apply();
        }
        #endregion

        #endregion

        #region 私有方法

        #endregion

        #region 属性字段
        #endregion
    }
}
