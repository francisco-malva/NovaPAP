#region

using System.IO;
using System.IO.Compression;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Files;

public static class FileSystem
{
    private static ZipArchive _archive;
    
    public static void Initialize()
    {
        _archive = ZipFile.Open("Assets.zip", ZipArchiveMode.Read);
    }

    public static void Quit()
    {
        _archive.Dispose();
    }

    public static Stream Open(string path)
    {
        path = Path.Combine("Assets", path);
        return _archive.GetEntry(path)?.Open() ?? File.OpenRead(path);
    }
}