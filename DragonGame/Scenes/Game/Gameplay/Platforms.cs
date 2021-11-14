using DragonGame.Engine.Rollback;
using DragonGame.Engine.Utilities;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay
{
    internal class Platforms : IRollbackable
    {
        public bool CanCollide;

        private const short PlatformCount = 10;
        private const int InitialPlatformHeight = 100;
        private const int PlatformYStep = 150;

        private readonly int _finishingY;
        private readonly Platform[] _platforms;

        private readonly Player _player;

        private readonly Texture _texture;

        public Platforms(Player player, DeterministicRandom random, Texture texture)
        {
            _player = player;
            _texture = texture;

            _platforms = new Platform[PlatformCount];

            for (short i = 0; i < _platforms.Length; i++)
                _platforms[i] = new Platform(i, new Point(
                    random.GetInteger(Platform.PlatformWidth / 2, GameField.Width - Platform.PlatformWidth / 2),
                    InitialPlatformHeight + i * PlatformYStep), random);

            _finishingY = InitialPlatformHeight + PlatformCount * PlatformYStep;
        }

        public bool PlayerFinishedCourse => _player.Position.Y + Player.PlatformCollisionHeight > _finishingY;

        public Platform this[short id] => _platforms[id];

        public void Save(StateBuffer stateBuffer)
        {
            stateBuffer.Write(CanCollide);
            foreach (var platform in _platforms) platform.Save(stateBuffer);
        }

        public void Rollback(StateBuffer stateBuffer)
        {
            CanCollide = stateBuffer.Read<bool>();
            foreach (var platform in _platforms) platform.Rollback(stateBuffer);
        }

        public void Update()
        {
            foreach (var platform in _platforms) platform.Update(CanCollide, _player);
        }

        public void Draw(int yScroll)
        {
            DrawPlatforms(yScroll);
            DrawFinishingLine(yScroll);
        }

        private void DrawPlatforms(int yScroll)
        {
            foreach (var platform in _platforms) platform.Draw(_texture, yScroll);
        }

        private void DrawFinishingLine(int yScroll)
        {
            var a = new Point(0, GameField.TransformY(_finishingY, yScroll));

            if (a.Y > GameField.Height) //Cull test
                return;

            var b = new Point(GameField.Width, GameField.TransformY(_finishingY, yScroll));

            Engine.Game.Instance.Renderer.SetDrawColor(255, 0, 0, 255);
            Engine.Game.Instance.Renderer.DrawLine(ref a, ref b);
        }

        public short GetPlatformAbove(ref Point position)
        {
            short target = -1;
            var closestDistance = int.MaxValue;

            foreach (var platform in _platforms)
            {
                var yDiff = platform.Position.Y - position.Y;

                if (yDiff <= 0 || yDiff >= closestDistance)
                    continue;

                closestDistance = yDiff;
                target = platform.ID;
            }

            return target;
        }

        public short GetPlatformBelow(ref Point position)
        {
            short target = -1;
            var closestDistance = int.MinValue;

            foreach (var platform in _platforms)
            {
                var yDiff = platform.Position.Y - position.Y;

                if (yDiff >= 0 || yDiff <= closestDistance)
                    continue;

                closestDistance = yDiff;
                target = platform.ID;
            }

            return target;
        }
    }
}