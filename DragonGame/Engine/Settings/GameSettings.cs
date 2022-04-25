#region

using System;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Settings;

public class GameSettings
{
    private int _sfxVolume;

    public GameSettings()
    {
    }

    public GameSettings(int musicVolume, int sfxVolume)
    {
        MusicVolume = musicVolume;
        SfxVolume = sfxVolume;
    }

    public int MusicVolume
    {
        get => SDL_mixer.Mix_VolumeMusic(-1);
        set => _ = SDL_mixer.Mix_VolumeMusic(Math.Clamp(value, 0, SDL_mixer.MIX_MAX_VOLUME));
    }

    public int SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = Math.Clamp(value, 0, SDL_mixer.MIX_MAX_VOLUME);
    }
}