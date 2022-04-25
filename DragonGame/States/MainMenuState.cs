#region

using System.Drawing;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Selector;
using DuckDuckJump.Engine.Subsystems.Flow;
using SDL2;

#endregion

namespace DuckDuckJump.States;

public class MainMenuSelector : TextSelector
{
    public MainMenuSelector(Font font) : base(font)
    {
    }

    public override void Update()
    {
        base.Update();

        Begin();
        Break(30.0f);
        Label("Duck Duck Jump!", Color.White);
        Label("Duck Duck Jump!", Color.White);
        if (Button("Click me!")) GameFlow.Set(new TestState());
        End();
    }
}

public class MainMenuState : IGameState
{
    private Font _font;
    private MainMenuSelector _selector;

    public void Initialize()
    {
        _font = new Font("public-pixel", 30);
        _selector = new MainMenuSelector(_font);
    }

    public void Exit()
    {
        _font.Dispose();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        _selector.Update();
    }

    public void Draw()
    {
        _selector.Draw();
    }
}