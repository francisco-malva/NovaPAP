using System;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Text;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuResources : IDisposable
{
    private const string FontName = "PublicPixel-0W6DP";

    private static readonly MenuOption[] Options =
    {
        new(" ", Color.White),
        new("DUCK DUCK JUMP!", Color.Yellow),
        new("START", Color.White),
        new("OPTIONS", Color.White),
        new("QUIT", Color.White),
        new("ARE YOU SURE?", Color.Red),
        new("YES...", Color.White),
        new("NO!", Color.White),
        new("MODE SELECTION.", Color.Aqua),
        new("SINGLE MATCH", Color.White),
        new("TIME ATTACK", Color.White),
        new("SURVIVAL", Color.White),
        new("WATCH MODE", Color.White),
        new("SELECT HOW YOU WANT TO PLAY A MATCH.", Color.Aqua),
        new("AGAINST THE CPU", Color.White),
        new("AGAINST ANOTHER HUMAN", Color.White),
        new("ONLINE", Color.White),
        new("BACK", Color.White),
        new("SELECT THE SETTINGS YOU'D LIKE TO CHANGE", Color.Gray),
        new("CONTROLLER SETTINGS", Color.White),
        new("SOUND SETTINGS", Color.White),
        new("LEADERBOARD", Color.White),
        new("REPLAYS", Color.White),
        new("STATISTICS", Color.White)
    };

    public readonly TextDrawer TextDrawer;

    public MainMenuResources(ResourceManager manager)
    {
        Debug.Assert(GameContext.Instance != null, "Engine.GameContext.Instance != null");
        var renderer = GameContext.Instance.Renderer;
        var font = manager.Fonts[FontName];

        TextDrawer = new TextDrawer(font, renderer, 32, "ABCDEFGHIJKLMNOPQRSTUVWXYZ!.? ");
    }

    public void Dispose()
    {
        TextDrawer.Dispose();
    }

    public static MenuOption GetOption(MenuOptionType optionType)
    {
        return Options[(int) optionType];
    }
}

internal readonly struct MenuOption
{
    public readonly string Caption;
    public readonly Color Color;

    public MenuOption(string caption, Color color)
    {
        Caption = caption;
        Color = color;
    }
}