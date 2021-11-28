
using System;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay;
using DragonGame.Scenes.Game.Gameplay.Players;

internal class FinishLine
{
    private int _y;
    private byte _alpha;
    public bool Decreasing;

    private Texture _finishLine;

    private Player _player;

    public FinishLine(Player player, int y)
    {
        _player = player;
        _y = y;

        _finishLine = DragonGame.Engine.Game.Instance.TextureManager["Game/finish-line"];
        _finishLine.SetBlendMode(SDL2.SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public void Update()
    {
        if (Decreasing)
        {
            if (_alpha > 0)
            {
                short alpha = (short)Math.Max((short)_alpha - 5, (short)0);
                _alpha = (byte)alpha;
            }
        }
        else
        {
            if (_alpha < byte.MaxValue)
            {
                short alpha = (short)Math.Min((short)_alpha + 5, (short)byte.MaxValue);
                _alpha = (byte)alpha;
            }
        }
    }

    public void Draw(int yScroll)
    {
        _finishLine.SetAlphaMod(_alpha);
        DragonGame.Engine.Game.Instance.Renderer.Copy(_finishLine, null, new Rectangle(0, GameField.TransformY(_y - 8, yScroll), GameField.Width, 16));
    }

    public bool CrossedFinishLine => _player.Position.Y + Player.PlatformCollisionHeight >= _y;
}