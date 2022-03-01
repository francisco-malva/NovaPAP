using System;
using System.Diagnostics;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;
using DuckDuckJump.Game.Gameplay.Items;
using SDL2;

namespace DuckDuckJump.Game.Gameplay.Resources;

internal class GameplayResources : IDisposable
{
    private const string GameBorderPath = "UI/game-border";
    private const string PlayerTexturePath = "Game/Field/player";
    private const string JumpingSfxPath = "Game/Player/jump";
    private const string GameplayMusicPath = "Game/music";
    private const string FinishLineTexturePath = "Game/Field/finish-line";
    private const string ItemUiBorderTexturePath = "Game/Items/item-ui";
    private const string BackgroundTexturePath = "Game/Backgrounds/sky";
    private const string PlatformTexturePath = "Game/Field/platform";
    private const string ItemBoxTexturePath = "Game/Items/item-box";
    private const string CheckmarkTexturePath = "Game/Scoreboard/checkmark";

    private static readonly string[] AnnouncerClipPaths =
    {
        "Game/Announcer/get-ready",
        "Game/Announcer/go",
        "Game/Announcer/p1-wins",
        "Game/Announcer/p2-wins",
        "Game/Announcer/draw"
    };

    private static readonly string[] ItemTexturePaths =
    {
        "Game/Items/umbrella",
        "Game/Items/double-jump"
    };

    private static readonly string[] BannerTexturePaths =
    {
        "Game/Banners/get-ready",
        "Game/Banners/go",
        "Game/Banners/winner",
        "Game/Banners/you-lose",
        "Game/Banners/draw"
    };


    private readonly Texture?[] _itemTextures;

    public readonly Chunk[] AnnouncerClips;

    public readonly Texture? BackgroundTexture;

    public readonly Texture?[] BannerTextures;

    public readonly Texture? CheckmarkTexture;

    public readonly Texture? FinishLineTexture;
    public readonly Texture? GameBorder;

    public readonly Music GameplayMusic;

    public readonly Texture? ItemBoxTexture;

    public readonly Texture? ItemUiBorderTexture;

    public readonly Chunk JumpingSfx;

    public readonly Texture OutputTexture;

    public readonly Texture? PlatformTexture;

    public readonly Texture? PlayerTexture;

    public GameplayResources(ResourceManager resourceManager)
    {
        GameBorder = resourceManager.Textures[GameBorderPath];
        PlayerTexture = resourceManager.Textures[PlayerTexturePath];
        FinishLineTexture = resourceManager.Textures[FinishLineTexturePath];

        CheckmarkTexture = resourceManager.Textures[CheckmarkTexturePath];

        ItemUiBorderTexture = resourceManager.Textures[ItemUiBorderTexturePath];

        _itemTextures = new Texture?[ItemTexturePaths.Length];
        for (var i = 0; i < ItemTexturePaths.Length; i++)
            _itemTextures[i] = resourceManager.Textures[ItemTexturePaths[i]];

        ItemBoxTexture = resourceManager.Textures[ItemBoxTexturePath];

        BackgroundTexture = resourceManager.Textures[BackgroundTexturePath];

        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        OutputTexture = new Texture(GameContext.Instance.Renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
            (int) SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, GameField.Width, GameField.Height);

        BannerTextures = new Texture?[BannerTexturePaths.Length];
        for (var i = 0; i < BannerTexturePaths.Length; i++)
            BannerTextures[i] = resourceManager.Textures[BannerTexturePaths[i]];

        PlatformTexture = resourceManager.Textures[PlatformTexturePath];

        JumpingSfx = resourceManager.Chunks[JumpingSfxPath];
        AnnouncerClips = new Chunk[AnnouncerClipPaths.Length];
        for (var i = 0; i < AnnouncerClipPaths.Length; i++)
            AnnouncerClips[i] = resourceManager.Chunks[AnnouncerClipPaths[i]];

        GameplayMusic = resourceManager.Musics[GameplayMusicPath];
    }

    public void Dispose()
    {
        BackgroundTexture?.Dispose();
        CheckmarkTexture?.Dispose();
        FinishLineTexture?.Dispose();
        GameBorder?.Dispose();
        GameplayMusic.Dispose();
        ItemBoxTexture?.Dispose();
        ItemUiBorderTexture?.Dispose();
        JumpingSfx.Dispose();
        PlatformTexture?.Dispose();
        PlayerTexture?.Dispose();

        foreach (var itemTexture in _itemTextures) itemTexture?.Dispose();

        foreach (var announcerClip in AnnouncerClips) announcerClip.Dispose();

        foreach (var bannerTexture in BannerTextures) bannerTexture?.Dispose();

        OutputTexture.Dispose();
    }

    public Texture? GetItemTexture(Item itemType)
    {
        return _itemTextures[(int) itemType];
    }
}