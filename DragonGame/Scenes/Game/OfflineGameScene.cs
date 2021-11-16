using DragonGame.Engine.Input;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Input;
using DragonGame.Wrappers;
using SDL2;

namespace DragonGame.Scenes.Game
{
    internal sealed class OfflineGameScene : GameScene
    {
        private GameInput _p1PreviousInput, _p1CurrentInput;
        private GameInput _p2PreviousInput, _p2CurrentInput;

        public OfflineGameScene(byte roundsToWin, bool p1Ai, bool p2Ai, AiDifficulty difficulty) : base(roundsToWin,
            p1Ai, p2Ai, difficulty)
        {
        }

        protected override void RunFrame()
        {
            ProcessInputs();

            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_P))
            {
                P1Field.Player.Position = new Point(0, int.MaxValue - 100);
                P2Field.Player.Position = new Point(0, int.MaxValue - 100);
            }
            Update(_p1CurrentInput, _p2CurrentInput);
            Draw();
        }

        private void ProcessInputs()
        {
            if (!P1Field.AiControlled)
                ProcessInput(ref _p1PreviousInput, ref _p1CurrentInput, SDL.SDL_Scancode.SDL_SCANCODE_A,
                    SDL.SDL_Scancode.SDL_SCANCODE_D, SDL.SDL_Scancode.SDL_SCANCODE_S);
            if (!P2Field.AiControlled)
                ProcessInput(ref _p2PreviousInput, ref _p2CurrentInput, SDL.SDL_Scancode.SDL_SCANCODE_J,
                    SDL.SDL_Scancode.SDL_SCANCODE_L, SDL.SDL_Scancode.SDL_SCANCODE_K);
        }

        private static void ProcessInput(ref GameInput previousInput, ref GameInput nextInput,
            SDL.SDL_Scancode leftScanCode,
            SDL.SDL_Scancode rightScanCode, SDL.SDL_Scancode specialScanCode)
        {
            previousInput = nextInput;
            nextInput = GameInput.None;

            nextInput |= Keyboard.KeyDown(leftScanCode) ? GameInput.Left : GameInput.None;
            nextInput |= Keyboard.KeyDown(rightScanCode) ? GameInput.Right : GameInput.None;
            nextInput |= Keyboard.KeyDown(specialScanCode) ? GameInput.Special : GameInput.None;
        }

        protected override void OnGameEnd()
        {
        }
    }
}