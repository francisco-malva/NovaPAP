using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Mixer;

internal readonly struct Channel
{
    public readonly int Handle;

    public Channel(int handle)
    {
        Handle = handle;
    }

    public void Resume()
    {
        SDL_mixer.Mix_Resume(Handle);
    }

    public void Pause()
    {
        SDL_mixer.Mix_Pause(Handle);
    }
}