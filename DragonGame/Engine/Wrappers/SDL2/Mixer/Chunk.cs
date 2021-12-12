using System;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

internal class Chunk : IDisposable
{
    private int _volume = SDL_mixer.MIX_MAX_VOLUME;

    public Chunk(string file)
    {
        Handle = SDL_mixer.Mix_LoadWAV(file);
    }

    public IntPtr Handle { get; private set; }

    public int Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            _ = SDL_mixer.Mix_VolumeChunk(Handle, _volume);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public int Play(int channel = -1, int loops = -1)
    {
        return SDL_mixer.Mix_PlayChannel(channel, Handle, loops);
    }

    private void ReleaseUnmanagedResources()
    {
        SDL_mixer.Mix_FreeChunk(Handle);
        Handle = IntPtr.Zero;
    }

    ~Chunk()
    {
        ReleaseUnmanagedResources();
    }
}