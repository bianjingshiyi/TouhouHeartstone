using System;

namespace Libopus
{
    public class Decoder : IDisposable
    {
        IntPtr decoder { get; set; }
        int channels { get; }
        float[] softClipMem { get; }

        public Decoder(SamplingRate fs, int channels)
        {
            this.channels = channels;
            this.softClipMem = new float[channels];

            ErrorCode err = ErrorCode.OK;
            decoder = LibraryWrapper.opus_decoder_create(fs, channels, ref err);
            if (err != ErrorCode.OK || decoder == IntPtr.Zero)
            {
                throw new LibopusException(err);
            }
        }

        /// <summary>
        /// Decode signals
        /// </summary>
        /// <param name="data">Input payload</param>
        /// <param name="pcm">Output signal</param>
        /// <param name="decodeFec"> to request that any in-band forward error correction data be decoded. If no such data is available, the frame is decoded as if it were lost.</param>
        /// <returns></returns>
        public int Decode(byte[] data, short[] pcm, bool decodeFec = false)
        {
            return LibraryWrapper.opus_decode(decoder, data, data.Length, pcm, pcm.Length / channels, decodeFec ? 1 : 0);
        }

        public int Decode(byte[] data, float[] pcm, bool decodeFec = false)
        {
            var len = LibraryWrapper.opus_decode_float(decoder, data, data.Length, pcm, pcm.Length / channels, decodeFec ? 1 : 0);
            LibraryWrapper.opus_pcm_soft_clip(pcm, len / channels, channels, softClipMem);
            return len;
        }

        public void Dispose()
        {
            if (decoder != IntPtr.Zero)
            {
                LibraryWrapper.opus_decoder_destroy(decoder);
                decoder = IntPtr.Zero;
            }
        }

        ~Decoder()
        {
            Dispose();
        }
    }
}
