using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items.Behaviors;

internal class DoubleJump : ItemBehavior
{
    public DoubleJump(Player player, GameField other, GameplayResources resources) : base(player, other)
    {
        Texture = resources.GetItemTexture(Item.DoubleJump);
    }

    public override bool IsDone()
    {
        return true;
    }

    public override void OnItemOver()
    {
    }

    public override void OnUse()
    {
        Player.Jump();
    }

    public override void Update()
    {
    }
}