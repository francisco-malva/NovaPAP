using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Players;
using Engine.Wrappers.SDL2;

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

        protected override Color GetPlatformDrawColor()
        {
            return new Color(208, 227, 196, 255);
        }

        protected override void OnUpdate(Player player)
        {
        }

        public override bool Equals(object obj)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override bool CanJumpOnPlatform(Player player)
        {
            return base.CanJumpOnPlatform(player);
        }

        public override bool TargetableByAi()
        {
            return true;
        }
    }
}