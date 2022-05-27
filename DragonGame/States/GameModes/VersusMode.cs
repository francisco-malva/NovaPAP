#region

using System;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.GameWork;
using DuckDuckJump.Game.Input;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes;

public class VersusMode : IGameState
{
    private AudioClip _gameMusic;

    public void Initialize()
    {
        _gameMusic = new AudioClip("gameplay", true);
        Audio.PlayMusic(_gameMusic);
        Match.Initialize(new GameInfo(new ComLevels(0, 5), 50, Environment.TickCount, 3, 99 * 60,
            BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None));
    }

    public void Exit()
    {
        _gameMusic.Dispose();
        Assets.Unload();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if (Match.IsOver)
        {
            GameFlow.Set(new MainMenuState());
            return;
        }

        Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];

        for (var i = 0; i < Match.PlayerCount; i++) inputs[i] = Settings.MyData.GetInput(i);

        Match.Update(inputs);
    }

    public void Draw()
    {
        Match.Draw();
    }
}