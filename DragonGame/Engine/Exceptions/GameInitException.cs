using System;

namespace DuckDuckJump.Engine.Exceptions;

public class GameInitException : Exception
{
    public GameInitException(string message) : base(message)
    {
    }
}