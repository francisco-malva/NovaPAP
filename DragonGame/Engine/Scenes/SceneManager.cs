using System.Collections.Generic;

namespace DuckDuckJump.Engine.Scenes;

internal class SceneManager
{
    /// <summary>
    ///     The scene stack.
    /// </summary>
    private readonly Stack<Scene> _scenes = new();

    public void Tick()
    {
        _scenes.Peek().OnTick();
    }

    /// <summary>
    ///     Remove all scenes from the stack and add the new scene.
    /// </summary>
    public void Set(Scene scene)
    {
        Clear();
        Push(scene);
    }

    /// <summary>
    ///     Adds a new scene to the stack, making it the top scene.
    /// </summary>
    private void Push(Scene scene)
    {
        _scenes.Push(scene);
    }

    /// <summary>
    ///     Remove and unload the scene on top of the stack.
    /// </summary>
    private void Pop()
    {
        _scenes.Peek().Dispose();
        _scenes.Pop();
    }

    /// <summary>
    ///     Remove all scenes from the stack.
    /// </summary>
    public void Clear()
    {
        while (_scenes.Count > 0) Pop();
    }
}