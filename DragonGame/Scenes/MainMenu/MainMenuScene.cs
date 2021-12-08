using DuckDuckJump.Engine.GUI;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;
using DuckDuckJump.Scenes.Game.Local;
using DuckDuckJump.Scenes.Game.Network;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuScene : Scene
{
    private readonly Selection[] _mainSelections;

    private readonly Selection[] _networkGamesSelection;

    private readonly Selection[] _quitYouSureSelection;
    private readonly Selector _selector;

    public MainMenuScene()
    {
        _selector = new Selector();

        _mainSelections = new[]
        {
            new("DUCK DUCK JUMP", null, null, false),
            new Selection("VS CPU",
                () =>
                {
                    Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, false, true, AiDifficulty.Nightmare));
                }, null),
            new Selection("VS PLAYER",
                () =>
                {
                    Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, false, false,
                        AiDifficulty.Nightmare));
                }, null),
            new Selection("NETWORK", () => { _selector.Push(_networkGamesSelection); }, null),
            new Selection("QUIT", ShowQuitPrompt, null)
        };

        _networkGamesSelection = new[]
        {
            new("NETWORK GAME", null, null, false),
            new Selection("HOST", () => { Engine.Game.Instance.SceneManager.Set(new ServerGameScene(3)); }, null),
            new Selection("JOIN", () => { Engine.Game.Instance.SceneManager.Set(new ClientGameScene()); }, null),
            new Selection("RETURN", () => { _selector.Pop(); }, null)
        };

        _quitYouSureSelection = new[]
        {
            new("ARE YOU SURE YOU WANT TO QUIT?", null, null, false),
            new Selection("YES!", () => { Engine.Game.Instance.Exit(); }, null),
            new Selection("NO!", () => { _selector.Pop(); }, null)
        };
        _selector.Push(_mainSelections);
    }

    public override void OnTick()
    {
        _selector.Tick();

        Engine.Game.Instance.Renderer.SetDrawColor(Color.Black);
        Engine.Game.Instance.Renderer.Clear();
        _selector.Draw();
        Engine.Game.Instance.Renderer.Present();
    }

    protected override void OnUnload()
    {
    }

    private void ShowQuitPrompt()
    {
        _selector.Push(_quitYouSureSelection);
    }
}