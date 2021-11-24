using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Players;
using Engine.Wrappers.SDL2;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal sealed class CooldownPlatform : Platform
    {
        private const ushort FadeOutTime = 15;
        private const ushort FadeInTime = 15;
        private const ushort DissapearenceTime = 60;

        private CooldownPlatformState _state;

        private ushort _timer;

        public CooldownPlatform(short id, Point position) : base(id, position)
        {
            SetState(CooldownPlatformState.Static);
        }

        private void SetState(CooldownPlatformState state)
        {
            _state = state;
            _timer = 0;
        }

        protected override void OnPlayerJump(Player player)
        {
            SetState(CooldownPlatformState.Dissapearing);
        }

        protected override Color GetPlatformDrawColor()
        {
            return new Color(192, 87, 70, 255);
        }

        protected override void OnUpdate(Player player)
        {
            ++_timer;
            switch (_state)
            {
                case CooldownPlatformState.Static:
                    break;
                case CooldownPlatformState.Dissapearing:
                    Alpha = (byte)((1.0f - _timer / (float)FadeOutTime) * 255.0f);
                    if (_timer == FadeOutTime) SetState(CooldownPlatformState.Dissapeared);
                    break;
                case CooldownPlatformState.Appearing:
                    Alpha = (byte)(_timer / (float)FadeOutTime * 255.0f);
                    if (_timer == FadeInTime) SetState(CooldownPlatformState.Static);
                    break;
                case CooldownPlatformState.Dissapeared:
                    if (_timer == DissapearenceTime) SetState(CooldownPlatformState.Appearing);
                    break;
            }
        }

        protected override bool CanJumpOnPlatform(Player player)
        {
            return base.CanJumpOnPlatform(player) && _state == CooldownPlatformState.Static;
        }

        public override bool TargetableByAi()
        {
            return _state == CooldownPlatformState.Static;
        }
    }
}