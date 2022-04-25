namespace DuckDuckJump.Game;

internal static partial class Match
{
    private static class ScoreWork
    {
        private static readonly sbyte[] Scores = new sbyte[PlayerCount];

        public static void Reset()
        {
            for (var i = 0; i < Scores.Length; i++) Scores[i] = 0;
        }

        public static void IncreaseScore(int playerIndex)
        {
            ++Scores[playerIndex];
        }

        public static bool GetWinner(out int playerIndex)
        {
            if (_info.ScoreCount <= 0)
            {
                playerIndex = -1;
                return false;
            }

            for (var i = 0; i < PlayerCount; i++)
            {
                if (Scores[i] < _info.ScoreCount) continue;

                playerIndex = i;
                return true;
            }

            playerIndex = -1;
            return false;
        }
    }
}