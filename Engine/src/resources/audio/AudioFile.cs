using System;

using OpenTK.Audio.OpenAL;

namespace Toast.Engine.Resources.Audio;

public class AudioFile : IDisposable
{
    public string Filepath;
    public float Volume;
    public bool Repeats;

    public byte[] Data;

    public int NumChannels;
    public int BitsPerSample;
    public int SampleRate;

    public int Buffer;
    public int Source;
    public int State;

    public void Dispose()
    {
        
    }
}