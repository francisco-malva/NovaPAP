using System.Diagnostics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming;

internal abstract class Platform
{
    public const int PlatformWidth = 68;
    public const int PlatformHeight = 14;
    private readonly Texture _platformTexture;

    private readonly TextureInfo _platformTextureInfo;

    protected byte Alpha = 255;

    public Point Position;

    protected Platform(Point position, Texture platformTexture)
    {
        Position = position;
        _platformTexture = platformTexture;
        _platformTextureInfo = _platformTexture.QueryTexture();
    }

    private Rectangle Collision => new(Position.X - PlatformWidth / 2, Position.Y - PlatformHeight / 2,
        PlatformWidth, PlatformHeight);

    private bool IsPlayerCollidingWithPlatform(Player player)
    {
        var platformRect = Collision;
        var playerRect = player.Collision;

        return platformRect.HasIntersection(ref playerRect);
    }

    public void Draw(Camera camera)
    {
        Debug.Assert(Engine.Game.Instance != null, "Engine.Game.Instance != null");
        var renderer = Engine.Game.Instance.Renderer;

        var screenPosition =
            camera.TransformPoint(new Point(Position.X - _platformTextureInfo.Width / 2,
                Position.Y + _platformTextureInfo.Height));
        var dst = new Rectangle(screenPosition.X, screenPosition.Y, _platformTextureInfo.Width,
            _platformTextureInfo.Height);

        if (!camera.OnScreen(dst)) return;
        _platformTexture.SetAlphaMod(Alpha);
        _platformTexture.SetColorMod(GetPlatformDrawColor());

        renderer.Copy(_platformTexture, null, dst);
    }

    public void Update(Player player)
    {
        OnUpdate();
        TestPlayerCollision(player);
    }

    protected abstract void OnUpdate();

    protected virtual bool ShouldPlayerTriggerJump(Player player)
    {
        return player.State == PlayerState.InGame && player.Descending && IsPlayerCollidingWithPlatform(player);
    }

    private void TestPlayerCollision(Player player)
    {
        if (!ShouldPlayerTriggerJump(player)) return;

        OnPlayerJump();
        player.Jump(this);
    }

    protected abstract void OnPlayerJump();

    protected abstract Color GetPlatformDrawColor();

    public abstract bool CanBeTargetedByAi();

    public bool InZone(AiPlayer player)
    {
        return player.Position.X >= Position.X - PlatformWidth / 2 + 5 &&
               player.Position.X <= Position.X + PlatformWidth / 2 - 5;
    }
}