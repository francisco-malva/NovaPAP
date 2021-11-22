using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Players;
using Engine.Wrappers.SDL2;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal abstract class Platform
    {
        public const int PlatformWidth = 68;
        public const int PlatformHeight = 14;

        public readonly short Id;
        protected byte Alpha = 255;

        public Point Position;

        protected Platform(short id, Point position)
        {
            Id = id;
            Position = position;
        }

        private Rectangle Collision => new(Position.X - PlatformWidth / 2, Position.Y - PlatformHeight / 2,
            PlatformWidth, PlatformHeight);

        public void Draw(Texture texture, int yScroll)
        {
            var renderer = Engine.Game.Instance.Renderer;

            var dst = new Rectangle(Position.X - texture.Width / 2,
                GameField.TransformY(Position.Y + texture.Height, yScroll), texture.Width,
                texture.Height);

            if (dst.Y is < PlatformHeight / 2 or > GameField.Height + PlatformHeight / 2) return;
            texture.SetAlphaMod(Alpha);
            texture.SetColorMod(GetPlatformDrawColor());

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
            if (!CanJumpOnPlatform(player)) return;

            OnPlayerJump(player);
            player.Jump(this);
        }

        protected abstract void OnPlayerJump(Player player);

        private bool CollidingWithPlatform(Player player)
        {
            var platformRect = Collision;
            var playerRect = player.Collision;

            return platformRect.HasIntersection(ref playerRect);
        }

        protected abstract Color GetPlatformDrawColor();
    }
}