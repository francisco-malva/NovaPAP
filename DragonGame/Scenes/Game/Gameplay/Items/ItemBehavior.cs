using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items;

internal abstract class ItemBehavior
{
    protected Player Player;
    protected GameField Other;

    public Texture Texture { get; protected set; }

    public ItemBehavior(Player player, GameField other)
    {
        Player = player;
        Other = other;
    }

    public abstract void OnUse();
    
    public abstract void Update();

    public abstract bool IsDone();

    public abstract void OnItemOver();
}
