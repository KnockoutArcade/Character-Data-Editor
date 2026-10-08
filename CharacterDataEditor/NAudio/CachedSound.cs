// This class was taken from https://markheath.net/post/fire-and-forget-audio-playback-with to use sound effects

using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace CharacterDataEditor.NAudio;

public class CachedSound
{
    public string filePath;
    public float[] AudioData { get; private set; }
    public WaveFormat WaveFormat { get; private set; }
    public CachedSound(string audioFileName)
    {
        filePath = audioFileName;

        using var audioFileReader = new AudioFileReader(audioFileName);

        // TODO: could add resampling in here if required
        WaveFormat = audioFileReader.WaveFormat;
        var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
        var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
        int samplesRead;
        while ((samplesRead = audioFileReader.Read(readBuffer)) > 0)
        {
            wholeFile.AddRange(readBuffer.Take(samplesRead));
        }
        AudioData = [.. wholeFile];
    }
}
