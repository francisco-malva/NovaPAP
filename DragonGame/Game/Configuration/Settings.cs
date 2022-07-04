#region

using System;
using System.IO;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Game.Input;
using SDL2;

#endregion

namespace DuckDuckJump.Game.Configuration;

internal static class Settings
{
    public static Data MyData;

    static unsafe Settings()
    {
        if (!File.Exists("settings.bin"))
        {
            MyData = new Data
            {
                MusicVolume = 0.5f,
                SfxVolume = 0.5f,
                Fullscreen = false
            };

            MyData.InputProfiles[0] = (int)SDL.SDL_Scancode.SDL_SCANCODE_A;
            MyData.InputProfiles[1] = (int)SDL.SDL_Scancode.SDL_SCANCODE_D;
            MyData.InputProfiles[2] = (int)SDL.SDL_Scancode.SDL_SCANCODE_S;
            MyData.InputProfiles[3] = (int)SDL.SDL_Scancode.SDL_SCANCODE_J;
            MyData.InputProfiles[4] = (int)SDL.SDL_Scancode.SDL_SCANCODE_L;
            MyData.InputProfiles[5] = (int)SDL.SDL_Scancode.SDL_SCANCODE_K;
            Save();
        }
        else
        {
            using var stream = File.OpenRead("settings.bin");
            MyData.Load(stream);
        }
    }

    public static void Save()
    {
        using var stream = File.Create("settings.bin");
        MyData.Save(stream);
    }

    public unsafe struct Nickname
    {
        public const byte MaxLength = 16;
        public fixed char Characters[MaxLength];
        public byte Length;

        public override string ToString()
        {
            fixed (char* ptr = Characters)
            {
                var characters = new Span<char>(ptr, Length);
                return new string(characters).Trim();
            }
        }
    }

    public unsafe struct Data
    {
        public const int InputProfileSize = 3;
        public float MusicVolume;
        public float SfxVolume;

        public fixed int InputProfiles[InputProfileSize * Match.PlayerCount];

        public bool NicknameDefined;
        public Nickname Nickname;


        public bool Fullscreen;

        public int GetInputStartingOffset(int player)
        {
            return player * InputProfileSize;
        }

        public GameInput GetInput(int player)
        {
            var input = GameInput.None;

            var offset = GetInputStartingOffset(player);

            for (var i = offset; i < offset + InputProfileSize; i++)
            {
                var idx = i - offset;

                if (Keyboard.KeyHeld((SDL.SDL_Scancode)InputProfiles[i])) input |= (GameInput)(1 << idx);
            }

            return input;
        }

        public void Save(Stream stream)
        {
            fixed (Data* ptr = &this)
            {
                var store = new Span<byte>(ptr, sizeof(Data));
                stream.Write(store);
            }
        }

        public void Load(Stream stream)
        {
            fixed (Data* ptr = &this)
            {
                var store = new Span<byte>(ptr, sizeof(Data));
                stream.Read(store);
            }
        }
    }
}