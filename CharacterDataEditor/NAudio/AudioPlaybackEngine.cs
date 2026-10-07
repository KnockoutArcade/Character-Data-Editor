// This class was taken from https://markheath.net/post/fire-and-forget-audio-playback-with to use sound effects

using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CharacterDataEditor.NAudio;

public class AudioPlaybackEngine : IDisposable
{
    private readonly IWavePlayer outputDevice;
    private readonly MixingSampleProvider mixer;

    public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
    {
        outputDevice = new WaveOut();

        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount))
        {
            ReadFully = true
        };
        outputDevice.Init(mixer);
        outputDevice.Play();
    }

    public void PlaySound(string fileName)
    {
        var input = new AudioFileReader(fileName);
        AddMixerInput(new AutoDisposeFileReader(input));
    }

    private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
    {
        if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
        {
            return input;
        }
        if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
        {
            return new MonoToStereoSampleProvider(input);
        }
        throw new NotImplementedException("Not yet implemented this channel count conversion");
    }

    public void PlaySound(CachedSound sound)
    {
        AddMixerInput(new CachedSoundSampleProvider(sound));
    }

    private void AddMixerInput(ISampleProvider input)
    {
        mixer.AddMixerInput(ConvertToRightChannelCount(input));
    }

    public void Dispose()
    {
        outputDevice.Dispose();
    }

    public static readonly AudioPlaybackEngine Instance = new(44100, 2);
}
