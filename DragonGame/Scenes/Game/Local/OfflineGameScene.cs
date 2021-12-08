using DuckDuckJump.Engine.GUI;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;
using DuckDuckJump.Scenes.Game.Input;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;
using System;

namespace DuckDuckJump.Scenes.Game.Local
{
    internal sealed class OfflineGameScene : GameScene
    {
        private Selector _pauseSelector = new Selector();

        private GameInput _p1CurrentInput;
        private GameInput _p2CurrentInput;

        private bool _paused = false;

        private readonly Selection[] _pauseSelection;
        private readonly Selection[] _quitYouSureSelection;

        public OfflineGameScene(byte roundsToWin, bool p1Ai, bool p2Ai, AiDifficulty difficulty) : base(roundsToWin,
            p1Ai, p2Ai, difficulty)
        {
            Random.Setup(Environment.TickCount);

            ChangeState(GameState.GetReady);

            _pauseSelection = new Selection[]
            {
                new Selection("PAUSED", null, null, false),
                new Selection("RESUME", () => { _paused = false; }, null),
                new Selection("RESET", () => { Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(roundsToWin,p1Ai,p2Ai,difficulty)); }, null),
                new Selection("QUIT", () => { _pauseSelector.Push(new SelectionGroup(_quitYouSureSelection)); }, null),
            };

            _quitYouSureSelection = new Selection[]
            {
                new Selection("ARE YOU SURE YOU WANT TO QUIT?",null,null,false),
                new Selection("YES!", () => { Engine.Game.Instance.SceneManager.Set(new MainMenuScene()); }, null),
                new Selection("NO!", () => {_pauseSelector.Pop(); }, null)
            };

            _pauseSelector.Push(new SelectionGroup(_pauseSelection));
        }

        public override void OnTick()
        {
            if (CanPause && !_paused && Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE))
            {
                _paused = true;
            }

            if (_paused)
            {
                _pauseSelector.Tick();
                Draw((renderer) =>
                {
                    renderer.SetDrawColor(new Engine.Wrappers.SDL2.Color(0, 0, 0, 0));
                    renderer.Clear();
                    _pauseSelector.Draw();
                });
            }
            else
            {
                ProcessInputs();

                SimulateAndDraw(_p1CurrentInput, _p2CurrentInput);
            }
            
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
}