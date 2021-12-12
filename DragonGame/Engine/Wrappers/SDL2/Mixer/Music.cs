using System;
using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

internal class Music : IDisposable
{
    private static int _volume = SDL_mixer.MIX_MAX_VOLUME;

    public Music(string file)
    {
        Handle = SDL_mixer.Mix_LoadMUS(file);
    }

    public IntPtr Handle { get; private set; }

    public static int Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            _ = SDL_mixer.Mix_VolumeMusic(_volume);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public static void Resume()
    {
        SDL_mixer.Mix_ResumeMusic();
    }

    public static void Pause()
    {
        SDL_mixer.Mix_PauseMusic();
    }

    public static int Halt()
    {
        return SDL_mixer.Mix_HaltMusic();
    }

    public bool Play(int loops = -1)
    {
        return SDL_mixer.Mix_PlayMusic(Handle, loops) == 0;
    }

    private void ReleaseUnmanagedResources()
    {
        SDL_mixer.Mix_FreeMusic(Handle);
        Handle = IntPtr.Zero;
    }

    ~Music()
    {
        ReleaseUnmanagedResources();
    }
}