using System;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Input;
using DragonGame.Scenes.MainMenu;
using SDL2;

namespace DragonGame.Scenes.Game.Network
{
    internal class OnlineGameScene : GameScene
    {
        protected InOutStream Stream;
        private readonly bool _onLeftSide;

        public OnlineGameScene(byte roundsToWin, bool onLeftSide) : base(roundsToWin)
        {
            _onLeftSide = onLeftSide;
        }

        public override void OnTick()
        {
            try
            {
                var gameInput = GameInput.None;

                ProcessInput(ref gameInput, SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
                    SDL.SDL_Scancode.SDL_SCANCODE_S);

                Stream.Writer.Write((byte)gameInput);
                var foreignInput = (GameInput)Stream.Reader.ReadByte();

                SimulateAndDraw(_onLeftSide ? gameInput : foreignInput, _onLeftSide ? foreignInput : gameInput);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Engine.Game.Instance.SceneManager.Set(new MainMenuScene());
            }
        }

        protected override void OnGameEnd()
        {
            Stream.Dispose();
        }

        
    }
}
