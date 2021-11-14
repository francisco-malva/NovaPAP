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

        public void Set(Scene scene)
        {
            Clear();
            Push(scene);
        }

        public void Push(Scene scene)
        {
            _scenes.Push(scene);
        }

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