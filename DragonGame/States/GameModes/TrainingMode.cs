#region

using System;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.GameWork.Banner;
using DuckDuckJump.Game.Input;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes;

internal class TrainingMode : IGameState
{
    public void Initialize()
    {
        Match.Initialize(new GameInfo(new ComInfo(0, 8), 10, Environment.TickCount, 5, 60 * 60,
            BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None));
    }

    public void Exit()
    {
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];

        for (var i = 0; i < Match.PlayerCount; i++) inputs[i] = Settings.MyData.GetInput(i);

        Match.Update(inputs);
    }

    public void Draw()
    {
        Match.Draw();
    }
}