using SDL2;

namespace DragonGame.Wrappers
{
    public struct Rectangle
    {
        public SDL.SDL_Rect Value;

        public Rectangle(int x, int y, int w, int h)
        {
            Value = new SDL.SDL_Rect { x = x, y = y, w = w, h = h };
        }

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

        public int W
        {
            get => Value.w;
            set => Value.w = value;
        }

        public int H
        {
            get => Value.h;
            set => Value.h = value;
        }

        public bool HasIntersection(ref Rectangle other)
        {
            return SDL.SDL_HasIntersection(ref Value, ref other.Value) == SDL.SDL_bool.SDL_TRUE;
        }

        public bool HasPoint(ref Point point)
        {
            return SDL.SDL_PointInRect(ref point.Value, ref Value) == SDL.SDL_bool.SDL_TRUE;
        }

        public bool HasLine(ref Point a, ref Point b)
        {
            int x1 = a.X, y1 = a.Y, x2 = b.X, y2 = b.Y;
            return SDL.SDL_IntersectRectAndLine(ref Value, ref x1, ref y1, ref x2, ref y2) == SDL.SDL_bool.SDL_TRUE;
        }
    }
}