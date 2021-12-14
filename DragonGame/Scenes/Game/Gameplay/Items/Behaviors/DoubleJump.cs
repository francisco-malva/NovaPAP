using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items.Behaviors;

internal class DoubleJump : ItemBehavior
{
    public DoubleJump(Player player, GameField other) : base(player, other)
    {
        Texture = Engine.Game.Instance.TextureManager["Game/double-jump"];
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