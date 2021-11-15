using DragonGame.Engine.Input;
using DragonGame.Engine.Scenes;
using DragonGame.Scenes.Game;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using SDL2;

namespace DragonGame.Scenes.MainMenu
{
    internal class MainMenuScene : Scene
    {
        public override void OnTick()
        {
            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_A))
                Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, false, true, AiDifficulty.Easy));

            Engine.Game.Instance.Renderer.SetDrawColor(255, 0, 0, 255);
            Engine.Game.Instance.Renderer.Clear();
            Engine.Game.Instance.Renderer.Present();
        }

        protected override void OnUnload()
        {
        }
    }
}