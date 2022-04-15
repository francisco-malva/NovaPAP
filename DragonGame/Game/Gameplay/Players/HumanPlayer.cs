using System;
using DuckDuckJump.Game.Gameplay.Messaging;
using DuckDuckJump.Game.Gameplay.Platforming;
using DuckDuckJump.Game.Gameplay.Resources;
using DuckDuckJump.Game.Input;

namespace DuckDuckJump.Game.Gameplay.Players;

internal class HumanPlayer : Player
{
    public HumanPlayer(Random random, GameplayResources resources, MessagePump pump) : base(random, resources, pump)
    {
    }

    protected override void MoveX(PlatformManager platformManager, GameInput input)
    {
        if (input.HasFlag(GameInput.Left))
        {
            XSpeed -= 10;

            if (XSpeed < -XMoveSpeed)
                XSpeed = -XMoveSpeed;
        }
        else if (input.HasFlag(GameInput.Right))
        {
            XSpeed += 10;

            if (XSpeed > XMoveSpeed)
                XSpeed = XMoveSpeed;
        }
        else
        {
            XSpeed = 0;
        }
    }

    protected override void OnJump(Platform? platform)
    {
    }

    protected override void OnPressSpecial()
    {
        MessagePump.SendMessage(new Message(MessageType.RequestItemUsage, null));
    }

    protected override void ResetSpecialFields()
    {
    }
}