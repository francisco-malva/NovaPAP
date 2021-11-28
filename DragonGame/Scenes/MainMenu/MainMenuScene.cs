using DragonGame.Engine.Input;
using DragonGame.Engine.Scenes;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Local;
using DragonGame.Scenes.Game.Network;
using Engine.Wrappers.SDL2;
using SDL2;

namespace DragonGame.Scenes.MainMenu
{
    internal class MainMenuScene : Scene
    {
        public override void OnTick()
        {
#if DEBUG
            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_A))
                Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, true, true, AiDifficulty.Nightmare));
            else if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_S))
                Engine.Game.Instance.SceneManager.Set(new ServerGameScene(3));
            else if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_D))
                Engine.Game.Instance.SceneManager.Set(new ClientGameScene());

            Engine.Game.Instance.Renderer.SetDrawColor(Color.Red);
            Engine.Game.Instance.Renderer.Clear();
            Engine.Game.Instance.Renderer.Present();
#else
            Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(3, false, true, AiDifficulty.Easy));
#endif
        }

        protected override void OnUnload()
        {
        }
    }
}