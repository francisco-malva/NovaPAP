using System.IO;
using DragonGame.Scenes.Game.Input;

namespace DragonGame.Scenes.Game.Replay
{
    internal readonly struct ReplayInput
    {
        public readonly GameInput P1Input, P2Input;

        public ReplayInput(GameInput p1Input, GameInput p2Input)
        {
            P1Input = p1Input;
            P2Input = p2Input;
        }

        public ReplayInput(BinaryReader reader)
        {
            P1Input = (GameInput)reader.ReadByte();
            P2Input = (GameInput)reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)P1Input);
            writer.Write((byte)P2Input);
        }
    }
}