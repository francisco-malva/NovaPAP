using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;

namespace DuckDuckJump.Game.Gameplay.Items.Behaviors;

internal abstract class ItemBehavior
{
    public Texture? Texture { get; protected init; }

    public abstract void OnUse();

    public abstract void Update();

    public abstract bool IsDone();

    public abstract void OnItemOver();
}