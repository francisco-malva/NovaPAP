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
                        x += 5;
                        break;
                    case '\n':
                        x = point.X;
                        y += 12;
                        break;
                    default:
                        var chara = character - 32;
                        var xCoord = chara % 20;
                        var yCoord = chara / 20;

                        var src = new Rectangle(xCoord * 5, yCoord * 12, 5, 12);
                        _renderer.Copy(_font, src, new Rectangle(x, y, 5, 12));
                        x += 5;
                        break;
                }
        }
    }
}