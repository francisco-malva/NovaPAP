using System;

namespace DuckDuckJump.Engine.Utilities;

/// <summary>
///     A random class that always returns the same values under the same conditions, also supports saving and loading its
///     state.
/// </summary>
internal class DeterministicRandom
{
    /// <summary>
    ///     The array of random bytes used to give out the random values.
    /// </summary>
    private readonly byte[] _randomBytes = new byte[4096];

    /// <summary>
    ///     Pointer to the next element to fetch in the random byte ID.
    /// </summary>
    private int _randomPtr;

    /// <summary>
    ///     Generate array of random numbers with the specified seed [NEED TO CALL BEFORE USING!]
    /// </summary>
    public void Setup(int seed)
    {
        var random = new Random(seed);
        random.NextBytes(_randomBytes);
    }

    /// <returns>A random number between 0 and byte.MaxValue</returns>
    public byte GetByte()
    {
        var value = _randomBytes[_randomPtr++];

        if (_randomPtr == _randomBytes.Length) _randomPtr = 0;

        return value;
    }

    /// <returns>Gets 4 random bytes and ORs them together to fill a 4 byte value.</returns>
    private uint ComposeInteger()
    {
        return (uint)(GetByte() | (GetByte() << 8) | (GetByte() << 16) | (GetByte() << 24));
    }

    ///
    public int GetInteger(int min, int max)
    {
        return (int)(min + MathF.Floor(GetFloat() * (max - min)));
    }

    public int GetInteger(int max)
    {
        return GetInteger(0, max);
    }

    /// <summary>
    ///     Returns a float between 0,0 and 1,0.
    /// </summary>
    /// <returns></returns>
    public float GetFloat()
    {
        var value = ComposeInteger() / (float)uint.MaxValue;
        return value;
    }
}