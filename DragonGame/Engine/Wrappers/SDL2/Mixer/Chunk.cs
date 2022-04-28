#region

using System;
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer.Exceptions;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

internal class Chunk : IDisposable
{
    public Chunk(string file)
    {
        Handle = SDL_mixer.Mix_LoadWAV(file);
        if (Handle == IntPtr.Zero)
            throw new ChunkException($"Could not load chunk. SDL Mixer Error: {SDL_mixer.Mix_GetError()}");
    }

    private IntPtr Handle { get; set; }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public int Play(int channel = -1, int loops = -1)
    {
        _ = SDL_mixer.Mix_VolumeChunk(Handle, SettingsLoader.Current.SfxVolume);
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