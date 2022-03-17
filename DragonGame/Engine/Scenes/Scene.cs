using System;
using DuckDuckJump.Engine.Assets;

namespace DuckDuckJump.Engine.Scenes;

/// <summary>
///     Class that represents the internal game state.
/// </summary>
internal abstract class Scene : IDisposable
{
    protected readonly ResourceManager ResourceManager;

    protected Scene()
    {
        ResourceManager = new ResourceManager(GameContext.Instance.Renderer);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Runs every frame that the scene is on top of the stack.
    /// </summary>
    public abstract void OnTick();

    /// <summary>
    ///     Runs when the scene gets pushed out of the scene stack.
    /// </summary>
    protected abstract void OnUnload();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        OnUnload();
        ResourceManager.Dispose();
    }
}