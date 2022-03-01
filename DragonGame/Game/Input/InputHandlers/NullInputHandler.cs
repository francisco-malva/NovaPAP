using DuckDuckJump.Engine.Utilities;

namespace DuckDuckJump.Game.Input.InputHandlers;

internal class NullInputHandler : IInputHandler
{
    public Pair<GameInput> GetGameInput()
    {
        return new Pair<GameInput>(GameInput.None, GameInput.None);
    }
}