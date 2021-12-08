using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming
{
    internal class Platforms
    {
        public const short PlatformCount = 1;
        public const int InitialPlatformHeight = 100;
        public const int PlatformYStep = 150;

        public const int FinishingY = InitialPlatformHeight + PlatformCount * PlatformYStep;

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
            return yPosition == 0 ? 0.0f : yPosition / (float) FinishingY;
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
                PlatformType.SimplePlatform => new SimplePlatform(id, position, _player),
                PlatformType.MovingPlatform => new MovingPlatform(id, position, _random, _player),
                PlatformType.TeleportingPlatform => new TeleportingPlatform(id, position, _random, _player),
                PlatformType.CooldownPlatform => new CooldownPlatform(id, position, _player),
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
            foreach (var platform in _platforms) platform.Update();
        }

        public void Draw(Camera camera)
        {
            foreach (var platform in _platforms) platform.Draw(camera);
        }

        public Platform GetAiTarget(AIPlayer player)
        {
            foreach (var platform in _platforms)
            {
                var yDiff = platform.Position.Y - player.Position.Y;

                if (yDiff <= 0)
                    continue;

                return platform;
            }

            return null;
        }
    }
}