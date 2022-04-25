#region

using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    public static class Assets
    {
        public enum FontIndex
        {
            BannerFont,
            TimerFont
        }

        public enum TextureIndex
        {
            Sky,
            Player,
            Platform
        }

        private static readonly FontData[] FontDatas =
        {
            new("terminator-two", 70),
            new("terminator-two", 40)
        };

        private static readonly string[] TexturePaths =
        {
            "Game/Backgrounds/sky",
            "Game/Field/player",
            "Game/Field/platform"
        };

        private static readonly Texture[] Textures = new Texture[TexturePaths.Length];
        private static readonly Font[] Fonts = new Font[FontDatas.Length];

        public static void Load()
        {
            for (var i = 0; i < Textures.Length; i++) Textures[i] = new Texture(TexturePaths[i]);
            for (var i = 0; i < Fonts.Length; i++) Fonts[i] = new Font(FontDatas[i].Path, FontDatas[i].Size);
        }

        public static Texture Texture(TextureIndex textureIndex)
        {
            return Textures[(int) textureIndex];
        }

        public static Font Font(FontIndex fontIndex)
        {
            return Fonts[(int) fontIndex];
        }

        public static void Unload()
        {
            foreach (var texture in Textures) texture.Dispose();
            foreach (var font in Fonts) font.Dispose();
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
}