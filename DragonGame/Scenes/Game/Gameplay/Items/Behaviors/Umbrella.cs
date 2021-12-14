using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items.Behaviors;

internal class Umbrella : ItemBehavior
{
    private ushort _timer;
    private const ushort UmbrellaTime = 500;

    public Umbrella(Player player, GameField other) : base(player, other)
    {
        Texture = Engine.Game.Instance.TextureManager["Game/umbrella"];
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
        if (_timer > 0)
        {
            --_timer;
        }
    }

    public override void OnUse()
    {
        Player.Umbrella = true;
    }
}
