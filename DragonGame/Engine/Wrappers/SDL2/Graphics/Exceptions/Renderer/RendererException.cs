using System;

namespace DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Exceptions.Renderer;

public class RendererException : Exception
{
    public RendererException(string message) : base(message)
    {
    }
}