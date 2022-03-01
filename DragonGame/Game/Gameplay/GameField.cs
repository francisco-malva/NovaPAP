using System;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Gameplay.Banners;
using DuckDuckJump.Game.Gameplay.Items;
using DuckDuckJump.Game.Gameplay.Messaging;
using DuckDuckJump.Game.Gameplay.Platforming;
using DuckDuckJump.Game.Gameplay.Players;
using DuckDuckJump.Game.Gameplay.Players.AI;
using DuckDuckJump.Game.Gameplay.Resources;
using DuckDuckJump.Game.Gameplay.Score;
using DuckDuckJump.Game.Input;
using SDL2;

namespace DuckDuckJump.Game.Gameplay;

internal class GameField
{
    public const int Width = GameMatch.Width / 2 - GameMatch.GameBorder;
    public const int Height = GameMatch.Height - GameMatch.GameBorder;

    private readonly Texture? _backgroundTexture;

    private readonly BannerDisplay _bannerDisplay;
    private readonly Camera _camera;
    private readonly FinishLine _finishLine;
    private readonly ItemManager? _itemManager;

    private readonly MessagePump _messagePump;

    private readonly GameMatch _owner;
    private readonly bool _p2;

    private readonly PlatformManager _platformManager;
    public readonly Scoreboard Scoreboard;

    public GameField(Random random, GameMatch owner, bool p2, GameInfo info, GameplayResources resources)
    {
        _messagePump = new MessagePump();

        _owner = owner;
        _p2 = p2;

        AiControlled = p2 ? info.P2Ai : info.P1Ai;

        _platformManager = new PlatformManager(random, info.PlatformCount, resources);

        if (info.HasItems) _itemManager = new ItemManager(this, _platformManager, random, resources, _messagePump);

        Player = AiControlled
            ? new AiPlayer(info.Difficulty, random, resources, _messagePump)
            : new HumanPlayer(random, resources, _messagePump);

        Scoreboard = new Scoreboard(Player, info.RoundsToWin, resources);

        _backgroundTexture = resources.BackgroundTexture;
        Debug.Assert(_backgroundTexture != null, nameof(_backgroundTexture) + " != null");
        _backgroundTexture.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

        _bannerDisplay = new BannerDisplay(resources);
        _finishLine = new FinishLine(Player, _platformManager.FinishingY, resources);

        _camera = new Camera(new Point(Width, Height), new Rectangle(0, 0, Width, _platformManager.FinishingY));
    }

    public float PlayerClimbingProgress => _platformManager.GetClimbingProgress(Player);

    private Player Player { get; }

    public bool AiControlled { get; }

    public bool PlayerWonGame => Scoreboard.WonGame;

    public bool PlayerWonRound => _finishLine.CrossedFinishLine;

    public bool IsWinning
    {
        get
        {
            if (_p2) return _owner.WinningSide == Winner.P2;

            return _owner.WinningSide == Winner.P1;
        }
    }

    public void SetOther(GameField other)
    {
        _itemManager?.SetOther(other);
    }

    public void GetReady()
    {
        _finishLine.Decreasing = false;
        _bannerDisplay.Raise(BannerType.GetReady, GameMatch.GetReadyTime);
        _platformManager.GeneratePlatforms();
        _itemManager?.GenerateItems(Player);
        Player.GetReady();
        UpdateCamera();
    }

    public void BeginRound()
    {
        _finishLine.Decreasing = false;
        _bannerDisplay.Raise(BannerType.Go, GameMatch.GetReadyTime / 4);
        Player.SetState(PlayerState.InGame);
    }

    public void WinRound(bool draw = false)
    {
        _finishLine.Decreasing = true;
        Player.SetState(PlayerState.Won);

        _bannerDisplay.Raise(draw ? BannerType.Draw : BannerType.Winner, GameMatch.RoundEndTime);
        Scoreboard.WinRound(draw, GameMatch.RoundEndTime);

        _itemManager?.EndItemEffect();
    }

    public void LoseRound()
    {
        _finishLine.Decreasing = true;
        _bannerDisplay.Raise(BannerType.YouLose, GameMatch.RoundEndTime);
        Player.SetState(PlayerState.Lost);

        _itemManager?.EndItemEffect();
    }

    public void Update(GameInput input)
    {
        _messagePump.HandleMessages();
        Player.Update(_platformManager, input);
        _platformManager.Update(Player);

        _bannerDisplay.Update();
        _finishLine.Update();

        switch (Player.State)
        {
            case PlayerState.InGame:
                UpdateCamera();
                _itemManager?.Update();
                break;
            case PlayerState.Won:
                Scoreboard.Update();
                break;
            case PlayerState.GetReady:
                break;
            case PlayerState.Lost:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(input));
        }
    }

    private void UpdateCamera()
    {
        _camera.Position = new Point(0, Player.Position.Y - Height / 2);
    }

    public void Draw(Texture outputTexture)
    {
        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        var renderer = GameContext.Instance.Renderer;
        renderer.SetRenderTarget(outputTexture);

        DrawBackground();
        DrawGameElements();
        Scoreboard.Draw();
        _itemManager?.DrawUi();
        _bannerDisplay.Draw();
    }

    private void DrawGameElements()
    {
        Player.Draw(_camera);
        _platformManager.Draw(_camera);
        _itemManager?.Draw(_camera);
        _finishLine.Draw(_camera);
    }

    private void DrawBackground()
    {
        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        var renderer = GameContext.Instance.Renderer;

        var nightExposure = Mathematics.Lerp(0.0f, 255.0f,
            PlayerClimbingProgress);

        //Drawing the day
        Debug.Assert(_backgroundTexture != null, nameof(_backgroundTexture) + " != null");
        _backgroundTexture.SetAlphaMod(255);
        renderer.Copy(_backgroundTexture,
            new Rectangle(0, 0, 250, 250),
            new Rectangle(0, 0, 250 * 2, 250 * 2));

        _backgroundTexture.SetAlphaMod((byte) nightExposure);
        renderer.Copy(_backgroundTexture,
            new Rectangle(250, 0, 250 * 2, 250 * 2),
            new Rectangle(0, 0, 250 * 2, 250 * 2));
    }
}