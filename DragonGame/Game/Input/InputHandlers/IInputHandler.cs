using DuckDuckJump.Engine.Utilities;

namespace DuckDuckJump.Game.Input.InputHandlers;

internal interface IInputHandler
{
    Pair<GameInput> GetGameInput();
}