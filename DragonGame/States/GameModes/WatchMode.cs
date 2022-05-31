#region

using System;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Game;
using DuckDuckJump.Game.GameWork.Banner;
using DuckDuckJump.Game.Input;
using DuckDuckJump.Game.Pausing;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes;

public class WatchMode : IGameState
{
    private PauseMenu _menu;

    private AudioClip _watchModeOst;

    public void Initialize()
    {
        _menu = new PauseMenu();
        _watchModeOst = new AudioClip("watch-mode", true);

        Audio.PlayMusic(_watchModeOst);


        Match.Initialize(new GameInfo(new ComInfo(8, 8), 1024, Environment.TickCount, -1, ushort.MaxValue,
            BannerWork.MessageIndex.WatchModeStart, GameInfo.Flags.Exhibition | GameInfo.Flags.NoItems));
    }

    public void Exit()
    {
        _watchModeOst.Dispose();
        _menu.Dispose();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if (Match.State == Match.MatchState.InGame) _menu.Update();

        if (_menu.Paused)
        {
            switch (_menu.Action)
            {
                case PauseMenu.PauseAction.None:
                    break;
                case PauseMenu.PauseAction.Resume:
                    break;
                case PauseMenu.PauseAction.Quit:
                    GameFlow.Set(new MainMenuState());
                    break;
                case PauseMenu.PauseAction.Reset:
                    GameFlow.Set(new WatchMode());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;
        }

        Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];
        Match.Update(inputs);
    }

    public void Draw()
    {
        Match.Draw();
        _menu.Draw();
    }
}