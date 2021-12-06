using System;

namespace DuckDuckJump.Engine.Scenes;

internal abstract class Scene : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Runs every frame.
    /// </summary>
    public abstract void OnTick();

    /// <summary>
    ///     Runs when the scene gets pushed out of the scene stack.
    protected abstract void OnUnload();

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) OnUnload();
    }
}
