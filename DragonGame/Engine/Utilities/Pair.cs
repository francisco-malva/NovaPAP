namespace DuckDuckJump.Engine.Utilities;

/// <summary>
///     A simple structure that houses 2 distinct structures within itself.
/// </summary>
/// <typeparam name="T">The type of value to hold.</typeparam>
internal struct Pair<T>
{
    /// <summary>
    ///     The first value.
    /// </summary>
    public readonly T First;

    /// <summary>
    ///     The second value.
    /// </summary>
    public readonly T Second;

    public Pair(T first, T second)
    {
        First = first;
        Second = second;
    }
}