#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Files;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

#endregion

namespace DuckDuckJump.Engine.Assets;

public class Font : IDisposable
{
    /// <summary>
    ///     Callback function used to modify the transformation of a glyph being drawn in the text drawing routine.
    /// </summary>
    public delegate Matrix3x2 OnTextDraw(string text, int index, Vector2 offset, object context);

    /// <summary>
    ///     Cached glyph textures.
    /// </summary>
    private readonly Dictionary<char, FontGlyph> _glyphs;

    /// <summary>
    ///     Creates a new font object.
    /// </summary>
    /// <param name="path">Path on the virtual filesystem.</param>
    public Font(string path)
    {
        path = Path.Combine("Fonts", $"{path}.fnt");

        using var archive = new ZipArchive(FileSystem.Open(path), ZipArchiveMode.Read);

        _glyphs = new Dictionary<char, FontGlyph>();


        foreach (var entry in archive.Entries)
        {
            var idx = int.Parse(Path.GetFileNameWithoutExtension(entry.Name));

            using var entryStream = entry.Open();

            var data = new byte[entry.Length];

            entryStream.Read(data, 0, data.Length);
            _glyphs.Add((char)idx, new FontGlyph(data));
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
    {
        foreach (var glyph in _glyphs.Values) glyph.Dispose();
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

        public FontGlyph(byte[] data)
        {
            GlyphTexture = new Texture(data, false);
            Info = GlyphTexture.Handle == IntPtr.Zero ? new TextureInfo() : GlyphTexture.QueryTexture();
        }

        public void Dispose()
        {
            GlyphTexture.Dispose();
        }
    }
}