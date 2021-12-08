using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;

namespace DuckDuckJump.Scenes.Game.Gameplay.Platforming;

internal abstract class Platform
{
    public const int PlatformWidth = 68;
    public const int PlatformHeight = 14;
    private readonly Texture _texture;

    public readonly short Id;

    protected readonly Player Player;
    protected byte Alpha = 255;

    public Point Position;

    protected Platform(short id, Point position, Player player)
    {
        Id = id;
        Position = position;
        Player = player;
        _texture = Engine.Game.Instance.TextureManager["Game/platform"];
    }

    private Rectangle Collision => new(Position.X - PlatformWidth / 2, Position.Y - PlatformHeight / 2,
        PlatformWidth, PlatformHeight);

    private bool PlayerCollidingWithPlatform
    {
        get
        {
            var platformRect = Collision;
            var playerRect = Player.Collision;

            return platformRect.HasIntersection(ref playerRect);
        }
    }

    public void Draw(Camera camera)
    {
        var renderer = Engine.Game.Instance.Renderer;

        var screenPosition =
            camera.TransformPoint(new Point(Position.X - _texture.Width / 2, Position.Y + _texture.Height));
        var dst = new Rectangle(screenPosition.X, screenPosition.Y, _texture.Width, _texture.Height);

        if (!camera.OnScreen(dst)) return;
        _texture.SetAlphaMod(Alpha);
        _texture.SetColorMod(GetPlatformDrawColor());

        renderer.Copy(_texture, null, dst);
    }

    public void Update()
    {
        OnUpdate();
        PlayerCollision();
    }

    protected abstract void OnUpdate();

    protected virtual bool CanJumpOnPlatform()
    {
        return Player.State == PlayerState.InGame && Player.Descending && PlayerCollidingWithPlatform;
    }

    private void PlayerCollision()
    {
        if (!CanJumpOnPlatform()) return;

        OnPlayerJump();
        Player.Jump(this);
    }

    protected abstract void OnPlayerJump();

    protected abstract Color GetPlatformDrawColor();

    public abstract bool TargetableByAi();

    public bool InZone(AIPlayer player)
    {
        return player.Position.X >= Position.X - PlatformWidth / 2 + 5 &&
               player.Position.X <= Position.X + PlatformWidth / 2 - 5;
    }
}