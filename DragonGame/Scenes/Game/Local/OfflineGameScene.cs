using System;
using DragonGame.Engine.Input;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;
using SDL2;

namespace DragonGame.Scenes.Game.Local
{
    internal sealed class OfflineGameScene : GameScene
    {
        private GameInput _p1PreviousInput, _p1CurrentInput;
        private GameInput _p2PreviousInput, _p2CurrentInput;

        public OfflineGameScene(byte roundsToWin, bool p1Ai, bool p2Ai, AiDifficulty difficulty) : base(roundsToWin,
            p1Ai, p2Ai, difficulty)
        {
        }

        public override void OnTick()
        {
            ProcessInputs();

            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_P))
            {
                P1Field.Player.Position = new Point(0, int.MaxValue - 100);
                P2Field.Player.Position = new Point(0, int.MaxValue - 100);
            }
            SimulateFrame(_p1CurrentInput, _p2CurrentInput);
            Draw();

            base.OnTick();
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

        protected override void OnStart()
        {
            Random.Setup(Environment.TickCount);
            base.OnStart();
        }

        protected override void OnGameEnd()
        {
        }
    }
}