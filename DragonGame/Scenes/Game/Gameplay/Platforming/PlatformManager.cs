using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming;

internal class PlatformManager
{
    public const int InitialPlatformHeight = 100;
    public const int PlatformYStep = 150;

    private readonly Player _player;

    private readonly DeterministicRandom _random;

    public readonly ushort PlatformCount;

    public readonly Platform[] Platforms;

    private PlatformType _lastPlatformType = PlatformType.None;


    public PlatformManager(Player player, DeterministicRandom random, ushort platformCount)
    {
        PlatformCount = platformCount;
        _player = player;

        Platforms = new Platform[PlatformCount];
        _random = random;
    }

    public int FinishingY => InitialPlatformHeight + PlatformCount * PlatformYStep;

    public bool PlayerFinishedCourse => _player.Position.Y + Player.PlatformCollisionHeight > FinishingY;

    public Platform this[short id] => Platforms[id];


    public float GetClimbingProgress()
    {
        return _player.Position.Y == 0 ? 0.0f : _player.Position.Y / (float)FinishingY;
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

    private Platform GetPlatform(PlatformType type, Point position)
    {
        return type switch
        {
            PlatformType.SimplePlatform => new SimplePlatform(position, _player),
            PlatformType.MovingPlatform => new MovingPlatform(position, _random, _player),
            PlatformType.TeleportingPlatform => new TeleportingPlatform(position, _random, _player),
            PlatformType.CooldownPlatform => new CooldownPlatform(position, _player),
            _ => null
        };
    }

    public void GeneratePlatforms()
    {
        for (ushort i = 0; i < PlatformCount; i++)
            Platforms[i] = GetPlatform(GetRandomPlatformType(), new Point(
                _random.GetInteger(Platform.PlatformWidth / 2, GameField.Width - Platform.PlatformWidth / 2),
                InitialPlatformHeight + PlatformYStep * i));
    }

    public void Update()
    {
        foreach (var platform in Platforms) platform.Update();
    }

    public void Draw(Camera camera)
    {
        foreach (var platform in Platforms) platform.Draw(camera);
    }

    public Platform GetAiTarget(AIPlayer player)
    {
        foreach (var platform in Platforms)
        {
            var yDiff = platform.Position.Y - player.Position.Y;

            if (yDiff <= 0)
                continue;

            return platform;
        }

        return null;
    }
}