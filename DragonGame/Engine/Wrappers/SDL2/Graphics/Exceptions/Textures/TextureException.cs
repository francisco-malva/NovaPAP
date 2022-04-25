#region

using System;

#endregion

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Exceptions.Textures;

public class TextureException : Exception
{
    public TextureException(string message) : base(message)
    {
    }
}