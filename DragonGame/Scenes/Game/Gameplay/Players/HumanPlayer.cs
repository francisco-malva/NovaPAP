using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Platforming;
using DragonGame.Scenes.Game.Input;

namespace DragonGame.Scenes.Game.Gameplay.Players
{
    internal class HumanPlayer : Player
    {
        public HumanPlayer(DeterministicRandom random) : base(random)
        {
        }

        protected override void MoveX(Platforms platforms, GameInput input)
        {
            if (input.HasFlag(GameInput.Left))
                XSpeed = -XMoveSpeed;
            else if (input.HasFlag(GameInput.Right))
                XSpeed = XMoveSpeed;
            else
                XSpeed = 0;
        }

        protected override void OnJump(Platform platform)
        {
        }

        protected override void ResetSpecialFields()
        {
        }
    }
}