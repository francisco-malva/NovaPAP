#region

using System;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Input;
using SDL2;

#endregion

namespace DuckDuckJump.States;

public class TestState : IGameState
{
    public void Initialize()
    {
        Match.Assets.Load();
        Match.Initialize(new GameInfo(new ComLevels(0, 1), 100, Environment.TickCount, 0, true, 99 * 60));
    }

    public void Exit()
    {
        Match.Assets.Unload();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];

        for (var i = 0; i < Match.PlayerCount; i++)
        {
            ref var input = ref inputs[i];

            input = GameInput.None;

            if (Keyboard.KeyHeld(SDL.SDL_Scancode.SDL_SCANCODE_A))
                input |= GameInput.Left;
            if (Keyboard.KeyHeld(SDL.SDL_Scancode.SDL_SCANCODE_D))
                input |= GameInput.Right;
        }

        Match.Update(inputs);
    }

    public void Draw()
    {
        Match.Draw();
    }
}