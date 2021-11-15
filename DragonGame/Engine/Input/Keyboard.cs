using SDL2;

namespace DragonGame.Engine.Input
{
    internal static class Keyboard
    {
        private static readonly unsafe bool* _keyState;

        static unsafe Keyboard()
        {
            _keyState = (bool*)SDL.SDL_GetKeyboardState(out _);
        }

        public static unsafe bool KeyDown(SDL.SDL_Scancode scancode)
        {
            return _keyState[(int)scancode];
        }
    }
}