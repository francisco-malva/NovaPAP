using System.Collections.Generic;

namespace DragonGame.Engine.Scenes
{
    internal class SceneManager
    {
        private readonly Stack<Scene> _scenes = new();

        public void Tick()
        {
            _scenes.Peek().OnTick();
        }

        /// <summary>
        /// Remove all scenes from the stack and add the new scene.
        /// </summary>
        public void Set(Scene scene)
        {
            Clear();
            Push(scene);
        }

        /// <summary>
        /// Adds a new scene to the stack, making it the top scene.
        /// </summary>
        public void Push(Scene scene)
        {
            _scenes.Push(scene);
        }

        /// <summary>
        /// Remove the top scene from the stack, unloading it from memory.
        /// </summary>
        private void Pop()
        {
            _scenes.Peek().Dispose();
            _scenes.Pop();
        }

        public void Clear()
        {
            while (_scenes.Count > 0) Pop();
        }
    }
}