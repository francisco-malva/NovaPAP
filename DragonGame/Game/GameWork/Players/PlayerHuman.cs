#region

using System.Numerics;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game.GameWork.Players;

internal partial struct Player
{
    private Vector2 _velocity;
    private float _xVelocity;
    private float _targetXVelocity;
    private float _xVelocityDelta;
    private float _yVelocity;
    private const float YDamping = 0.45f;
    private const float MaxXVelocity = 15.0f;
    private const float JumpVelocity = -16.25f;

    private void HumanGameUpdate(GameInput input)
    {
        _targetXVelocity = 0.0f;

        if (input.HasFlag(GameInput.Left)) _targetXVelocity -= MaxXVelocity;
        if (input.HasFlag(GameInput.Right)) _targetXVelocity += MaxXVelocity;
    }
}