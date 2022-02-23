using System.Collections.Generic;
using System.IO;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Scenes.Game.Input;

namespace DuckDuckJump.Scenes.Game.Local;

internal class Replay
{
    public readonly List<Pair<GameInput>> _gameInput;
    public readonly GameInfo Info;
    private int _index;

    public Replay(GameInfo info)
    {
        Info = info;
        _gameInput = new List<Pair<GameInput>>();
    }

    public Replay(BinaryReader reader)
    {
        Info = new GameInfo(reader);
        _gameInput = new List<Pair<GameInput>>(reader.ReadInt32());

        for (var i = 0; i < _gameInput.Capacity; i++)
            _gameInput.Add(new Pair<GameInput>((GameInput) reader.ReadByte(), (GameInput) reader.ReadByte()));
    }

    public void Enqueue(Pair<GameInput> input)
    {
        _gameInput.Add(input);
    }

    public Pair<GameInput> Get()
    {
        return _index > _gameInput.Count ? new Pair<GameInput>(GameInput.None, GameInput.None) : _gameInput[_index++];
    }


    public void Save(BinaryWriter writer)
    {
        Info.Save(writer);

        writer.Write(_gameInput.Count);

        foreach (var input in _gameInput)
        {
            writer.Write((byte) input.First);
            writer.Write((byte) input.Second);
        }
    }
}