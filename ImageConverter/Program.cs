// See https://aka.ms/new-console-template for more information

using Common.Utilities;
using DuckDuckJump.Engine.Utilities;
using StbImageSharp;

try
{
    if (args.Length == 0)
    {
        throw new ArgumentException("No arguments!");
    }
    foreach (var path in args)
    {
        using var inputFile = File.OpenRead(path);
        var outputFile = File.OpenWrite(Path.GetFileNameWithoutExtension(path) + ".tex");

        var result = ImageResult.FromStream(inputFile, ColorComponents.RedGreenBlueAlpha);
        
        outputFile.Write(result.Width);
        outputFile.Write(result.Height);
        outputFile.Write(result.Data);
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.ReadKey();
}


