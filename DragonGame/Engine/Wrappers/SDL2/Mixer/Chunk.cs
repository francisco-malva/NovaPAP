using System;
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer.Exceptions;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

internal class Chunk : IDisposable
{
    private int _volume = SDL_mixer.MIX_MAX_VOLUME;

    public Chunk(string file)
    {
        Handle = SDL_mixer.Mix_LoadWAV(file);
    }

    private IntPtr Handle { get; set; }

    public int Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (SDL_mixer.Mix_VolumeChunk(Handle, _volume) != 0)
                throw new ChunkException(
                    $"Could not set the chunk's volume. SDL Mixer Error: {SDL_mixer.Mix_GetError()}");
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