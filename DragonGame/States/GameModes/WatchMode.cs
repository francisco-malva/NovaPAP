#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game;
using DuckDuckJump.Game.GameWork.Banner;
using DuckDuckJump.Game.Input;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes;

public class WatchMode : IGameState
{
    private Font _captionFont;
    private Size _size;

    private float _time;
    private AudioClip _watchModeOst;

    public void Initialize()
    {
        _captionFont = new Font("terminator-two-20", 20);
        _watchModeOst = new AudioClip("watch-mode", true);

        Audio.PlayMusic(_watchModeOst);


        _size = _captionFont.MeasureString("PRESS ANY KEY TO EXIT");
        Match.Initialize(new GameInfo(new ComLevels(8, 8), 1024, Environment.TickCount, -1, ushort.MaxValue,
            BannerWork.MessageIndex.WatchModeStart, GameInfo.Flags.Exhibition));
    }

    public void Exit()
    {
        _watchModeOst.Dispose();
        _captionFont.Dispose();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if (Keyboard.AnyDown(out var _) && Match.State != Match.MatchState.BeginningMessage)
        {
            GameFlow.Set(new MainMenuState());
            return;
        }

        Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];
        Match.Update(inputs);

        if (Match.State != Match.MatchState.BeginningMessage)
            _time += GameFlow.TimeStep;
    }

    public void Draw()
    {
        var alpha = 1.0f - (MathF.Cos(_time * 1.5f) + 1.0f) * 0.5f;
        Match.Draw();

        if (Match.State == Match.MatchState.BeginningMessage)
            return;

        _captionFont.Draw("PRESS ANY KEY TO EXIT",
            Matrix3x2.CreateTranslation(Graphics.Midpoint.X - _size.Width / 2.0f,
                Graphics.LogicalSize.Height - _size.Height - 10.0f),
            Color.FromArgb((int)(alpha * byte.MaxValue), Color.LightGray.R, Color.LightGray.G, Color.LightGray.B));
    }
}