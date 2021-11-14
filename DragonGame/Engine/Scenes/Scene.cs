using System;

namespace DragonGame.Engine.Scenes
{
    internal abstract class Scene : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void OnTick();
        protected abstract void OnUnload();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) OnUnload();
        }
    }
}