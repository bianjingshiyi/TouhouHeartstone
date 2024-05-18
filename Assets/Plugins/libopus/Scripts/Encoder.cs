using System;

namespace Libopus
{
    public class Encoder : IDisposable
    {
        IntPtr encoder { get; set; }

        int channels { get; }

        public Encoder(SamplingRate fs, int channels, Application application) 
        {
            this.channels = channels;

            ErrorCode err = ErrorCode.OK;
            encoder = LibraryWrapper.opus_encoder_create(fs, channels, application, ref err);
            if (err != ErrorCode.OK || encoder == IntPtr.Zero)
            {
                throw new LibopusException(err);
            }
        }

        private int bitrate;

        public int Bitrate
        {
            get { return bitrate; }
            set { bitrate = value; LibraryWrapper.opus_set_bitrate(encoder, value); }
        }

        private int complexity;

        public int Complexity
        {
            get { return complexity; }
            set { complexity = value; LibraryWrapper.opus_set_complexity(encoder, value); }
        }

        private Signal signal;

        public Signal Signal
        {
            get { return signal; }
            set { signal = value; LibraryWrapper.opus_set_signal(encoder, value); }
        }

        public int Encode(float[] pcm, byte[] output)
        {
            return LibraryWrapper.opus_encode_float(encoder, pcm, pcm.Length / channels, output, output.Length);
        }

        public int Encode(short[] pcm, byte[] output)
        {
            return LibraryWrapper.opus_encode(encoder, pcm, pcm.Length / channels, output, output.Length);
        }

        public void Dispose()
        {
            if (encoder != IntPtr.Zero)
            {
                LibraryWrapper.opus_encoder_destroy(encoder);
                encoder = IntPtr.Zero;
            }
        }

        ~Encoder()
        {
            Dispose();
        }
    }
}
