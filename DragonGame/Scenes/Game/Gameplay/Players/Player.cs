﻿using System;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;
using DuckDuckJump.Scenes.Game.Gameplay.Items;
using DuckDuckJump.Scenes.Game.Gameplay.Platforming;
using DuckDuckJump.Scenes.Game.Input;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Gameplay.Players;

internal abstract class Player
{
    public const int PlatformCollisionWidth = 16;
    public const int PlatformCollisionHeight = 16;
    private const int YTerminalSpeed = -15;
    private const int YJumpSpeed = 20;
    private const int YDrag = 1;
    protected const int XMoveSpeed = 8;

    private readonly Chunk _jump;

    private readonly Texture _texture;

    protected readonly DeterministicRandom Random;

    private double _angle;

    private SDL.SDL_RendererFlip _flip;

    private ulong _stateTimer;

    protected ItemManager ItemManager;
    public Point Position;

    protected int XSpeed, YSpeed;

    public bool Umbrella;

    protected Player(DeterministicRandom random)
    {
        Random = random;
        _texture = Engine.Game.Instance.TextureManager["Game/player"];
        _jump = Engine.Game.Instance.ChunkManager["jump"];
    }

    public PlayerState State { get; private set; }

    public bool Descending => YSpeed < 0;

    public int FallSpeed => Umbrella ? YTerminalSpeed / 2 : YTerminalSpeed;

    public Rectangle Collision => new(Position.X - PlatformCollisionWidth / 2,
        Position.Y - PlatformCollisionHeight / 2, PlatformCollisionWidth, PlatformCollisionHeight);

    public void GetReady()
    {
        XSpeed = 0;
        YSpeed = 0;
        Position = new Point(GameField.Width / 2, 0);
        ResetSpecialFields();

        SetState(PlayerState.GetReady);
    }

    protected abstract void ResetSpecialFields();

    public void Update(PlatformManager platformManager, GameInput input)
    {
        UpdateMovement(platformManager, input);
        UpdateAngle();
        UpdateFlip();
        ++_stateTimer;
    }

    public void SetItemManager(ItemManager manager)
    {
        ItemManager = manager;
    }

    protected abstract void OnPressSpecial();

    private void UpdateMovement(PlatformManager platformManager, GameInput input)
    {
        switch (State)
        {
            case PlayerState.GetReady:
                UpdatePosition();
                break;
            case PlayerState.InGame:
                if ((input & GameInput.Special) != 0) OnPressSpecial();
                MoveX(platformManager, input);
                UpdatePosition();
                break;
            case PlayerState.Lost:
                UpdatePosition();
                break;
            case PlayerState.Won:
                Position.Y += (int)_stateTimer;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateAngle()
    {
        switch (State)
        {
            case PlayerState.GetReady:
                _angle = 0.0;
                break;
            case PlayerState.InGame:
                _angle = XSpeed switch
                {
                    > 0 => Math.Min(_angle + 3.0f, 25.0),
                    < 0 => Math.Max(_angle - 3.0f, -25.0),
                    _ => 0
                };
                break;
            case PlayerState.Won:
                _angle = 0.0;
                break;
            case PlayerState.Lost:
                _angle += 15.0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateFlip()
    {
        switch (State)
        {
            case PlayerState.GetReady:
                _flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;
                break;
            case PlayerState.InGame:
                if (XSpeed != 0)
                    _flip = XSpeed < 0
                        ? SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL
                        : SDL.SDL_RendererFlip.SDL_FLIP_NONE;

                break;
            case PlayerState.Won:
                break;
            case PlayerState.Lost:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdatePosition()
    {
        if (Position.Y < 0 && State != PlayerState.Lost) Jump(null, true);
        Position.X = Math.Min(GameField.Width, Math.Max(0, Position.X + XSpeed));
        YSpeed = Math.Max(FallSpeed, YSpeed - YDrag);
        Position.Y += YSpeed;
    }

    public void Draw(Camera camera)
    {
        var renderer = Engine.Game.Instance.Renderer;

        var screenPosition = camera.TransformPoint(new Point(Position.X - _texture.Width / 2,
            Position.Y + _texture.Height));
        var dst = new Rectangle(screenPosition.X, screenPosition.Y, _texture.Width,
            _texture.Height);

        renderer.CopyEx(_texture, null, dst, _angle, null, _flip);
    }

    protected abstract void MoveX(PlatformManager platformManager, GameInput input);

    public void Jump(Platform platform = null, bool ground = false, int jumpMultiplier = 1)
    {
        OnJump(platform);

        Position.Y = platform == null ? ground ? 0 : Position.Y : platform.Position.Y + Platform.PlatformHeight;
        YSpeed = YJumpSpeed * jumpMultiplier;

        _jump.Play(-1, 0);
    }

    protected abstract void OnJump(Platform platform);

    public void SetState(PlayerState state)
    {
        State = state;

        switch (state)
        {
            case PlayerState.Lost:
                XSpeed = 0;
                Jump();
                break;
            case PlayerState.GetReady:
                break;
            case PlayerState.InGame:
                break;
            case PlayerState.Won:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        _stateTimer = 0;
    }
}