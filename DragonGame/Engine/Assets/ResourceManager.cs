using System;
using System.Collections.Generic;

namespace DragonGame.Engine.Assets
{
    internal abstract class ResourceManager<T> : IDisposable where T : IDisposable
    {
        private readonly string _fileExtension;
        private readonly string _rootPath;
        private readonly Dictionary<string, T> _assetCache = new();

        internal ResourceManager(string rootPath, string fileExtension)
        {
            _rootPath = rootPath;
            _fileExtension = fileExtension;
        }

        public T this[string name]
        {
            get
            {
                if (_assetCache.ContainsKey(name)) return _assetCache[name];

                var asset = LoadAsset($"{_rootPath}/{name}.{_fileExtension}");
                _assetCache.Add(name, asset);
                return asset;
            }
        }

        public void Dispose()
        {
            ClearCache();
        }

        public void ClearCache()
        {
            foreach (var pair in _assetCache) pair.Value.Dispose(); //Unload all cached assets
        }

        protected abstract T LoadAsset(string path);
    }
}