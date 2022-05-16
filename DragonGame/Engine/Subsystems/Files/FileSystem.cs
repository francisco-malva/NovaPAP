#region

using System.IO;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Files;

public static class FileSystem
{
    public static void Initialize()
    {
    }

    public static void Quit()
    {
    }

    public static Stream Open(string path)
    {
        return File.OpenRead($"Assets\\{path}");
    }

    public static byte[] GetAllBytes(string path)
    {
        using var stream = Open(path);

        var data = new byte[stream.Length];

        stream.Read(data, 0, data.Length);

        return data;
    }
}