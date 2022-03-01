using System;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using SDL2;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuScene : Scene
{
    private readonly MenuOptionType[] _renderedOptions =
    {
        MenuOptionType.Title,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.Start,
        MenuOptionType.Settings,
        MenuOptionType.Quit
    };

    private int _selection;
    private int _oldSelection;
    private float _selectionFadeProgress;
    
    private readonly Renderer _renderer;
    private readonly MainMenuResources _resources;

    private void SetSelection(int newSelection)
    {
        _oldSelection = _selection;
        _selection = newSelection;
        _selectionFadeProgress = 0.0f;
    }
    public MainMenuScene()
    {
        _resources = new MainMenuResources(ResourceManager);

        Debug.Assert(GameContext.Instance != null, "Engine.GameContext.Instance != null");
        _renderer = GameContext.Instance.Renderer;
    }

    private int CalculateY(int menuOption)
    {
        var y = 0;

        for (var i = 1; i <= menuOption; i++)
        {
            y += _resources.GetOptionTextureInfo(_renderedOptions[i]).Height;
        }

        return y;
    }
    private Rectangle GetDestination(int menuOption)
    {
        var menu = _renderedOptions[menuOption];
        
        var textureInfo = _resources.GetOptionTextureInfo(menu);

        var destination = new Rectangle(640 / 2 - textureInfo.Width / 2, CalculateY(menuOption), textureInfo.Width, textureInfo.Height);
        return destination;
    }
    private void RenderOptions()
    {
        for (var i = 0; i < _renderedOptions.Length; i++)
        {
            var menu = _renderedOptions[i];
            
            var texture = _resources.GetOptionTexture(menu);

            var prev = GetDestination(_oldSelection);
            var current = GetDestination(i);
            
            if (i == _selection)
            {
                _renderer.BlendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
                _renderer.DrawColor = Color.FromArgb(128, Color.Blue.R, Color.Blue.G, Color.Blue.B);
                _renderer.FillRect(new Rectangle((int) Mathematics.Lerp(prev.X, current.X, _selectionFadeProgress),
                    (int) Mathematics.Lerp(prev.Y, current.Y, _selectionFadeProgress),
                    (int) Mathematics.Lerp(prev.Width, current.Width, _selectionFadeProgress),
                    (int) Mathematics.Lerp(prev.Height, current.Height, _selectionFadeProgress)));
            }

            _renderer.Copy(texture, null,
                current);
        }
    }

    public override void OnTick()
    {
        _renderer.DrawColor = Color.Black;
        _renderer.Clear();

        _selectionFadeProgress = MathF.Min(1.0f, _selectionFadeProgress + 0.1f);
        
        if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
        {
            if (_selection + 1 == _renderedOptions.Length)
            {
                SetSelection(0);
            }
            else
                SetSelection(_selection + 1);
        }
        RenderOptions();

        _renderer.Present();
    }

    protected override void OnUnload()
    {
        _resources.Dispose();
    }
}