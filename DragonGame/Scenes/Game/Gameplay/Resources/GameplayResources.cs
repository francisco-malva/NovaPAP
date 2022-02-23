using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;
using DuckDuckJump.Scenes.Game.Gameplay.Items;

namespace DuckDuckJump.Scenes.Game.Gameplay.Resources;

internal class GameplayResources
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


    private readonly Texture[] _itemTextures;

    public readonly Chunk[] AnnouncerClips;

    public readonly Texture BackgroundTexture;

    public readonly Texture[] BannerTextures;

    public readonly Texture CheckmarkTexture;

    public readonly Texture FinishLineTexture;
    public readonly Texture GameBorder;

    public readonly Music GameplayMusic;

    public readonly Texture ItemBoxTexture;

    public readonly Texture ItemUiBorderTexture;

    public readonly Chunk JumpingSfx;

    public readonly Texture PlatformTexture;

    public readonly Texture PlayerTexture;

    public GameplayResources(ResourceProviders resourceProviders)
    {
        GameBorder = resourceProviders.Textures[GameBorderPath];
        PlayerTexture = resourceProviders.Textures[PlayerTexturePath];
        FinishLineTexture = resourceProviders.Textures[FinishLineTexturePath];

        CheckmarkTexture = resourceProviders.Textures[CheckmarkTexturePath];

        ItemUiBorderTexture = resourceProviders.Textures[ItemUiBorderTexturePath];

        _itemTextures = new Texture[ItemTexturePaths.Length];
        for (var i = 0; i < AnnouncerClipPaths.Length; i++)
            _itemTextures[i] = resourceProviders.Textures[ItemTexturePaths[i]];

        ItemBoxTexture = resourceProviders.Textures[ItemBoxTexturePath];

        BackgroundTexture = resourceProviders.Textures[BackgroundTexturePath];

        BannerTextures = new Texture[BannerTexturePaths.Length];
        for (var i = 0; i < BannerTexturePaths.Length; i++)
            BannerTextures[i] = resourceProviders.Textures[BannerTexturePaths[i]];

        PlatformTexture = resourceProviders.Textures[PlatformTexturePath];

        JumpingSfx = resourceProviders.Chunks[JumpingSfxPath];
        AnnouncerClips = new Chunk[AnnouncerClipPaths.Length];
        for (var i = 0; i < AnnouncerClipPaths.Length; i++)
            AnnouncerClips[i] = resourceProviders.Chunks[AnnouncerClipPaths[i]];

        GameplayMusic = resourceProviders.Musics[GameplayMusicPath];
    }

    public Texture GetItemTexture(Item itemType)
    {
        return _itemTextures[(int) itemType];
    }
}