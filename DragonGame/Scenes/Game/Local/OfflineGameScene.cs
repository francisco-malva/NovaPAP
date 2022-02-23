using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Scenes.Game.Input;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Local;

internal sealed class OfflineGameScene : PausableGameScene
{
    private GameInput _p1CurrentInput;
    private GameInput _p2CurrentInput;

    public OfflineGameScene(GameInfo info) : base(info)
    {
        CurrentReplay = new Replay(info);
    }

    protected override void RunGame()
    {
        ProcessInputs();
        SimulateAndDraw(_p1CurrentInput, _p2CurrentInput);
        CurrentReplay.Enqueue(new Pair<GameInput>(_p1CurrentInput, _p2CurrentInput));
    }


    private void ProcessInputs()
    {
        if (!P1Field.AiControlled)
            ProcessInput(ref _p1CurrentInput, SDL.SDL_Scancode.SDL_SCANCODE_A,
                SDL.SDL_Scancode.SDL_SCANCODE_D, SDL.SDL_Scancode.SDL_SCANCODE_S);
        if (!P2Field.AiControlled)
            ProcessInput(ref _p2CurrentInput, SDL.SDL_Scancode.SDL_SCANCODE_J,
                SDL.SDL_Scancode.SDL_SCANCODE_L, SDL.SDL_Scancode.SDL_SCANCODE_K);
    }
}