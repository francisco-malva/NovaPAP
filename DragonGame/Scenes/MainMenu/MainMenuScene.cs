using System.IO;
using DragonGame.Engine.Input;
using DragonGame.Engine.Scenes;
using DragonGame.Scenes.Game;
using DragonGame.Scenes.Game.Gameplay;
using DragonGame.Scenes.Game.Replay;

namespace DragonGame.Scenes.MainMenu
{
    internal class MainMenuScene : Scene
    {
        public override void OnTick()
        {
            if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_A))
            {
                Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, false, true, AiDifficulty.Easy));
            }
            else if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_S))
            {
                using var reader = new BinaryReader(File.OpenRead("replay.rpy"));
                Engine.Game.Instance.SceneManager.Set(new ReplayGameScene(new Replay(reader)));
            }
            Engine.Game.Instance.Renderer.SetDrawColor(255, 0, 0, 255);
            Engine.Game.Instance.Renderer.Clear();
            Engine.Game.Instance.Renderer.Present();
        }

        protected override void OnUnload()
        {
        }
    }
}