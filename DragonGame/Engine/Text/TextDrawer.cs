using System;
using System.Collections.Generic;
using System.Drawing;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Engine.Wrappers.SDL2.TTF;

namespace DuckDuckJump.Engine.Text;

internal sealed class TextDrawer : IDisposable
{
    private readonly Renderer _renderer;
    private readonly Dictionary<char, CachedSymbol> _symbolCache;

    public TextDrawer(Font font, Renderer renderer, int ptSize, IEnumerable<char> characterSet)
    {
        font.Size = ptSize;
        _renderer = renderer;

        _symbolCache = new Dictionary<char, CachedSymbol>();
        foreach (var character in characterSet)
        {
            using var surface = font.RenderTextBlended(character.ToString(), Color.White);
            _symbolCache.Add(character, new CachedSymbol(new Texture(renderer, surface)));
        }
    }

    public void DrawText(int x, int y, IEnumerable<char> characterSet, Color color)
    {
        var drawX = x;
        foreach (var character in characterSet)
        {
            var symbol = _symbolCache[character];
            
            symbol.Texture.SetColorMod(color);
            _renderer.Copy(symbol.Texture, null, new Rectangle(drawX, y, symbol.Info.Width, symbol.Info.Height));

            drawX += symbol.Info.Width;
        }
    }

    public Size MeasureText(IEnumerable<char> characterSet)
    {
        var x = 0;
        var y = 0;

        foreach (var character in characterSet)
        {
            if (!_symbolCache.TryGetValue(character, out var symbol) && symbol == null) continue;
            x += symbol.Info.Width;
            y = Math.Max(y, symbol.Info.Height);
        }

        return new Size(x, y);
    }

    private sealed class CachedSymbol : IDisposable
    {
        public readonly TextureInfo Info;
        public readonly Texture Texture;

        public CachedSymbol(Texture texture)
        {
            Texture = texture;
            Info = texture.QueryTexture();
        }

        public void Dispose()
        {
            Texture.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var cachedSymbol in _symbolCache)
        {
            cachedSymbol.Value.Dispose();
        }
    }
}