using DuckDuckJump.Scenes.Game.Gameplay.Items;
using DuckDuckJump.Scenes.Game.Gameplay.Players;

namespace DuckDuckJump.Scenes.Game.Gameplay.Items.Behaviors
{
    internal class Flip : ItemBehavior
    {
        private ushort _timer;

        private ushort FilpTime = 60 * 2;

        public Flip(Player player, GameField other) : base(player, other)
        {
            _timer = FilpTime;
        }

        public override bool IsDone()
        {
            return _timer == 0;
        }

        public override void OnItemOver()
        {
            Other.Flipped = false;
        }

        public override void OnUse()
        {
            Other.Flipped = true;
        }

        public override void Update()
        {
            if (_timer > 0)
                --_timer;
        }
    }
}