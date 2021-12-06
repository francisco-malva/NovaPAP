using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Players;
using Engine.Wrappers.SDL2;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class SimplePlatform : Platform
    {
        public SimplePlatform(short id, Point position, Player player) : base(id, position, player)
        {
        }

        protected override void OnPlayerJump()
        {
        }

        protected override Color GetPlatformDrawColor()
        {
            return new Color(208, 227, 196, 255);
        }

        protected override void OnUpdate()
        {
        }
        public override bool TargetableByAi()
        {
            return true;
        }
    }
}