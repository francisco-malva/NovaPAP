using System.Diagnostics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;

namespace DuckDuckJump.Scenes.Game.Gameplay.Score;

internal sealed class Scoreboard
{
    private readonly Texture _checkmark;

    private readonly Player _player;
    private ushort _blinkTimer;

    /// <summary>
    ///     Is the checkmark that indicates the won round dark?
    /// </summary>
    private bool _checkmarkDark;

    private byte _roundsWon;
    public byte RoundsToWin;

    public Scoreboard(Player player, byte roundsToWin, GameplayResources resources)
    {
        _checkmark = resources.CheckmarkTexture;
        _player = player;
        RoundsToWin = roundsToWin;
        _roundsWon = 0;
    }

    public bool WonGame => RoundsToWin == _roundsWon;

    public void WinRound(bool draw, ushort blinkTime)
    {
        if (draw && _roundsWon == RoundsToWin - 1)
            return;
        _roundsWon++;
        _blinkTimer = blinkTime;
    }

    public void Update()
    {
        if (_blinkTimer > 0)
        {
            --_blinkTimer;

            if (_blinkTimer % 25 == 0) _checkmarkDark = !_checkmarkDark;
        }
        else
        {
            _checkmarkDark = false;
        }
    }

    public void Draw()
    {
        if (RoundsToWin == 1)
            return;

        Debug.Assert(Engine.Game.Instance != null, "Engine.Game.Instance != null");
        var renderer = Engine.Game.Instance.Renderer;

        for (var i = 0; i < RoundsToWin; i++)
        {
            if (i > _roundsWon - 1)
                _checkmark.SetColorMod(Color.Black);
            else if (i == _roundsWon - 1 && _player.State == PlayerState.Won)
                _checkmark.SetColorMod(_checkmarkDark ? Color.Black : Color.White);
            else
                _checkmark.SetColorMod(Color.White);
            renderer.Copy(_checkmark, null, new Rectangle(8 + 20 * i, 8, 16, 16));
        }
    }
}