using System;
using System.Diagnostics;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Text;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuResources : IDisposable
{
    private const string FontName = "PublicPixel-0W6DP";

    public readonly TextDrawer TextDrawer;

    public MainMenuResources(ResourceManager manager)
    {
        Debug.Assert(GameContext.Instance != null, "Engine.GameContext.Instance != null");
        var renderer = GameContext.Instance.Renderer;
        var font = manager.Fonts[FontName];

        TextDrawer = new TextDrawer(font, renderer, 32,
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-!.? ");
    }

    public void Dispose()
    {
        TextDrawer.Dispose();
    }
}