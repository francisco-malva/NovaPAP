using DragonGame.Engine.Wrappers.SDL2;

namespace DragonGame.Engine.Assets
{
    internal class TextureManager : ResourceManager<Texture>
    {
        public TextureManager() : base("Assets/Textures", "bmp")
        {
        }

        protected override Texture LoadAsset(string path)
        {
            return Texture.FromBmp(path);
        }
    }
}