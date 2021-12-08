using SDL2;

namespace DuckDuckJump.Engine.Wrappers.SDL2
{
    public struct Point
    {
        public SDL.SDL_Point Value;

        public static readonly Point One = new(1, 1);

        public int X
        {
            get => Value.x;
            set => Value.x = value;
        }

        public int Y
        {
            get => Value.y;
            set => Value.y = value;
        }

        public Point(int x, int y)
        {
            Value = new SDL.SDL_Point { x = x, y = y };
        }
    }
}