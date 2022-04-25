#region

using System;

#endregion

namespace DuckDuckJump.Engine.Exceptions;

public class GameInitException : Exception
{
    public GameInitException(string message) : base(message)
    {
    }
}