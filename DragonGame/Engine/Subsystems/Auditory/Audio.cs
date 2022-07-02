#region

using System;
using System.IO;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Game.Configuration;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Auditory;

internal static class Audio
{
    private const int MixingChannelCount = 16;

    private const int ChannelCount = 2;
    private const int SampleSize = sizeof(short);

    private static readonly Channel[] Channels = new Channel[MixingChannelCount];

    private static uint _device;

    private static readonly SDL.SDL_AudioCallback AudioPollCallback = AudioCallback;

    public static float MusicFade
    {
        get => Channels[0].Volume;
        set => Channels[0].Volume = value;
    }


    public static void PlayMusic(AudioClip clip, float volume = 1.0f)
    {
        Channels[0].Assign(clip, volume);
    }

    public static void StopMusic()
    {
        Channels[0].Stop();
    }

    public static void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        var channel = GetAvailableChannel();
        channel?.Assign(clip, volume);
    }

    private static Channel GetAvailableChannel()
    {
        for (var i = 1; i < Channels.Length; i++)
            if (!Channels[i].InUse)
                return Channels[i];
        return null;
    }

    public static void Initialize()
    {
        for (var i = 0; i < Channels.Length; i++) Channels[i] = new Channel(i == 0);


        var spec = new SDL.SDL_AudioSpec
        {
            callback = AudioPollCallback,
            userdata = IntPtr.Zero,
            format = SDL.AUDIO_S16,
            samples = 400,
            freq = 48000,
            channels = ChannelCount
        };

        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) != 0)
            return;

        _device = SDL.SDL_OpenAudioDevice(IntPtr.Zero, 0, ref spec, out var obtained, 0);

        if (_device == 0)
            return;


        SDL.SDL_PauseAudioDevice(_device, 0);
    }

    private static void AudioCallback(IntPtr userdata, IntPtr stream, int len)
    {
        unsafe
        {
            var span = new Span<short>((void*)stream, len / SampleSize);

            for (var i = 0; i < span.Length; i++) span[i] = 0;
            for (var i = 0; i < Channels.Length; i++)
                Channels[i].Mix(span,
                    i == 0
                        ? Settings.MyData.MusicVolume
                        : Settings.MyData.SfxVolume);
        }
    }

    public static void Quit()
    {
        SDL.SDL_CloseAudioDevice(_device);
    }

    private class Channel
    {
        public readonly bool Loop;
        private int _offset;
        public AudioClip Clip;
        public bool InUse;
        public float Volume;

        public Channel(bool loop)
        {
            InUse = false;
            Loop = loop;
            Clip = null;
        }

        public void Assign(AudioClip clip, float volume)
        {
            InUse = true;
            _offset = 0;
            Clip = clip;
            Volume = Math.Clamp(volume, 0.0f, 1.0f);
        }

        public void Stop()
        {
            InUse = false;
            Clip = null;
        }

        public void Mix(Span<short> outputBuffer, float mixingVolume)
        {
            unsafe
            {
                if (!InUse || Clip == null)
                    return;

                if (Clip.Disposed)
                {
                    Stop();
                    return;
                }

                Span<short> buffer = stackalloc short[outputBuffer.Length];

                fixed (short* ptr = buffer)
                {
                    int bytesRead;
                    var castedBuffer = new Span<byte>(ptr, buffer.Length * SampleSize);

                    Clip.Stream.Seek(_offset, SeekOrigin.Begin);

                    if ((bytesRead = Clip.Stream.Read(castedBuffer)) == 0)
                    {
                        if (Loop)
                            _offset = 0;
                        else
                            InUse = false;
                    }
                    else
                    {
                        for (var i = 0; i < buffer.Length; i++)
                            outputBuffer[i] += (short)(buffer[i] * Volume * mixingVolume);
                    }

                    _offset += bytesRead;
                }
            }
        }
    }
}