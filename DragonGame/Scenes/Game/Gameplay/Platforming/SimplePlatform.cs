using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class SimplePlatform : Platform
    {
        public SimplePlatform(short id, Point position) : base(id, position)
        {
        }

        protected override void OnPlayerJump(Player player)
        {
        }

        protected override void OnUpdate(Player player)
        {
        }
    }
}