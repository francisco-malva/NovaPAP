using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using DuckDuckJump.Engine.Subsystems.Files;
using DuckDuckJump.Engine.Subsystems.Output;
using SDL2;

namespace DuckDuckJump.Engine.Subsystems.Auditory;

internal static class Audio
{
    private static bool _running = true;
    private static Thread _audioThread;
    
    public static void Initialize()
    {
        _audioThread = new Thread(AudioLoop);
        _audioThread.Start();
    }

    private static unsafe void AudioLoop()
    {
        using var stream = FileSystem.Open("Assets/test.raw");
        
        var spec = new SDL.SDL_AudioSpec()
        {
            callback = null,
            userdata = IntPtr.Zero,
            format = SDL.AUDIO_S16,
            samples = 512,
            freq = 44100,
            channels = 2,
        };

        SDL.SDL_Init(SDL.SDL_INIT_AUDIO);
        
        var device = SDL.SDL_OpenAudioDevice(IntPtr.Zero, 0, ref spec, out var obtained, 0);
        
        
        if(device == 0)
            Error.Panic($"Audio device not initialized. SDL Error: {SDL.SDL_GetError()}");
        
        var data = stackalloc short[512];
        var span = new Span<byte>(data, sizeof(short) * 512);
        
        SDL.SDL_PauseAudioDevice(device, 0);
        
        while (_running)
        {
            while (SDL.SDL_GetQueuedAudioSize(device) < 1024 * sizeof(short))
            {
                if (stream.Read(span) == 0)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(span);
                }
            
                SDL.SDL_QueueAudio(device, (IntPtr) data, 512 * sizeof(short));
            }
            Thread.Sleep(0);
        }
    }
    
    public static void Quit()
    {
        _running = false;
        _audioThread.Join();
    }
}