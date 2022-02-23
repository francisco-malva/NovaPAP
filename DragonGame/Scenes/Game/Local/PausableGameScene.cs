using DuckDuckJump.Engine.Input;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Local;

internal abstract class PausableGameScene : GameScene
{
    private bool _paused;

    protected PausableGameScene(GameInfo info) : base(info)
    {
    }

    public override void OnTick()
    {
        if (CanPause && !_paused && Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE)) _paused = true;

        if (_paused)
        {
        }
        else
        {
            RunGame();
        }
    }

    protected abstract void RunGame();
}