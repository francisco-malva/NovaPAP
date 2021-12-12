using System;
using System.Collections.Generic;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Platforming;
using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items;

internal class ItemManager
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

    private readonly Texture[] _itemTextureTable =
    {
        Engine.Game.Instance.TextureManager["Game/umbrella"],
        Engine.Game.Instance.TextureManager["Game/double-jump"]
    };

    private readonly Texture _itemUi;

    private readonly GameField _owner;
    private readonly PlatformManager _platformManager;

    private readonly Player _player;
    private readonly DeterministicRandom _random;
    private Item _currentItem;

    private bool _p2;

    private ItemManagerState _state;
    private ushort _timer;

    public ItemManager(GameField owner, Player player, PlatformManager platformManager, DeterministicRandom random)
    {
        _random = random;
        _itemUi = Engine.Game.Instance.TextureManager["Game/item_ui"];
        _itemBoxes = new List<ItemBox>();
        _platformManager = platformManager;

        _player = player;

        _owner = owner;
    }

    public void GenerateItems()
    {
        _itemBoxes.Clear();

        foreach (var platform in _platformManager.Platforms)
            if (_random.GetFloat() <= 0.35f)
                _itemBoxes.Add(new ItemBox(_player, new Point(platform.Position.X, platform.Position.Y + 32 + 8)));
    }

    public void Update()
    {
        UpdateItemBoxes();

        if (_timer > 0) _timer--;

        switch (_state)
        {
            case ItemManagerState.NoItem:
                break;
            case ItemManagerState.Shuffling:
                ShufflingUpdate();
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

        ActivateItemEffect();
        SetState(ItemManagerState.NoItem);
    }

    private void ActivateItemEffect()
    {
        switch (_currentItem)
        {
            case Item.Umbrella:
                _player.UseUmbrella();
                break;
            case Item.DoubleJump:
                _player.Jump();
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
                ChooseItem();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void ChooseItem()
    {
        if (_owner.IsWinning) _currentItem = WinningTable[_random.GetInteger(WinningTable.Length)];

        _currentItem = LosingTable[_random.GetInteger(LosingTable.Length)];
    }

    public void Draw(Camera camera)
    {
        foreach (var item in _itemBoxes) item.Draw(camera);
    }

    public void DrawUi()
    {
        var renderer = Engine.Game.Instance.Renderer;

        renderer.Copy(_itemUi, null,
            new Rectangle(GameField.Width - _itemUi.Width - 8, 8, _itemUi.Width, _itemUi.Height));

        switch (_state)
        {
            case ItemManagerState.NoItem:
                break;
            case ItemManagerState.Shuffling:
                DrawItem(renderer, (Item)(_timer % 2));
                break;
            case ItemManagerState.GotItem:
                DrawItem(renderer, _currentItem);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DrawItem(Renderer renderer, Item item)
    {
        renderer.Copy(_itemTextureTable[(int)item], null,
            new Rectangle(GameField.Width - _itemUi.Width - 8 + 2, 8 + 2, _itemUi.Width - 4, _itemUi.Height - 4));
    }
}