using DragonGame.Engine;

namespace DragonGame
{
    internal class Program
    {
        private static int Main()
        {
            using var game = new Game();
            game.Run(60);

            return 0;
        }
    }
}