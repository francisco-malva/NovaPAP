using System;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Mixer.Exceptions;

public class ChunkException : Exception
{
    public ChunkException(string message) : base(message)
    {
    }
}