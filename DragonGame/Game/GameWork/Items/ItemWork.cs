using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.GameWork.Players;
using DuckDuckJump.Game.GameWork.Rng;
using DuckDuckJump.Game.GameWork.Sound;

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
        for (short i = 0; i < Match.Info.PlatformCount; i++)
        {
            if (RandomWork.Next(0.0f, 1.0f) < 0.25f)
            {
                AddBox(i);
            }
        }
            
    }

    private static void AddBox(short platformId)
    {
        ref var itemBox = ref ItemBoxes[_itemCount++];

        itemBox.PlatformId = platformId;
        itemBox.InvisibleTimer = 0;
    }

    public static void UpdateMe()
    {
        if ((Match.Info.GameFlags & GameInfo.Flags.NoItems) != 0)
            return;

        UpdateItemBoxes();
        UpdateBoxIntersection();
    }

    private static void UpdateBoxIntersection()
    {
        for (var i = 0; i < Match.PlayerCount; i++)
        {
            ref var player = ref PlayerWork.Get(i);

            if (player.IsCom)
                continue;

            for (var j = 0; j < _itemCount; j++)
            {
                ref var itemBox = ref ItemBoxes[j];

                if (!itemBox.IntersectsPlayer(ref player)) continue;
                
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
            ref var player = ref PlayerWork.Get(i);
            
            if(player.IsCom)
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