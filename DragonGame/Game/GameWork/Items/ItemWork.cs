#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.GameWork.Players;
using DuckDuckJump.Game.GameWork.Rng;
using DuckDuckJump.Game.GameWork.Sound;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game.GameWork.Items;

internal static class ItemWork
{
    private const short MaxItemCount = 1024;
    private static readonly ItemBox[] ItemBoxes = new ItemBox[MaxItemCount];
    private static short _itemCount;

    private static readonly Vector2[] BoxDrawPositions =
    {
        new(10.0f, 50.0f),
        new(Graphics.LogicalSize.Width - 64.0f, 50.0f)
    };

    private static readonly ItemType[] PlayerItems = new ItemType[Match.PlayerCount];

    private static readonly MatchAssets.TextureIndex[] UiItemTextures =
    {
        MatchAssets.TextureIndex.DoubleJumpItem,
        MatchAssets.TextureIndex.DoubleJumpItem,
        MatchAssets.TextureIndex.FreezeItem,
        MatchAssets.TextureIndex.SlowdownItem,
        MatchAssets.TextureIndex.ReviveItem,
        MatchAssets.TextureIndex.UmbrellaItem
    };

    public static void Reset()
    {
        _itemCount = 0;

        for (var i = 0; i < PlayerItems.Length; i++) PlayerItems[i] = ItemType.None;
        for (short i = 0; i < Match.Info.PlatformCount; i++)
            if (RandomWork.Next(0.0f, 1.0f) < 0.25f)
                AddBox(i);
    }

    private static void AddBox(short platformId)
    {
        ref var itemBox = ref ItemBoxes[_itemCount++];

        itemBox.PlatformId = platformId;
        itemBox.InvisibleTimer = 0;
    }

    public static void Update(Span<GameInput> inputs)
    {
        if (Match.State != Match.MatchState.InGame || (Match.Info.GameFlags & GameInfo.Flags.NoItems) != 0)
            return;

        UpdateItemBoxes();
        UpdateBoxIntersection();
        UpdateItemUsage(inputs);
    }

    private static void UpdateItemUsage(Span<GameInput> inputs)
    {
        for (var i = 0; i < Match.PlayerCount; i++)
        {
            var player = PlayerWork.Get(i);

            if (player.IsCom)
                continue;

            if ((inputs[i] & GameInput.Special) == 0 || PlayerItems[i] == ItemType.None) continue;
            UseItem(i, ref player, PlayerItems[i]);
            PlayerItems[i] = ItemType.None;
        }
    }

    private static void UseItem(int index, ref Player player, ItemType playerItem)
    {
        switch (playerItem)
        {
            case ItemType.None:
                break;
            case ItemType.DoubleJump:
                player.Jump();
                break;
            case ItemType.Freeze:
                for (var i = 0; i < Match.PlayerCount; i++)
                {
                    var otherPlayer = PlayerWork.Get(i);

                    if (i == index) continue;

                    otherPlayer.ApplyFreeze();
                }

                break;
            case ItemType.Slowdown:
                for (var i = 0; i < Match.PlayerCount; i++)
                {
                    var otherPlayer = PlayerWork.Get(i);

                    if (i == index) continue;

                    otherPlayer.ApplySlowdown();
                }

                break;
            case ItemType.Revive:
                player.ApplyRevive();
                break;
            case ItemType.Umbrella:
                player.ApplyUmbrella();
                break;
            case ItemType.All:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerItem), playerItem, null);
        }
    }

    private static void UpdateBoxIntersection()
    {
        for (var i = 0; i < Match.PlayerCount; i++)
        {
            var player = PlayerWork.Get(i);

            if (player.IsCom)
                continue;

            for (var j = 0; j < _itemCount; j++)
            {
                ref var itemBox = ref ItemBoxes[j];

                if (!itemBox.IntersectsPlayer(ref player) || PlayerItems[i] != ItemType.None) continue;

                itemBox.InvisibleTimer = 60 * 3;
                SoundEffectWork.Queue(MatchAssets.SfxIndex.ItemPop, 0.5f);
                PlayerItems[i] = (ItemType)RandomWork.Next((byte)ItemType.DoubleJump, (byte)ItemType.All);
            }
        }
    }

    private static void UpdateItemBoxes()
    {
        for (var i = 0; i < _itemCount; i++) ItemBoxes[i].Update();
    }

    public static void DrawGui()
    {
        if ((Match.Info.GameFlags & GameInfo.Flags.NoItems) != 0)
            return;

        for (var i = 0; i < Match.PlayerCount; i++)
        {
            var player = PlayerWork.Get(i);

            if (player.IsCom)
                continue;

            var uiTransform = Matrix3x2.CreateScale(1.0f - Match.Fade) *
                              Matrix3x2.CreateTranslation(BoxDrawPositions[i]);

            Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.ItemFrame), null,
                Matrix3x2.CreateScale(1.0f - Match.Fade) * Matrix3x2.CreateTranslation(BoxDrawPositions[i]),
                Color.White);

            if (Match.State != Match.MatchState.InGame || PlayerItems[i] == ItemType.None) continue;

            var playerItem = PlayerItems[i];
            Graphics.Draw(MatchAssets.Texture(UiItemTextures[(int)playerItem]), null, uiTransform, Color.White);
        }
    }

    public static void DrawMe()
    {
        if ((Match.Info.GameFlags & GameInfo.Flags.NoItems) != 0)
            return;

        for (var i = 0; i < _itemCount; i++) ItemBoxes[i].DrawMe();
    }
}