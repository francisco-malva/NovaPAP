#region

using System;
using System.IO;
using System.IO.Compression;
using SDL2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#endregion

unsafe
{
    if (SDL_ttf.TTF_Init() != 0) throw new Exception($"Could not initialize SDL_ttf. {SDL_ttf.TTF_GetError()}");

    var font = SDL_ttf.TTF_OpenFont(args[0], int.Parse(args[1]));

    Console.WriteLine($"{SDL_ttf.TTF_GetError()}");

    using var fileStream = File.Create(Path.GetFileNameWithoutExtension(args[0]) + ".fnt");

    using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);

    for (var i = char.MinValue; i < 255; i++)
    {
        var renderedSurface =
            SDL_ttf.TTF_RenderText_Blended(font, $"{i}", new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 });

        if (renderedSurface == IntPtr.Zero) continue;

        var entry = zipArchive.CreateEntry($"{i}.png", CompressionLevel.Optimal);

        using var entryStream = entry.Open();


        var convertedSurface =
            (SDL.SDL_Surface*)SDL.SDL_ConvertSurfaceFormat(renderedSurface, SDL.SDL_PIXELFORMAT_RGBA8888, 0);
        var span = new Span<byte>((void*)convertedSurface->pixels, convertedSurface->w * convertedSurface->h * 4);

        var data = span.ToArray();

        using var image = Image.LoadPixelData<Rgba32>(data, convertedSurface->w, convertedSurface->h);

        image.SaveAsPng(entryStream);
        SDL.SDL_FreeSurface(renderedSurface);
        SDL.SDL_FreeSurface((IntPtr)convertedSurface);
    }

    SDL_ttf.TTF_CloseFont(font);
    SDL_ttf.TTF_Quit();
}