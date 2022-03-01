using System;
using System.Collections.Generic;
using System.IO;

namespace DuckDuckJump.Engine.Assets.Providers;

/// <summary>
///     An abstract class used when one wants to implement resource loading.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ResourceProvider<T> : IDisposable where T : IDisposable
{
    /// <summary>
    ///     Base path for all assets.
    /// </summary>
    private const string RootFolder = "Assets";

    /// <summary>
    ///     Cache where assets already accessed are kept (as to not be loaded more than once).
    /// </summary>
    private readonly Dictionary<string, T> _assetCache = new();

    /// <summary>
    ///     File extension for the resource.
    /// </summary>
    private readonly string _fileExtension;

    /// <summary>
    ///     Path where the resources will be loaded from.
    /// </summary>
    private readonly string _rootPath;

    internal ResourceProvider(string rootPath, string fileExtension)
    {
        _rootPath = Path.Combine(RootFolder, rootPath);
        _fileExtension = fileExtension;
    }

    public T this[string name]
    {
        get
        {
            if (_assetCache.ContainsKey(name)) return _assetCache[name];

            var path = $"{_rootPath}/{name}.{_fileExtension}";

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Could not load {typeof(T).Name} because '{path} points to a non existent file.'");
            var asset = LoadAsset($"{_rootPath}/{name}.{_fileExtension}");
            _assetCache.Add(name, asset);
            return asset;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Unloads a resource from memory (all handles pertaining to it will be invalidated).
    /// </summary>
    /// <param name="name"></param>
    public void Unload(string name)
    {
        var asset = this[name];

        asset.Dispose();
        _assetCache.Remove(name);
    }

    /// <summary>
    ///     Unloads all cached assets, invalidating all their handles.
    /// </summary>
    private void UnloadAll()
    {
        foreach (var pair in _assetCache) pair.Value.Dispose();
        _assetCache.Clear();
    }

    /// <summary>
    ///     Load the requested file from the filesystem.
    /// </summary>
    /// <param name="path">Actual path to the file.</param>
    /// <returns>The handle to the loaded file.</returns>
    protected abstract T LoadAsset(string path);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) UnloadAll();
    }
}