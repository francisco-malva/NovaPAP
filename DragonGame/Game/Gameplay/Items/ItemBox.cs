using System;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Gameplay.Players;
using SDL2;

namespace DuckDuckJump.Game.Gameplay.Items;

internal enum ItemBoxState
{
    Active,
    Caught,
    Invisible,
    Appearing
}

internal class ItemBox
{
    private const int CollisionWidth = 32;
    private const int CollisionHeight = 32;


    private const ushort CaughtTime = 5;
    private const ushort CooldownTime = 120;
    private const ushort AppearingTime = 10;

    private readonly Texture? _itemBoxTexture;
    private readonly TextureInfo _itemBoxTextureInfo;

    private readonly Player _player;
    private readonly Point _position;

    private byte _alpha = 255;
    private ItemBoxState _state;

    private ushort _timer;

    public ItemBox(Player player, Point position, Texture? itemBoxTexture)
    {
        _player = player;
        _position = position;
        _itemBoxTexture = itemBoxTexture;
        Debug.Assert(_itemBoxTexture != null, nameof(_itemBoxTexture) + " != null");
        _itemBoxTextureInfo = _itemBoxTexture.QueryTexture();
        _itemBoxTexture.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public bool CanCatch
    {
        get
        {
            var collision = Collision;
            return _state == ItemBoxState.Active && _player.Collision.IntersectsWith(collision);
        }
    }

    private Rectangle Collision => new(_position.X - CollisionWidth / 2, _position.Y - CollisionHeight / 2,
        CollisionWidth, CollisionHeight);

    private void SetState(ItemBoxState state)
    {
        _state = state;

        switch (state)
        {
            case ItemBoxState.Active:
                break;
            case ItemBoxState.Caught:
                _timer = CaughtTime;
                break;
            case ItemBoxState.Invisible:
                _timer = CooldownTime;
                break;
            case ItemBoxState.Appearing:
                _timer = AppearingTime;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public void Update()
    {
        if (_timer > 0) _timer--;

        switch (_state)
        {
            case ItemBoxState.Active:
                break;
            case ItemBoxState.Caught:
                CaughtUpdate();
                break;
            case ItemBoxState.Invisible:
                InvisibleUpdate();
                break;
            case ItemBoxState.Appearing:
                AppearingUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Catch()
    {
        SetState(ItemBoxState.Caught);
    }

    private void CaughtUpdate()
    {
        if (_timer == 0)
        {
            _alpha = 0;
            SetState(ItemBoxState.Invisible);
        }
        else
        {
            _alpha = (byte)((float)_timer / CaughtTime * byte.MaxValue);
        }
    }

    private void InvisibleUpdate()
    {
        if (_timer == 0) SetState(ItemBoxState.Appearing);
    }

    private void AppearingUpdate()
    {
        if (_timer == 0)
        {
            SetState(ItemBoxState.Active);
            _alpha = 255;
        }
        else
        {
            _alpha = (byte)(byte.MaxValue - (byte)((float)_timer / CaughtTime * byte.MaxValue));
        }
    }

    public void Draw(Camera camera)
    {
        var transformedPoint = camera.TransformPoint(_position);

        var dst = new Rectangle(transformedPoint.X - _itemBoxTextureInfo.Width / 2,
            transformedPoint.Y - _itemBoxTextureInfo.Height / 2,
            _itemBoxTextureInfo.Width, _itemBoxTextureInfo.Height);

        if (!camera.OnScreen(dst))
            return;

        Debug.Assert(_itemBoxTexture != null, nameof(_itemBoxTexture) + " != null");
        _itemBoxTexture.SetAlphaMod(_alpha);
        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        GameContext.Instance.Renderer.Copy(_itemBoxTexture, null,
            dst);
    }
}