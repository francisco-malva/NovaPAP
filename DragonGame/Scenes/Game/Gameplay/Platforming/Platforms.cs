using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class Platforms
    {
        public const short PlatformCount = 10;
        public const int InitialPlatformHeight = 100;
        public const int PlatformYStep = 150;

        private const int FinishingY = InitialPlatformHeight + PlatformCount * PlatformYStep;

        private readonly Platform[] _platforms;

        private readonly Player _player;

        private readonly DeterministicRandom _random;

        private readonly Texture _texture;

        private PlatformType _lastPlatformType = PlatformType.None;

        public Platforms(Player player, DeterministicRandom random, Texture texture)
        {
            _player = player;
            _texture = texture;

            _platforms = new Platform[PlatformCount];
            _random = random;
        }

        public bool PlayerFinishedCourse => _player.Position.Y + Player.PlatformCollisionHeight > FinishingY;

        public Platform this[short id] => _platforms[id];

        private PlatformType GetRandomPlatformType()
        {
            PlatformType type;

            do
            {
                var randomNum = _random.GetFloat();

                type = randomNum switch
                {
                    <= 0.25f => PlatformType.MovingPlatform,
                    > 0.25f and < 0.40f => PlatformType.TeleportingPlatform,
                    _ => PlatformType.SimplePlatform
                };
            } while (type == _lastPlatformType);

            _lastPlatformType = type;

            return type;
        }

        private Platform GetPlatform(PlatformType type, short id, Point position)
        {
            switch (type)
            {
                case PlatformType.SimplePlatform:
                    return new SimplePlatform(id, position);
                case PlatformType.MovingPlatform:
                    return new MovingPlatform(id, position);
                case PlatformType.TeleportingPlatform:
                    return new TeleportingPlatform(id, position, _random);
            }

            return null;
        }

        public void GeneratePlatforms()
        {
            for (short i = 0; i < PlatformCount; i++)
                _platforms[i] = GetPlatform(GetRandomPlatformType(), i, new Point(
                    _random.GetInteger(Platform.PlatformWidth / 2, GameField.Width - Platform.PlatformWidth / 2),
                    InitialPlatformHeight + PlatformYStep * i));
        }

        public void Update()
        {
            foreach (var platform in _platforms) platform.Update(_player);
        }

        public void Draw(int yScroll)
        {
            DrawPlatforms(yScroll);
            if (_player.State == PlayerState.InGame)
                DrawFinishingLine(yScroll);
        }

        private void DrawPlatforms(int yScroll)
        {
            foreach (var platform in _platforms) platform.Draw(_texture, yScroll);
        }

        private static void DrawFinishingLine(int yScroll)
        {
            var a = new Point(0, GameField.TransformY(FinishingY, yScroll));

            if (a.Y > GameField.Height) //Cull test
                return;

            var b = new Point(
                GameField.Width,
                GameField.TransformY(FinishingY, yScroll));

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