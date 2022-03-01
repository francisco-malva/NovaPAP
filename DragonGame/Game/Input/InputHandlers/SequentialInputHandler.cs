using DuckDuckJump.Engine.Utilities;

namespace DuckDuckJump.Game.Input.InputHandlers;

internal class SequentialInputHandler : IInputHandler
{
    private readonly Pair<GameInput>[] _inputs;
    private int _frameIndex;

    public SequentialInputHandler(Pair<GameInput>[] inputs)
    {
        _inputs = inputs;
    }

    public Pair<GameInput> GetGameInput()
    {
        return _inputs[_frameIndex];
    }

    public void AdvanceFrame()
    {
        ++_frameIndex;
    }
}