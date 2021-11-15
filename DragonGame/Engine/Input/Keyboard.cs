namespace DragonGame.Engine.Input
{
    internal static class Keyboard
    {

        private static unsafe bool* _keyState;

        static unsafe Keyboard()
        {
            _keyState = (bool*)SDL2.SDL.SDL_GetKeyboardState(out _);
        }

        public static unsafe bool KeyDown(SDL2.SDL.SDL_Scancode scancode)
        {
            return _keyState[(int)scancode];
        }
    }
}