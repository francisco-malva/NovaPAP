using System;
using System.IO;
using System.Text;

namespace DuckDuckJump.Engine.Utilities;

/// <summary>
///     An utility class to wrap both a BinaryReader and BinaryWriter around a stream, to enable both reading and writing.
/// </summary>
public sealed class StreamReaderWriter : IDisposable
{
    public readonly BinaryReader Reader;
    public readonly Stream Stream;
    public readonly BinaryWriter Writer;

    public StreamReaderWriter(Stream stream, Encoding encoding, bool leaveOpen = false)
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