using System;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Scenes.Game;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;
using DuckDuckJump.Scenes.Game.Local;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuScene : Scene
{
    public override void OnTick()
    {
        Engine.Game.Instance?.SceneManager.Set(new OfflineGameScene(new GameInfo(50, 3, false, true,
            Environment.TickCount, AiDifficulty.Normal, true)));
    }

    protected override void OnUnload()
    {
    }
}