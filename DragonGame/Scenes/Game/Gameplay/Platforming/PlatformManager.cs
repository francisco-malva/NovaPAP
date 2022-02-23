using System;
using System.Linq;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming;

internal class PlatformManager
{
    private const int InitialPlatformHeight = 100;
    private const int PlatformYStep = 150;

    private readonly ushort _platformCount;


    private readonly Texture _platformTexture;


    private readonly Random _random;

    public readonly Platform?[] Platforms;

    private PlatformType _lastPlatformType = PlatformType.None;

    public PlatformManager(Random random, ushort platformCount, GameplayResources resources)
    {
        _platformCount = platformCount;

        Platforms = new Platform?[_platformCount];
        _random = random;

        _platformTexture = resources.PlatformTexture;
    }

    public int FinishingY => InitialPlatformHeight + _platformCount * PlatformYStep;

    public Platform? this[short id] => Platforms[id];

    public bool HasPlayerFinishedCourse(Player player)
    {
        return player.Position.Y + Player.PlatformCollisionHeight > FinishingY;
    }


    public float GetClimbingProgress(Player player)
    {
        return player.Position.Y == 0 ? 0.0f : player.Position.Y / (float) FinishingY;
    }

    private PlatformType GetRandomPlatformType()
    {
        PlatformType type;

        do
        {
            var randomNum = _random.NextSingle();

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

    private Platform? GetPlatform(PlatformType type, Point position)
    {
        return type switch
        {
            PlatformType.SimplePlatform => new SimplePlatform(position, _platformTexture),
            PlatformType.MovingPlatform => new MovingPlatform(position, _random, _platformTexture),
            PlatformType.TeleportingPlatform => new TeleportingPlatform(position, _random, _platformTexture),
            PlatformType.CooldownPlatform => new CooldownPlatform(position, _platformTexture),
            _ => null
        };
    }

    public void GeneratePlatforms()
    {
        for (ushort i = 0; i < _platformCount; i++)
            Platforms[i] = GetPlatform(GetRandomPlatformType(), new Point(
                _random.Next(Platform.PlatformWidth / 2, GameField.Width - Platform.PlatformWidth / 2),
                InitialPlatformHeight + PlatformYStep * i));
    }

    public void Update(Player player)
    {
        foreach (var platform in Platforms) platform?.Update(player);
    }

    public void Draw(Camera camera)
    {
        foreach (var platform in Platforms) platform?.Draw(camera);
    }

    public Platform? GetAiTarget(AiPlayer player)
    {
        return (from platform in Platforms
            let yDiff = platform.Position.Y - player.Position.Y
            where yDiff > 0
            select platform).FirstOrDefault();
    }
}