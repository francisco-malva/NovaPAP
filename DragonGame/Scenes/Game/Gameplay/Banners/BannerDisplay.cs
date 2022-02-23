using System.Diagnostics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;

namespace DuckDuckJump.Scenes.Game.Gameplay.Banners;

internal class BannerDisplay
{
    private readonly Texture[] _bannerTextures;

    private ushort _bannerDuration;

    private ushort _bannerTimer;

    private BannerType _currentBanner;

    public BannerDisplay(GameplayResources resources)
    {
        _bannerTextures = resources.BannerTextures;
    }

    private Texture BannerTexture => _bannerTextures[(int) _currentBanner];
    private TextureInfo BannerTextureInfo => BannerTexture.QueryTexture();

    public void Raise(BannerType type, ushort duration)
    {
        _currentBanner = type;
        _bannerTimer = duration;
        _bannerDuration = duration;
    }


    public void Update()
    {
        if (_bannerTimer > 0) --_bannerTimer;
    }

    public void Draw()
    {
        if (_bannerTimer == 0) return;

        var textureInfo = BannerTextureInfo;

        BannerTexture.SetAlphaMod((byte) (_bannerTimer / (float) _bannerDuration * 255.0f));
        Debug.Assert(Engine.Game.Instance != null, "Engine.Game.Instance != null");
        Engine.Game.Instance.Renderer.Copy(BannerTexture, null,
            new Rectangle(GameField.Width / 2 - textureInfo.Width / 2,
                GameField.Height / 2 - textureInfo.Height / 2, textureInfo.Width, textureInfo.Height));
        --_bannerTimer;
    }
}