using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Gameplay.Items.Behaviors;
using DuckDuckJump.Game.Gameplay.Messaging;
using DuckDuckJump.Game.Gameplay.Platforming;
using DuckDuckJump.Game.Gameplay.Players;
using DuckDuckJump.Game.Gameplay.Resources;

namespace DuckDuckJump.Game.Gameplay.Items;

internal class ItemManager : MessagePoint
{
    private const ushort ShufflingTime = 60;

    private static readonly Item[] WinningTable =
    {
        Item.DoubleJump
    };

    private static readonly Item[] LosingTable =
    {
        Item.Umbrella,
        Item.DoubleJump
    };

    private readonly List<ItemBox> _itemBoxes;

    private readonly Texture? _itemBoxTexture;

    private readonly Texture? _itemUiBorderTexture;

    private readonly TextureInfo _itemUiBorderTextureInfo;

    private readonly GameField _owner;
    private readonly PlatformManager _platformManager;

    private readonly MessagePump _pump;

    private readonly Random _random;


    private readonly GameplayResources _resources;
    private ItemBehavior? _currentItem;


    private float _fade;

    private ItemManagerState _state;
    private ushort _timer;

    public ItemManager(GameField owner, PlatformManager platformManager, Random random,
        GameplayResources resources, MessagePump pump) : base(pump)
    {
        _pump = pump;
        _random = random;

        _resources = resources;

        _itemUiBorderTexture = resources.ItemUiBorderTexture;
        Debug.Assert(_itemUiBorderTexture != null, nameof(_itemUiBorderTexture) + " != null");
        _itemUiBorderTextureInfo = _itemUiBorderTexture.QueryTexture();

        _itemBoxTexture = resources.ItemBoxTexture;
        _itemBoxes = new List<ItemBox>();
        _platformManager = platformManager;

        _owner = owner;
    }

    public void SetOther(GameField other)
    {
    }

    public void GenerateItems(Player player)
    {
        _itemBoxes.Clear();

        foreach (var platform in _platformManager.Platforms)
            if (_random.NextSingle() <= 0.35f)
            {
                Debug.Assert(platform != null, nameof(platform) + " != null");
                _itemBoxes.Add(new ItemBox(player, new Point(platform.Position.X, platform.Position.Y + 32 + 8),
                    _itemBoxTexture));
            }
    }

    public void Update()
    {
        UpdateItemBoxes();

        if (_timer > 0) _timer--;
        _fade += 0.1f;

        switch (_state)
        {
            case ItemManagerState.NoItem:
                break;
            case ItemManagerState.Shuffling:
                ShufflingUpdate();
                break;
            case ItemManagerState.UsingItem:
                UsingItemUpdate();
                break;
            case ItemManagerState.GotItem:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UseItem()
    {
        if (_state != ItemManagerState.GotItem) return;
        SetState(ItemManagerState.UsingItem);
    }


    private void UsingItemUpdate()
    {
        if (_currentItem.IsDone())
            EndItemEffect();
        else
            _currentItem.Update();
    }

    public void EndItemEffect()
    {
        if (_currentItem != null)
        {
            _currentItem.OnItemOver();
            _currentItem = null;
            SetState(ItemManagerState.NoItem);
        }
    }

    private void UpdateItemBoxes()
    {
        foreach (var item in _itemBoxes)
        {
            item.Update();

            if (!item.CanCatch) continue;

            item.Catch();
            ShuffleItems();
        }
    }

    private void ShuffleItems()
    {
        if (_state == ItemManagerState.NoItem) SetState(ItemManagerState.Shuffling);
    }

    private void ShufflingUpdate()
    {
        if (_timer == 0) SetState(ItemManagerState.GotItem);
    }

    private void SetState(ItemManagerState state)
    {
        _state = state;

        switch (state)
        {
            case ItemManagerState.NoItem:
                break;
            case ItemManagerState.Shuffling:
                _timer = ShufflingTime;
                break;
            case ItemManagerState.GotItem:
                _currentItem = GetRandomItem();
                break;
            case ItemManagerState.UsingItem:
                _currentItem.OnUse();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private ItemBehavior GetRandomItem()
    {
        if (_owner.IsWinning)
            return _currentItem = GetItemBehavior(WinningTable[_random.Next(WinningTable.Length)]);

        return GetItemBehavior(LosingTable[_random.Next(LosingTable.Length)]);
    }

    private ItemBehavior GetItemBehavior(Item item)
    {
        return item switch
        {
            Item.DoubleJump => new DoubleJump(_resources, _pump),
            Item.Umbrella => new Umbrella(_resources, _pump),
            _ => null
        };
    }

    public void Draw(Camera camera)
    {
        foreach (var item in _itemBoxes) item.Draw(camera);
    }

    public void DrawUi()
    {
        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        var renderer = GameContext.Instance.Renderer;

        renderer.Copy(_itemUiBorderTexture, null,
            new Rectangle(GameField.Width - _itemUiBorderTextureInfo.Width - 8, 8, _itemUiBorderTextureInfo.Width,
                _itemUiBorderTextureInfo.Height));

        switch (_state)
        {
            case ItemManagerState.NoItem:
                break;
            case ItemManagerState.Shuffling:
                //DrawItem(renderer, (Item)(_timer % 2));
                break;
            case ItemManagerState.GotItem:
                DrawItem(renderer, _currentItem, false);
                break;
            case ItemManagerState.UsingItem:
                DrawItem(renderer, _currentItem, true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DrawItem(Renderer renderer, ItemBehavior item, bool blinking)
    {
        var a = blinking ? (byte) ((MathF.Cos(_fade) + 1.0f) * 0.5f * 255) : (byte) 255;
        item.Texture.SetAlphaMod(a);
        renderer.Copy(item.Texture, null,
            new Rectangle(GameField.Width - _itemUiBorderTextureInfo.Width - 8 + 2, 8 + 2,
                _itemUiBorderTextureInfo.Width - 4, _itemUiBorderTextureInfo.Height - 4));
    }

    public override void HandleMessage(Message message)
    {
        switch (message.Type)
        {
            case MessageType.RequestItemUsage:
                UseItem();
                break;
            case MessageType.PlayerDoubleJump:
            case MessageType.PlayerBeginUmbrella:
            case MessageType.PlayerEndUmbrella:
            default:
                break;
        }
    }
}