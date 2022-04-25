namespace DuckDuckJump.Game;

internal unsafe struct ComLevels
{
#pragma warning disable CS0649
    private fixed byte _values[Match.PlayerCount];
#pragma warning restore CS0649
    private readonly ushort _comSum;

    public byte this[int i]
    {
        get => _values[i];
        private init => _values[i] = value;
    }

    public ComLevels(params byte[] values)
    {
        _comSum = 0;
        for (var i = 0; i < values.Length; i++)
        {
            _comSum += values[i];
            this[i] = values[i];
        }
    }

    public bool HasAi => _comSum != 0;
}