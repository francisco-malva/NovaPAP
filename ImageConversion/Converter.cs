using Common.Utilities;
using StbImageSharp;

namespace ImageConversion;

public static class Converter
{
    public static void ResultToTexStream(ImageResult result, Stream stream)
    {
        stream.Write(result.Width);
        stream.Write(result.Height);
        stream.Write(result.Data);
    }
}