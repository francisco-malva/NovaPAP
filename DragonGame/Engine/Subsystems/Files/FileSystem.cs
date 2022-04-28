﻿#region

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
        path = Path.Combine("Assets", path);
        return File.OpenRead(path);
    }
}