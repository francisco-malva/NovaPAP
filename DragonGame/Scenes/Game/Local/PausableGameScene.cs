using DuckDuckJump.Engine;
using DuckDuckJump.Engine.GUI;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game;
using DuckDuckJump.Scenes.Game.Gameplay;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

internal abstract class PausableGameScene : GameScene
{
    private readonly Selection[] _pauseSelection;
    private readonly Selector _pauseSelector = new();
    private readonly Selection[] _quitYouSureSelection;
    private bool _paused;

    public PausableGameScene(GameInfo info) : base(info)
    {
        _pauseSelection = new[]
        {
            new Selection("PAUSED", null, null, null, false),
            new Selection("RESUME", null, (sel) => { _paused = false; }, null),
            new Selection("RESET", null,
                OnReset, null),
            new Selection("QUIT", null, (sel) => { _pauseSelector.Push(_quitYouSureSelection); }, null)
        };

        _quitYouSureSelection = new[]
        {
            new Selection("ARE YOU SURE YOU WANT TO QUIT?", null, null, null, false),
            new Selection("YES!", null, (sel) => { Game.Instance.SceneManager.Set(new MainMenuScene()); }, null),
            new Selection("NO!", null, (sel) => { _pauseSelector.Pop(); }, null)
        };

        _pauseSelector.Push(_pauseSelection);
    }

    protected abstract void OnReset(Selection selection);

    public override void OnTick()
    {
        if (CanPause && !_paused && Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE)) _paused = true;

        if (_paused)
        {
            _pauseSelector.Tick();
            Draw(renderer =>
            {
                renderer.SetDrawColor(new Color(0, 0, 0, 0));
                renderer.Clear();
                _pauseSelector.Draw();
            });
        }
        else
        {
            RunGame();
        }
    }

    protected abstract void RunGame();
}