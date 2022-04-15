using System;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Gameplay.Players;
using DuckDuckJump.Game.Gameplay.Resources;
using SDL2;

namespace DuckDuckJump.Game.Gameplay;

internal class FinishLine
{
    private readonly Texture? _finishLineTexture;
    private readonly TextureInfo _finishLineTextureInfo;
    private readonly Player _player;
    private readonly int _y;
    private byte _alpha;
    public bool Decreasing;

    public FinishLine(Player player, int y, GameplayResources resources)
    {
        _player = player;
        _y = y;

        _finishLineTexture = resources.FinishLineTexture;
        _finishLineTextureInfo = _finishLineTexture.QueryTexture();
        _finishLineTexture.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public bool CrossedFinishLine => _player.Position.Y + Player.PlatformCollisionHeight >= _y;

    public void Update()
    {
        if (Decreasing)
        {
            if (_alpha > 0)
            {
                var alpha = (short)Math.Max(_alpha - 5, 0);
                _alpha = (byte)alpha;
            }
        }
        else
        {
            if (_alpha < byte.MaxValue)
            {
                var alpha = (short)Math.Min(_alpha + 5, byte.MaxValue);
                _alpha = (byte)alpha;
            }
        }
    }

    public void Draw(Camera camera)
    {
        var screenPosition = camera.TransformPoint(new Point(0, _y + _finishLineTextureInfo.Height / 2));
        var dst = new Rectangle(screenPosition.X, screenPosition.Y, _finishLineTextureInfo.Width,
            _finishLineTextureInfo.Height);
        if (!camera.OnScreen(dst)) return;

        //My lord
        _finishLineTexture.SetAlphaMod(_alpha);
        GameContext.Instance.Renderer.Copy(_finishLineTexture, null, dst);
        GameContext.Instance.Renderer.Copy(_finishLineTexture, null,
            new Rectangle(dst.X + _finishLineTextureInfo.Width, dst.Y, dst.Width, dst.Height));
        GameContext.Instance.Renderer.Copy(_finishLineTexture, null,
            new Rectangle(dst.X + _finishLineTextureInfo.Width * 2, dst.Y, dst.Width, dst.Height));
        GameContext.Instance.Renderer.Copy(_finishLineTexture, null,
            new Rectangle(dst.X + _finishLineTextureInfo.Width * 3, dst.Y, dst.Width, dst.Height));
    }
}