using DragonGame.Engine.Utilities;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Players;
using Engine.Wrappers.SDL2;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class Platforms
    {
        public const short PlatformCount = 30;
        public const int InitialPlatformHeight = 100;
        public const int PlatformYStep = 150;

        private const int FinishingY = InitialPlatformHeight + PlatformCount * PlatformYStep;

        private readonly Platform[] _platforms;

        private readonly Player _player;

        private readonly DeterministicRandom _random;

        private readonly Texture _texture;

        private PlatformType _lastPlatformType = PlatformType.None;


        public Platforms(Player player, DeterministicRandom random)
        {
            _player = player;
            _texture = Engine.Game.Instance.TextureManager["Game/platform"];

            _platforms = new Platform[PlatformCount];
            _random = random;
        }

        public bool PlayerFinishedCourse => _player.Position.Y + Player.PlatformCollisionHeight > FinishingY;

        public Platform this[short id] => _platforms[id];


        public static float GetClimbingProgress(int yPosition)
        {
            return yPosition == 0 ? 0.0f : yPosition / (float)FinishingY;
        }

        private PlatformType GetRandomPlatformType()
        {
            PlatformType type;

            do
            {
                var randomNum = _random.GetFloat();

                type = randomNum switch
                {
                    <= 0.25f => PlatformType.MovingPlatform,
                    > 0.25f and <= 0.40f => PlatformType.TeleportingPlatform,
                    > 0.40f and <= 0.50f => PlatformType.CooldownPlatform,
                    _ => PlatformType.SimplePlatform
                };
            } while (type == _lastPlatformType);

            _lastPlatformType = type;

            return type;
        }

        private Platform GetPlatform(PlatformType type, short id, Point position)
        {
            return type switch
            {
                PlatformType.SimplePlatform => new SimplePlatform(id, position),
                PlatformType.MovingPlatform => new MovingPlatform(id, position, _random),
                PlatformType.TeleportingPlatform => new TeleportingPlatform(id, position, _random),
                PlatformType.CooldownPlatform => new CooldownPlatform(id, position),
                _ => null
            };
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

            var b = new Point(
                GameField.Width,
                GameField.TransformY(FinishingY, yScroll));

            Engine.Game.Instance.Renderer.SetDrawColor(Color.Red);
            Engine.Game.Instance.Renderer.DrawLine(a, b);
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
                target = platform.Id;
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
                target = platform.Id;
            }

            return target;
        }
    }
}