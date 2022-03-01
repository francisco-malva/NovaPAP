using DuckDuckJump.Engine;

namespace DuckDuckJump;

internal static class Program
{
    private static int Main()
    {
        using var game = new GameContext();
        game.Run(60);

        return 0;
    }
}