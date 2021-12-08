using System;
using DuckDuckJump.Engine.Wrappers.SDL2;

namespace DuckDuckJump.Engine.Utilities
{
    internal static class UI
    {
        private static readonly Texture _font;
        private static readonly Renderer _renderer;

        static UI()
        {
            _font = Game.Instance.TextureManager["UI/font"];
            _renderer = Game.Instance.Renderer;
        }


        public static void DrawText(Point point, Color color, string text)
        {
            _font.SetColorMod(color);
            _font.SetAlphaMod(color.A);

            var x = point.X;
            var y = point.Y;

            foreach (var character in text)
                switch (character)
                {
                    case ' ':
                        x += 8;
                        break;
                    case '\n':
                        x = point.X;
                        y += 8;
                        break;
                    default:
                        var xCoord = (int) (character * 8.0f % 128.0f);
                        var yCoord = (int) (MathF.Floor(character / 8.0f) * 8.0f);

                        _renderer.Copy(_font, new Rectangle(xCoord, yCoord, 8, 8), new Rectangle(x, y, 8, 8));
                        break;
                }
        }
    }
}