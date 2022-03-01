using DuckDuckJump.Game.Gameplay.Players;

namespace DuckDuckJump.Game.Gameplay.Items.Behaviors;

internal class Flip : ItemBehavior
{
    private readonly ushort FilpTime = 60 * 2;
    private ushort _timer;

    public Flip(Player player, GameField other)
    {
        _timer = FilpTime;
    }

    public override bool IsDone()
    {
        return _timer == 0;
    }

    public override void OnItemOver()
    {
    }

    public override void OnUse()
    {
    }

    public override void Update()
    {
        if (_timer > 0)
            --_timer;
    }
}