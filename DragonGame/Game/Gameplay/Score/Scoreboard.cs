using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Gameplay.Players;
using DuckDuckJump.Game.Gameplay.Resources;

namespace DuckDuckJump.Game.Gameplay.Score;

internal sealed class Scoreboard
{
    private readonly Texture _checkmarkTexture;

    private readonly Player _player;
    public readonly sbyte RoundsToWin;
    private ushort _blinkTimer;

    /// <summary>
    ///     Is the checkmark that indicates the won round dark?
    /// </summary>
    private bool _checkmarkDark;

    private sbyte _roundsWon;

    public Scoreboard(Player player, sbyte roundsToWin, GameplayResources resources)
    {
        Debug.Assert(resources.CheckmarkTexture != null, "resources.CheckmarkTexture != null");
        _checkmarkTexture = resources.CheckmarkTexture;
        _player = player;
        RoundsToWin = roundsToWin;
        _roundsWon = 0;
    }

    public bool WonGame => RoundsToWin == _roundsWon;

    public void WinRound(bool draw, ushort blinkTime)
    {
        if (RoundsToWin < 0 || draw && _roundsWon == RoundsToWin - 1)
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

        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        var renderer = GameContext.Instance.Renderer;

        for (var i = 0; i < RoundsToWin; i++)
        {
            if (i > _roundsWon - 1)
                _checkmarkTexture.SetColorMod(Color.Black);
            else if (i == _roundsWon - 1 && _player.State == PlayerState.Won)
                _checkmarkTexture.SetColorMod(_checkmarkDark ? Color.Black : Color.White);
            else
                _checkmarkTexture.SetColorMod(Color.White);
            renderer.Copy(_checkmarkTexture, null, new Rectangle(8 + 20 * i, 8, 16, 16));
        }
    }
}