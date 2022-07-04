#region

using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace Common.Utilities;

public static class StreamExtensions
{
    private static readonly StringBuilder Builder = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Write<T>(this Stream stream, T value) where T : unmanaged
    {
        var ptr = &value;
        var span = new Span<byte>(ptr, sizeof(T));
        stream.Write(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T Read<T>(this Stream stream) where T : unmanaged
    {
        Span<byte> span = stackalloc byte[sizeof(T)];
        stream.Read(span);

        fixed (byte* internalPtr = span)
        {
            return *(T*)internalPtr;
        }
    }

    public static void Write(this Stream stream, Color value)
    {
        stream.Write(value.ToArgb());
    }

    public static Color ReadColor(this Stream stream)
    {
        return Color.FromArgb(stream.Read<int>());
    }

    public static void Write(this Stream stream, string value)
    {
        stream.Write(value.Length + 1);
        foreach (var character in value) stream.Write(character);

        stream.Write('\0');
    }

    public static string ReadString(this Stream stream)
    {
        var length = stream.Read<int>();

        unsafe
        {
            var str = length <= 1024 ? stackalloc byte[length * sizeof(char)] : new byte[length * sizeof(char)];
            stream.Read(str);

            fixed (byte* ptr = str)
            {
                return new string((char*)ptr);
            }
        }
    }
}