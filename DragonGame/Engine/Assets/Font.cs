#region

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using DuckDuckJump.Engine.Subsystems.Files;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Assets;

public class Font : IDisposable
{
    public delegate Matrix3x2 OnTextDraw(string text, int index, Vector2 offset, object context);

    private readonly FontGlyph[] _glyphs;

    public Font(string path, int ptSize)
    {
        path = Path.Combine("Assets", "Fonts", $"{path}.ttf");

        using var stream = FileSystem.Open(path);
        var size = stream.Length;
        var ptr = Marshal.AllocHGlobal((int) size);

        unsafe
        {
            stream.Read(new Span<byte>((void*) ptr, (int) size));
        }

        var rawOps = SDL.SDL_RWFromMem(ptr, (int) size);

        var fontHandle = SDL_ttf.TTF_OpenFontRW(rawOps, 1, ptSize);

        _glyphs = new FontGlyph[256];

        for (var i = 0; i < _glyphs.Length; i++) _glyphs[i] = new FontGlyph(Graphics.Renderer, fontHandle, (char) i);

        Marshal.FreeHGlobal(ptr);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
    {
        foreach (var glyph in _glyphs) glyph.Dispose();
    }

    public void Draw(string text, Matrix3x2 transformation, Color color, OnTextDraw onDraw = null,
        object context = null)
    {
        var stride = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            var glyph = _glyphs[character];

            var offset = new Vector2(stride, 0.0f);
            var matrix = transformation *
                         (onDraw?.Invoke(text, i, offset, context) ?? Matrix3x2.CreateTranslation(stride, 0.0f));
            Graphics.Draw(glyph.GlyphTexture, null, matrix, color);
            stride += glyph.Info.Width;
        }
    }

    public Size MeasureString(string text)
    {
        int w = 0, h = 0;

        foreach (var glyph in text.Select(character => _glyphs[character]))
        {
            w += glyph.Info.Width;
            h = Math.Max(h, glyph.Info.Height);
        }

        return new Size(w, h);
    }

    ~Font()
    {
        ReleaseUnmanagedResources();
    }

    private readonly struct FontGlyph : IDisposable
    {
        public readonly TextureInfo Info;
        public readonly Texture GlyphTexture;

        public FontGlyph(IntPtr renderer, IntPtr fontHandle, char glyph)
        {
            using var surface = new Surface(SDL_ttf.TTF_RenderText_Blended(fontHandle, glyph.ToString(),
                new SDL.SDL_Color {r = 255, g = 255, b = 255, a = 255}));
            GlyphTexture = new Texture(surface);
            Info = GlyphTexture.Handle == IntPtr.Zero ? new TextureInfo() : GlyphTexture.QueryTexture();
        }

        public void Dispose()
        {
            GlyphTexture.Dispose();
        }
    }
}