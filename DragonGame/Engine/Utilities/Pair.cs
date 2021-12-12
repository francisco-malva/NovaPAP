namespace DuckDuckJump.Engine.Utilities
{
    internal struct Pair<T>
    {
        public readonly T First;
        public readonly T Second;

        public Pair(T first, T second)
        {
            First = first;
            Second = second;
        }
    }
}