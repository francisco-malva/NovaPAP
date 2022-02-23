using System;
using System.Diagnostics;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Scenes.Game.Gameplay.Banners;
using DuckDuckJump.Scenes.Game.Gameplay.Items;
using DuckDuckJump.Scenes.Game.Gameplay.Platforming;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;
using DuckDuckJump.Scenes.Game.Gameplay.Score;
using DuckDuckJump.Scenes.Game.Input;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Gameplay;

internal class GameField : IDisposable
{
    public const int Width = GameScene.Width / 2 - GameScene.GameBorder;
    public const int Height = GameScene.Height - GameScene.GameBorder;

    private readonly Texture _backgroundTexture;

    private readonly BannerDisplay _bannerDisplay;
    private readonly Camera _camera;
    private readonly FinishLine _finishLine;
    private readonly ItemManager _itemManager;

    private readonly GameScene _owner;
    private readonly bool _p2;

    public readonly PlatformManager PlatformManager;
    public readonly Scoreboard Scoreboard;
    public bool Flipped;

    public GameField(Random random, GameScene owner, bool p2, GameInfo info, GameplayResources resources)
    {
        _owner = owner;
        _p2 = p2;

        AiControlled = p2 ? info.P2Ai : info.P1Ai;

        PlatformManager = new PlatformManager(random, info.PlatformCount, resources);

        if (info.HasItems) _itemManager = new ItemManager(this, PlatformManager, random, resources);

        Player = AiControlled
            ? new AiPlayer(info.Difficulty, random, resources, _itemManager)
            : new HumanPlayer(random, resources, _itemManager);

        Scoreboard = new Scoreboard(Player, info.RoundsToWin, resources);

        _backgroundTexture = resources.BackgroundTexture;
        _backgroundTexture.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

        _bannerDisplay = new BannerDisplay(resources);
        _finishLine = new FinishLine(Player, PlatformManager.FinishingY, resources);

        _camera = new Camera(new Point(Width, Height), new Rectangle(0, 0, Width, PlatformManager.FinishingY));


        Debug.Assert(Engine.Game.Instance != null, "Engine.Game.Instance != null");
        OutputTexture = new Texture(Engine.Game.Instance.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
            (int) SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);
    }

    public float PlayerClimbingProgress => PlatformManager.GetClimbingProgress(Player);

    private Player Player { get; }

    public bool AiControlled { get; }

    public Texture OutputTexture { get; }

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

    public void Dispose()
    {
        OutputTexture?.Dispose();
    }

    public void SetOther(GameField other)
    {
        _itemManager?.SetOther(other);
    }

    public void GetReady()
    {
        _finishLine.Decreasing = false;
        _bannerDisplay.Raise(BannerType.GetReady, GameScene.GetReadyTime);
        PlatformManager.GeneratePlatforms();
        _itemManager?.GenerateItems();
        Player.GetReady();
        UpdateCamera();
    }

    public void BeginRound()
    {
        _finishLine.Decreasing = false;
        _bannerDisplay.Raise(BannerType.Go, GameScene.GetReadyTime / 4);
        Player.SetState(PlayerState.InGame);
    }

    public void WinRound(bool draw = false)
    {
        _finishLine.Decreasing = true;
        Player.SetState(PlayerState.Won);

        _bannerDisplay.Raise(draw ? BannerType.Draw : BannerType.Winner, GameScene.RoundEndTime);
        Scoreboard.WinRound(draw, GameScene.RoundEndTime);

        _itemManager?.EndItemEffect();
    }

    public void LoseRound()
    {
        _finishLine.Decreasing = true;
        _bannerDisplay.Raise(BannerType.YouLose, GameScene.RoundEndTime);
        Player.SetState(PlayerState.Lost);

        _itemManager?.EndItemEffect();
    }

    public void Update(GameInput input)
    {
        Player.Update(PlatformManager, input);
        PlatformManager.Update(Player);

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

    public void Draw()
    {
        var renderer = Engine.Game.Instance.Renderer;
        renderer.SetRenderTarget(OutputTexture);

        DrawBackground();
        DrawGameElements();
        Scoreboard.Draw();
        _itemManager?.DrawUi();
        _bannerDisplay.Draw();

        renderer.SetRenderTarget(null);
    }

    private void DrawGameElements()
    {
        Player.Draw(_camera);
        PlatformManager.Draw(_camera);
        _itemManager?.Draw(_camera);
        _finishLine.Draw(_camera);
    }

    private void DrawBackground()
    {
        var renderer = Engine.Game.Instance.Renderer;

        var nightExposure = Mathematics.Lerp(0.0f, 255.0f,
            PlayerClimbingProgress);

        //Drawing the day
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