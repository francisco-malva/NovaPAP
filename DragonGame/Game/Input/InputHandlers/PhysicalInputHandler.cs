using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Utilities;
using SDL2;

namespace DuckDuckJump.Game.Input.InputHandlers;

internal class PhysicalInputHandler : IInputHandler
{
    private readonly InputProfile _p1InputProfile;
    private readonly InputProfile _p2InputProfile;

    public PhysicalInputHandler(InputProfile p1InputProfile, InputProfile p2InputProfile)
    {
        _p1InputProfile = p1InputProfile;
        _p2InputProfile = p2InputProfile;
    }

    public Pair<GameInput> GetGameInput()
    {
        return new Pair<GameInput>(_p1InputProfile.GetTranslatedInput(), _p2InputProfile.GetTranslatedInput());
    }
}

internal readonly struct InputProfile
{
    private readonly SDL.SDL_Scancode _leftKey;
    private readonly SDL.SDL_Scancode _rightKey;
    private readonly SDL.SDL_Scancode _specialKey;
    private readonly SDL.SDL_Scancode _pauseKey;

    public InputProfile(SDL.SDL_Scancode leftKey, SDL.SDL_Scancode rightKey, SDL.SDL_Scancode specialKey,
        SDL.SDL_Scancode pauseKey)
    {
        _leftKey = leftKey;
        _rightKey = rightKey;
        _specialKey = specialKey;
        _pauseKey = pauseKey;
    }

    public GameInput GetTranslatedInput()
    {
        var input = GameInput.None;

        if (Keyboard.KeyHeld(_leftKey))
            input |= GameInput.Left;
        if (Keyboard.KeyHeld(_rightKey))
            input |= GameInput.Right;
        if (Keyboard.KeyDown(_specialKey))
            input |= GameInput.Special;
        if (Keyboard.KeyDown(_pauseKey))
            input |= GameInput.Pause;

        return input;
    }
}