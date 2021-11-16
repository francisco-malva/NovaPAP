using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Gameplay.Players;
using DragonGame.Wrappers;

namespace DragonGame.Scenes.Game.Gameplay.Platforming
{
    internal class TeleportingPlatform : Platform
    {

        private DeterministicRandom _random;

        private const ushort StaticTime = 120;
        private const ushort DissapearingTime = 60;
        private const ushort AppearingTime = 60;


        private ushort _stateTimer;
        private TeleportingPlatformState _state;

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
                    _alpha = byte.MaxValue;
                    if (_stateTimer == 0)
                    {
                        SetState(TeleportingPlatformState.Dissapearing);
                    }
                    break;
                case TeleportingPlatformState.Dissapearing:
                    _alpha = (byte)((float)_stateTimer / (float)DissapearingTime * 255.0f);
                    if (_stateTimer == 0)
                    {
                        Position.X = _random.GetInteger(PlatformWidth / 2, GameField.Width - PlatformWidth / 2);
                        SetState(TeleportingPlatformState.Appearing);
                    }
                    break;
                case TeleportingPlatformState.Appearing:
                    _alpha = (byte)((1.0f - ((float)_stateTimer / DissapearingTime)) * 255);
                    if (_stateTimer == 0)
                    {
                        SetState(TeleportingPlatformState.Static);
                    }
                    break;
            }
            if (_stateTimer > 0)
            {
                --_stateTimer;
            }
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