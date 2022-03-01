using DuckDuckJump.Engine.Wrappers.SDL2.TTF;

namespace DuckDuckJump.Engine.Assets.Providers.Fonts;

internal class FontProvider : ResourceProvider<Font>
{
    private const string RootPath = "Fonts";
    private const string FileExtension = "ttf";

    public FontProvider() : base(RootPath, FileExtension)
    {
    }

    protected override Font LoadAsset(string path)
    {
        return new Font(path);
    }
}