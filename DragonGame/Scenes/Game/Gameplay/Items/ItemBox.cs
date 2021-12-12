using System;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items;

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


    private const ushort CaughtTime = 10;
    private const ushort CooldownTime = 120;
    private const ushort AppearingTime = 10;

    private readonly Texture _itemBox;

    private readonly Player _player;

    private byte _alpha = 255;
    private ItemBoxState _state;

    private ushort _timer;
    public Point Position;

    public ItemBox(Player player, Point position)
    {
        _player = player;
        Position = position;
        _itemBox = Engine.Game.Instance.TextureManager["Game/itembox"];
        _itemBox.SetBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
    }

    public bool CanCatch
    {
        get
        {
            var collision = Collision;
            return _state == ItemBoxState.Active && _player.Collision.HasIntersection(ref collision);
        }
    }

    private Rectangle Collision => new(Position.X - CollisionWidth / 2, Position.Y - CollisionHeight / 2,
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
        var transformedPoint = camera.TransformPoint(Position);

        var dst = new Rectangle(transformedPoint.X - _itemBox.Width / 2, transformedPoint.Y - _itemBox.Height / 2,
            _itemBox.Width, _itemBox.Height);

        if (!camera.OnScreen(dst))
            return;

        _itemBox.SetAlphaMod(_alpha);
        Engine.Game.Instance.Renderer.Copy(_itemBox, null,
            dst);
    }
}