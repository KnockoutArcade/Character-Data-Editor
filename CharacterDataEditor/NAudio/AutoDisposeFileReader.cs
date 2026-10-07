using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This class was taken from https://markheath.net/post/fire-and-forget-audio-playback-with to use sound effects


namespace CharacterDataEditor.NAudio
{
    public class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        //public int Read(float[] buffer, int offset, int count)
        public int Read(Span<float> buffer)
        {
            if (isDisposed)
            {
                return 0;
            }

            var read = reader.Read(buffer);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}
