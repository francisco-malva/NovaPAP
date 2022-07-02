#region

using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

#endregion

namespace DuckDuckJump.Game.Assets;

public static class MatchAssets
{
    public enum FontIndex : byte
    {
        BannerFont,
        TimerFont
    }

    public enum SfxIndex : byte
    {
        None,
        Jump,
        ItemPop
    }

    public enum TextureIndex : byte
    {
        Sky,
        Player,
        Platform,
        ScoreIcon,
        FinishLine,
        ItemBox,
        ItemFrame,
        DoubleJumpItem,
        FreezeItem,
        SlowdownItem,
        ReviveItem,
        UmbrellaItem,
        PlayerAi,
        Player2,
        PlayerAi2
    }

    private static readonly FontData[] FontDatum =
    {
        new("terminator-two-70", 70),
        new("terminator-two-40", 40)
    };

    private static readonly string[] TexturePaths =
    {
        "Game/Backgrounds/sky",
        "Game/Field/player",
        "Game/Field/platform",
        "Game/Scoreboard/checkmark",
        "Game/Field/finish-line",
        "Game/Items/item-box",
        "Game/Items/item-ui",
        "Game/Items/double-jump",
        "Game/Items/freeze",
        "Game/Items/slowdown",
        "Game/Items/revive",
        "Game/Items/umbrella",
        "Game/Field/player-ai",
        "Game/Field/player-2",
        "Game/Field/player-ai-2"
    };

    private static readonly string[] SfxPaths =
    {
        null,
        "Game/Player/jump",
        "Game/Player/item-pop"
    };

    private static readonly Texture[] Textures = new Texture[TexturePaths.Length];
    private static readonly Font[] Fonts = new Font[FontDatum.Length];
    private static readonly AudioClip[] SoundEffects = new AudioClip[SfxPaths.Length];

    private static bool _loaded;

    public static void Load()
    {
        if (_loaded)
            return;

        for (var i = 0; i < Textures.Length; i++) Textures[i] = new Texture(TexturePaths[i]);
        for (var i = 0; i < Fonts.Length; i++) Fonts[i] = new Font(FontDatum[i].Path);
        for (var i = 1; i < SoundEffects.Length; i++) SoundEffects[i] = new AudioClip(SfxPaths[i]);

        _loaded = true;
    }

    public static Texture Texture(TextureIndex textureIndex)
    {
        return Textures[(int)textureIndex];
    }

    public static Font Font(FontIndex fontIndex)
    {
        return Fonts[(int)fontIndex];
    }

    public static AudioClip SoundEffect(SfxIndex sfxIndex)
    {
        return SoundEffects[(int)sfxIndex];
    }

    public static void Unload()
    {
        if (!_loaded)
            return;
        foreach (var texture in Textures) texture.Dispose();
        foreach (var font in Fonts) font.Dispose();
        foreach (var clip in SoundEffects) clip?.Dispose();
        _loaded = false;
    }

    private readonly struct FontData
    {
        public readonly string Path;
        public readonly int Size;

        public FontData(string path, int size)
        {
            Path = path;
            Size = size;
        }
    }
}