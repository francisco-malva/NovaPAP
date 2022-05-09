#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Selector;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using SDL2;

#endregion

namespace DuckDuckJump.Game.Pausing;

public class PauseMenu : IDisposable
{
    public enum PauseAction
    {
        None,
        Resume,
        Quit,
        Reset
    }

    private readonly Font _pauseFont;
    private readonly PauseSelector _selector;

    public PauseMenu()
    {
        Paused = false;
        _pauseFont = new Font("public-pixel", 30);
        _selector = new PauseSelector(_pauseFont);
    }

    public PauseAction Action => _selector.Action;
    public bool Paused { get; private set; }

    public void Dispose()
    {
        _pauseFont.Dispose();
    }

    public void Update()
    {
        if (!Paused)
        {
            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE)) Paused = true;
        }
        else
        {
            if (Action == PauseAction.Resume)
            {
                Paused = false;
                _selector.Action = PauseAction.None;
            }
            else
            {
                _selector.Update();
            }
        }
    }

    public void Draw()
    {
        if (!Paused) return;

        Graphics.Draw(Texture.White, null,
            Matrix3x2.CreateScale(Graphics.LogicalSize.Width, Graphics.LogicalSize.Height),
            Color.FromArgb(230, 0, 0, 0));
        _selector.Draw();
    }

    private class PauseSelector : TextSelector
    {
        public PauseAction Action;

        public PauseSelector(Font font) : base(font)
        {
        }

        public override void Update()
        {
            Begin();

            Break(100.0f);
            Label("PAUSED", Color.DarkGoldenrod);
            Break(100.0f);

            if (Button("RESUME")) Action = PauseAction.Resume;

            if (Button("RESET")) Action = PauseAction.Reset;

            if (Button("QUIT")) Action = PauseAction.Quit;
            End();

            base.Update();
        }
    }
}