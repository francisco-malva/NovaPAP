using DuckDuckJump.Engine.GUI;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay;
using DuckDuckJump.Scenes.Game.Input;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Local;

internal sealed class OfflineGameScene : PausableGameScene
{

    private GameInput _p1CurrentInput;
    private GameInput _p2CurrentInput;
    private GameInfo _info;

    public OfflineGameScene(GameInfo info) : base(info)
    {
        CurrentReplay = new Replay(info);
        _info = info;
    }

    protected override void OnReset()
    {
        Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(_info));
    }

    protected override void RunGame()
    {
        ProcessInputs();
        SimulateAndDraw(_p1CurrentInput, _p2CurrentInput);
        CurrentReplay.Enqueue(new Engine.Utilities.Pair<GameInput>(_p1CurrentInput, _p2CurrentInput));
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

    protected override void OnGameEnd()
    {
        base.OnGameEnd();
    }
}