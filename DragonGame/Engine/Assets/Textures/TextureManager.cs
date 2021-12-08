using System.IO;
using DuckDuckJump.Engine.Wrappers.SDL2;
using StbImageSharp;

namespace DuckDuckJump.Engine.Assets.Textures
{
    internal class TextureManager : ResourceManager<Texture>
    {
        public TextureManager() : base("Assets/Textures", "bmp")
        {
        }

        protected override Texture LoadAsset(string path)
        {
            using (var file = File.OpenRead(path))
            {
                using (var surface = new Surface(ImageResult.FromStream(file, ColorComponents.RedGreenBlueAlpha)))
                {
                    return new Texture(Game.Instance.Renderer, surface);
                }
            }
        }
    }
}