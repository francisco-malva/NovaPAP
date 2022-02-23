using DuckDuckJump.Scenes.Game.Gameplay.Players;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items.Behaviors;

internal class Umbrella : ItemBehavior
{
    private const ushort UmbrellaTime = 500;
    private ushort _timer;

    public Umbrella(Player player, GameField other, GameplayResources resources) : base(player, other)
    {
        Texture = resources.GetItemTexture(Item.Umbrella);
        _timer = UmbrellaTime;
    }

    public override bool IsDone()
    {
        return _timer == 0;
    }

    public override void OnItemOver()
    {
        Player.Umbrella = false;
    }

    public override void Update()
    {
        if (_timer > 0) --_timer;
    }

    public override void OnUse()
    {
        Player.Umbrella = true;
    }
}