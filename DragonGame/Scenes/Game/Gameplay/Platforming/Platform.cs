using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal abstract class Platform
    {
        public const int PlatformWidth = 68;
        public const int PlatformHeight = 14;
        private const int PlatformMoveSpeed = 2;

        public readonly short ID;
        protected byte _alpha = 255;

        public Point Position;

        public Platform(short id, Point position)
        {
            ID = id;
            Position = position;
        }

        public void Draw(Texture texture, int yScroll)
        {
            var renderer = Engine.Game.Instance.Renderer;

            var dst = new Rectangle(Position.X - texture.Width / 2,
                GameField.TransformY(Position.Y + texture.Height, yScroll), texture.Width,
                texture.Height);
            texture.SetAlphaMod(_alpha);

            renderer.Copy(texture, null, dst);
        }

        public void Update(Player player)
        {
            OnUpdate(player);
            PlayerCollision(player);
        }

        protected abstract void OnUpdate(Player player);

        protected virtual bool CanJumpOnPlatform(Player player)
        {
            return player.State == PlayerState.InGame && player.Descending && CollidingWithPlatform(player);
        }
        private void PlayerCollision(Player player)
        {
            if (CanJumpOnPlatform(player))
            {
                OnPlayerJump(player);
                player.Jump(this);
            }

        }

        protected abstract void OnPlayerJump(Player player);

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