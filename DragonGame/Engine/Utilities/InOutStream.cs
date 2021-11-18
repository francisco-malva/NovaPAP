using System;
using System.IO;
using System.Text;

namespace DragonGame.Engine.Utilities
{
    public sealed class InOutStream : IDisposable
    {
        public readonly BinaryWriter Writer;
        public readonly BinaryReader Reader;
        public readonly Stream Stream;

        public InOutStream(Stream stream, Encoding encoding, bool leaveOpen = false)
        {
            Stream = stream;
            Writer = new BinaryWriter(Stream, encoding, leaveOpen);
            Reader = new BinaryReader(Stream, encoding, leaveOpen);
        }

        public void Dispose()
        {
            Writer.Dispose();
            Reader.Dispose();
            Stream.Dispose();
        }
    }
}
