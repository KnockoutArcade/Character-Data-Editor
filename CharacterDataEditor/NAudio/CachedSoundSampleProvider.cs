// This class was taken from https://markheath.net/post/fire-and-forget-audio-playback-with to use sound effects

using System;
using NAudio.Wave;

namespace CharacterDataEditor.NAudio;

public class CachedSoundSampleProvider : ISampleProvider
{
    private readonly CachedSound cachedSound;
    private long position;

    public CachedSoundSampleProvider(CachedSound cachedSound)
    {
        this.cachedSound = cachedSound;
    }

    public int Read(Span<float> buffer)
    {
        var availableSamples = cachedSound.AudioData.Length - position;
        var samplesToCopy = (int)Math.Min(availableSamples, buffer.Length);

        cachedSound.AudioData.AsSpan((int)position, samplesToCopy).CopyTo(buffer);

        position += samplesToCopy;
        return samplesToCopy;
    }

    public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
}
