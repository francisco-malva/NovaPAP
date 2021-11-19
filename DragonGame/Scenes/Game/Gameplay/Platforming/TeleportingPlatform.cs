using DragonGame.Engine.Utilities;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay.Players;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class TeleportingPlatform : Platform
    {
        private const ushort StaticTime = 180;
        private const ushort DissapearingTime = 30;
        private const ushort AppearingTime = 30;

        private readonly DeterministicRandom _random;
        private TeleportingPlatformState _state;

        private ushort _stateTimer;

        public TeleportingPlatform(short id, Point position, DeterministicRandom random) : base(id, position)
        {
            _random = random;
            SetState(TeleportingPlatformState.Static);
            _stateTimer = (ushort)_random.GetInteger(10, StaticTime);
        }

        protected override bool CanJumpOnPlatform(Player player)
        {
            return base.CanJumpOnPlatform(player) && _state == TeleportingPlatformState.Static;
        }

        protected override void OnPlayerJump(Player player)
        {
        }

        protected override void OnUpdate(Player player)
        {
            switch (_state)
            {
                case TeleportingPlatformState.Static:
                    Alpha = byte.MaxValue;
                    if (_stateTimer == 0) SetState(TeleportingPlatformState.Dissapearing);
                    break;
                case TeleportingPlatformState.Dissapearing:
                    Alpha = (byte)(_stateTimer / (float)DissapearingTime * 255.0f);
                    if (_stateTimer == 0)
                    {
                        Position.X = _random.GetInteger(PlatformWidth / 2, GameField.Width - PlatformWidth / 2);
                        SetState(TeleportingPlatformState.Appearing);
                    }
                    break;
                case TeleportingPlatformState.Appearing:
                    Alpha = (byte)((1.0f - (float)_stateTimer / DissapearingTime) * 255);
                    if (_stateTimer == 0) SetState(TeleportingPlatformState.Static);
                    break;
            }

            if (_stateTimer > 0) --_stateTimer;
        }

        private void SetState(TeleportingPlatformState state)
        {
            _state = state;
            switch (state)
            {
                case TeleportingPlatformState.Static:
                    _stateTimer = StaticTime;
                    break;
                case TeleportingPlatformState.Dissapearing:
                    _stateTimer = DissapearingTime;
                    break;
                case TeleportingPlatformState.Appearing:
                    _stateTimer = AppearingTime;
                    break;
            }
        }
    }
}