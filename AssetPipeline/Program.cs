using AssetPipeline;
using ImageConversion;
using StbImageSharp;

var destination = args[0] + "(temp)";

if (Directory.Exists(destination))
    Directory.Delete(destination, true);

Directory.CreateDirectory(destination);

Utilities.CopyDirectory(args[0], destination, true);

var files = Utilities.GetAllFilesInDirectory(destination);

Console.WriteLine("Files:");
files.ForEach(f => Console.WriteLine(f));
Console.WriteLine();

foreach (var info in files.Select(file => new FileInfo(file)))
{
    switch (info.Extension)
    {
        case ".png":
            WriteTex(info);
            Console.WriteLine($"Write Texture: {info.FullName}");
            break;
        case ".ico":
            Console.WriteLine($"Delete icon: {info.FullName}");
            break;
        default:
            Console.WriteLine($"Unknown file type: {info.FullName}");
            break;
    }
    info.Delete();
}

void WriteTex(FileInfo info)
{
    using var inputFile = File.OpenRead(info.FullName);
    var result = ImageResult.FromStream(inputFile);
    using var fileStream =
        File.OpenWrite(Path.Join(info.Directory.FullName, Path.GetFileNameWithoutExtension(info.Name) + ".tex"));
    Converter.ResultToTexStream(result, fileStream);
}