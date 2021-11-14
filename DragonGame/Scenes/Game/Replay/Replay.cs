using System;
using System.Collections.Generic;
using System.IO;
using DragonGame.Engine.Rollback;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Input;

namespace DragonGame.Scenes.Game.Replay
{
    internal class Replay
    {
        private readonly SortedDictionary<ulong, ReplayInput> _inputs = new();
        private byte[] _initialState;
        private byte[] _randomBytes;

        public Replay(GameScene scene, DeterministicRandom random)
        {
            SaveInitialState(scene);

            _randomBytes = random.GetInternalArray();
        }

        public Replay(BinaryReader reader)
        {
            _randomBytes = reader.ReadBytes(reader.Read());
            _initialState = reader.ReadBytes(reader.Read());
            for (var i = 0; i < reader.ReadInt32(); i++) _inputs.Add(reader.ReadUInt64(), new ReplayInput(reader));
        }

        public void Setup(DeterministicRandom random, GameScene scene)
        {
            random.SetInternalArray(ref _randomBytes);

            using var initialState = new StateBuffer(new Span<byte>(_initialState));
            scene.Rollback(initialState);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_randomBytes.Length);
            writer.Write(_randomBytes);
            writer.Write(_initialState.Length);
            writer.Write(_initialState);

            writer.Write(_inputs.Count);
            foreach (var (frame, input) in _inputs)
            {
                writer.Write(frame);
                input.Write(writer);
            }
        }

        private void SaveInitialState(GameScene scene)
        {
            using var buffer = new StateBuffer();
            scene.Save(buffer);
            _initialState = buffer.AsSpan().ToArray();
        }

        public void RegisterInput(ulong frame, GameInput p1Input, GameInput p2Input)
        {
            if (!_inputs.TryAdd(frame, new ReplayInput(p1Input, p2Input)))
                _inputs[frame] = new ReplayInput(p1Input, p2Input);
        }

        public void FetchInput(ulong frame, ref GameInput p1Input, ref GameInput p2Input)
        {
            if (!_inputs.ContainsKey(frame)) return;

            var input = _inputs[frame];

            p1Input = input.P1Input;
            p2Input = input.P2Input;
        }
    }
}