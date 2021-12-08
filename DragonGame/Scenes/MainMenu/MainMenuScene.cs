using DuckDuckJump.Engine.GUI;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Local;

namespace DuckDuckJump.Scenes.MainMenu
{
    internal class MainMenuScene : Scene
    {
        private readonly Selector _selector;

        private readonly Selection[] _mainSelections;

        private readonly Selection[] _quitYouSureSelection;

        public MainMenuScene()
        {
            _selector = new Selector();

            _mainSelections = new Selection[]
            {
                new Selection("DUCK DUCK JUMP",null,null,false),
                new Selection("VS CPU", () => { Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, false, true, Game.Gameplay.Players.AI.AiDifficulty.Nightmare)); }, null),
                new Selection("VS PLAYER", () => { Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, false, false, Game.Gameplay.Players.AI.AiDifficulty.Nightmare)); }, null),
                new Selection("QUIT", ShowQuitPrompt, null)
            };
            _quitYouSureSelection = new Selection[]
            {
                new Selection("ARE YOU SURE YOU WANT TO QUIT?",null,null,false),
                new Selection("YES!", () => { Engine.Game.Instance.Exit(); }, null),
                new Selection("NO!", () => {_selector.Pop(); }, null)
            };
            _selector.Push(new SelectionGroup(_mainSelections));
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
            _selector.Push(new SelectionGroup(_quitYouSureSelection));
        }
    }
}