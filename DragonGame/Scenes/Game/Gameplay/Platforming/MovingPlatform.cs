using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class MovingPlatform : Platform
    {
        private const int PlatformMoveSpeed = 3;
        private bool _moveLeft;

        public MovingPlatform(short id, Point position) : base(id, position)
        {
        }

        protected override void OnPlayerJump(Player player)
        {
        }

        protected override void OnUpdate(Player player)
        {
            if (_moveLeft)
            {
                Position.X -= PlatformMoveSpeed;

                if (Position.X <= PlatformWidth / 2) _moveLeft = false;
            }
            else
            {
                Position.X += PlatformMoveSpeed;

                if (Position.X >= GameField.Width - PlatformWidth / 2) _moveLeft = true;
            }
        }
    }
}