#region

using System;
using System.IO;
using DuckDuckJump.Engine.Subsystems.Files;

#endregion

namespace DuckDuckJump.Engine.Assets;

public class AudioClip : IDisposable
{
    public AudioClip(string path, bool streaming = false)
    {
        path = Path.Combine("Audio", $"{path}.snd");
        if (!streaming)
        {
            using var file = FileSystem.Open(path);
            var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            Stream = memoryStream;
        }
        else
        {
            Stream = FileSystem.Open(path);
        }
    }

    public Stream Stream { get; }
    public bool Closed { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Closed = true;
        Stream?.Dispose();
    }
}