using System;
using DragonGame.Engine.Rollback;
using DragonGame.Engine.Utilities;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay
{
    internal class Platform : IRollbackable
    {
        public const int PlatformWidth = 68;
        public const int PlatformHeight = 14;
        private const int PlatformMoveSpeed = 2;
        public readonly short ID;
        private bool _moveLeft;
        private bool _moving;


        private bool _onScreen;
        public Point Position;


        public Platform(short id, Point position, DeterministicRandom random)
        {
            ID = id;
            Position = position;

            _moving = random.GetFloat() <= 0.25;
        }

        public void Save(StateBuffer stateBuffer)
        {
            stateBuffer.Write(Position.X);
            stateBuffer.Write(Position.Y);
            stateBuffer.Write(_moving);
            stateBuffer.Write(_moveLeft);
        }

        public void Rollback(StateBuffer stateBuffer)
        {
            Position.X = stateBuffer.Read<int>();
            Position.Y = stateBuffer.Read<int>();
            _moving = stateBuffer.Read<bool>();
            _moveLeft = stateBuffer.Read<bool>();
        }

        public void Draw(Texture texture, int yScroll)
        {
            if (!_onScreen)
                return;

            var dst = new Rectangle(Position.X - texture.Width / 2,
                GameField.TransformY(Position.Y + texture.Height, yScroll), texture.Width,
                texture.Height);
            Engine.Game.Instance.Renderer.Copy(texture, null, dst);
        }

        public void Update(bool canCollide, Player player)
        {
            if (_moving) UpdateMove();

            UpdateScreenState(player);

            if (_onScreen && canCollide) PlayerCollision(player);
        }

        private void UpdateScreenState(Player player)
        {
            _onScreen = Math.Abs(Position.Y - player.Position.Y) <= GameField.Height;
        }

        private void UpdateMove()
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

        private void PlayerCollision(Player player)
        {
            if (player.Descending && CollidingWithPlatform(player)) player.Jump(this);
        }

        private bool CollidingWithPlatform(Player player)
        {
            var rectangle = new Rectangle(Position.X - PlatformWidth / 2, Position.Y - PlatformHeight / 2,
                PlatformWidth, PlatformHeight);

            var a1 = new Point(player.Position.X - Player.PlatformCollisionWidth / 2, player.Position.Y);
            var b1 = new Point(player.Position.X + Player.PlatformCollisionWidth / 2, player.Position.Y);

            var a2 = new Point(player.Position.X - Player.PlatformCollisionWidth / 2,
                player.Position.Y + Player.PlatformCollisionHeight / 2);
            var b2 = new Point(player.Position.X + Player.PlatformCollisionWidth / 2,
                player.Position.Y + Player.PlatformCollisionHeight / 2);

            var a3 = new Point(player.Position.X - Player.PlatformCollisionWidth / 2,
                player.Position.Y + Player.PlatformCollisionHeight);
            var b3 = new Point(player.Position.X + Player.PlatformCollisionWidth / 2,
                player.Position.Y + Player.PlatformCollisionHeight);

            return rectangle.HasLine(ref a1, ref b1) || rectangle.HasLine(ref a2, ref b2) ||
                   rectangle.HasLine(ref a3, ref b3);
        }
    }
}