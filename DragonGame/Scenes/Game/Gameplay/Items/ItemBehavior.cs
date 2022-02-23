using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items;

internal abstract class ItemBehavior
{
    protected GameField Other;
    protected Player Player;

    public ItemBehavior(Player player, GameField other)
    {
        Player = player;
        Other = other;
    }

    public Texture Texture { get; protected set; }

    public abstract void OnUse();

    public abstract void Update();

    public abstract bool IsDone();

    public abstract void OnItemOver();
}