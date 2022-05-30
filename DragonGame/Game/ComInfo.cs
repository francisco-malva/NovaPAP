#region

using System.Linq;

#endregion

namespace DuckDuckJump.Game;

internal class ComInfo
{
    public readonly byte[] Levels;

    public ComInfo()
    {
        Levels = new byte[Match.PlayerCount];
    }

    public ComInfo(params byte[] levels)
    {
        Levels = levels;
    }

    public bool HasAi
    {
        get { return Levels.Any(value => value > 0); }
    }
}