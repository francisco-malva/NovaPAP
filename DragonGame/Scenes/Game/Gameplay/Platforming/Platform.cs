﻿using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class Platform
    {
        public const int PlatformWidth = 68;
        public const int PlatformHeight = 14;
        private const int PlatformMoveSpeed = 2;

        private readonly DeterministicRandom _random;
        public readonly short ID;
        private bool _moveLeft;
        private bool _moving;
        public Point Position;


        public Platform(short id, DeterministicRandom random)
        {
            ID = id;
            _random = random;

            Reset();
        }

        public void Reset()
        {
            _moving = _random.GetFloat() <= 0.25f;
            Position.X = _random.GetInteger(PlatformWidth / 2, GameField.Width - PlatformWidth / 2);
            Position.Y = Platforms.InitialPlatformHeight + ID * Platforms.PlatformYStep;
        }

        public void Draw(Texture texture, int yScroll)
        {
            var dst = new Rectangle(Position.X - texture.Width / 2,
                GameField.TransformY(Position.Y + texture.Height, yScroll), texture.Width,
                texture.Height);
            Engine.Game.Instance.Renderer.Copy(texture, null, dst);
        }

        public void Update(Player player)
        {
            if (_moving) UpdateMove();
            PlayerCollision(player);
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
            if (player.State == PlayerState.InGame && player.Descending && CollidingWithPlatform(player))
                player.Jump(this);
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