using System;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuResources : IDisposable
{
    private const string FontName = "VeraMono";

    private static readonly MenuOption[] Options =
    {
        new(32, " ", Color.White),
        new(64, "DUCK DUCK JUMP!", Color.Yellow),
        new(32, "START", Color.White),
        new(32, "OPTIONS", Color.White),
        new(32, "QUIT", Color.White)
    };

    private readonly Texture[] _optionTextures;
    private readonly TextureInfo[] _optionTexturesInfo;

    public MainMenuResources(ResourceManager manager)
    {
        Debug.Assert(GameContext.Instance != null, "Engine.GameContext.Instance != null");
        var renderer = GameContext.Instance.Renderer;
        var font = manager.Fonts[FontName];

        _optionTextures = new Texture[Options.Length];
        _optionTexturesInfo = new TextureInfo[Options.Length];

        for (var i = 0; i < _optionTextures.Length; i++)
        {
            font.Size = Options[i].Size;
            using var surface = font.RenderTextBlended(Options[i].Caption, Options[i].Color);
            _optionTextures[i] = new Texture(renderer, surface);
            _optionTexturesInfo[i] = _optionTextures[i].QueryTexture();
        }
    }

    public void Dispose()
    {
        foreach (var texture in _optionTextures) texture.Dispose();
    }

    public Texture GetOptionTexture(MenuOptionType option)
    {
        return _optionTextures[(int) option];
    }

    public TextureInfo GetOptionTextureInfo(MenuOptionType option)
    {
        return _optionTexturesInfo[(int) option];
    }
}

internal readonly struct MenuOption
{
    public readonly int Size;
    public readonly string Caption;
    public readonly Color Color;

    public MenuOption(int size, string caption, Color color)
    {
        Size = size;
        Caption = caption;
        Color = color;
    }
}