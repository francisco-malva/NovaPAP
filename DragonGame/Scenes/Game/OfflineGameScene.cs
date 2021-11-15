using System.IO;
using DragonGame.Engine.Input;
using DragonGame.Scenes.Game.Gameplay;
using DragonGame.Scenes.Game.Input;
using SDL2;

namespace DragonGame.Scenes.Game
{
    internal class OfflineGameScene : GameScene
    {
        private readonly Replay.Replay _replay;
        private GameInput _p1PreviousInput, _p1CurrentInput;
        private GameInput _p2PreviousInput, _p2CurrentInput;

        public OfflineGameScene(byte roundsToWin, bool p1Ai, bool p2Ai, AiDifficulty difficulty) : base(roundsToWin, p1Ai, p2Ai, difficulty)
        {
            _replay = new Replay.Replay(this, Random);
        }

        protected override void RunFrame()
        {
            ProcessInputs();
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

        private void ProcessInput(ref GameInput previousInput, ref GameInput nextInput, SDL.SDL_Scancode leftScanCode,
            SDL.SDL_Scancode rightScanCode, SDL.SDL_Scancode specialScanCode)
        {
            previousInput = nextInput;
            unsafe
            {
                nextInput = GameInput.None;

                nextInput |= Keyboard.KeyDown(leftScanCode) ? GameInput.Left : GameInput.None;
                nextInput |= Keyboard.KeyDown(rightScanCode) ? GameInput.Right : GameInput.None;
                nextInput |= Keyboard.KeyDown(specialScanCode) ? GameInput.Special : GameInput.None;
            }

            if (nextInput != previousInput) _replay.RegisterInput(FrameCount, _p1CurrentInput, _p2CurrentInput);
        }

        protected override void OnGameEnd()
        {
            using var writer = new BinaryWriter(File.OpenWrite("replay.rpy"));
            _replay.Write(writer);
        }
    }
}