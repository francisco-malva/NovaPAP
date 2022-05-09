#region

using System;
using System.IO;
using DuckDuckJump.Engine.Subsystems.Files;

#endregion

namespace DuckDuckJump.Engine.Assets;

public class AudioClip : IDisposable
{
    private readonly bool _streaming;
    private readonly string _tempFilePath;
    
    /// <summary>
    /// Constructs a new audio clip (file must be stereo, 48000HZ and use a signed 16-bit value per audio channel).
    /// </summary>
    /// <param name="path">Path in the virtual filesystem to the file.</param>
    /// <param name="streaming">Is the file being streamed? If not, the PCM data will be copied to memory completely before being read, occupying more memory but being faster. Use streaming only with long files (for example, music tracks).</param>
    public AudioClip(string path, bool streaming = false)
    {
        _streaming = streaming;
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
            using var tempStream = FileSystem.Open(path);

            _tempFilePath = Path.GetTempFileName();
            
            Stream = File.Open(_tempFilePath, FileMode.Truncate, FileAccess.ReadWrite);
            tempStream.CopyTo(Stream);
            Stream.Seek(0, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// The opened audio clip stream.
    /// </summary>
    public Stream Stream { get; }
    /// <summary>
    /// Has the audio clip been disposed?
    /// </summary>
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Disposed = true;

        Stream?.Dispose();
        if (_streaming)
        { 
            File.Delete(_tempFilePath);
        }
        
    }
}