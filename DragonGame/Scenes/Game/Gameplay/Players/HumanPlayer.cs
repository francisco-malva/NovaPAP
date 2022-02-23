using System;
using DuckDuckJump.Scenes.Game.Gameplay.Items;
using DuckDuckJump.Scenes.Game.Gameplay.Platforming;
using DuckDuckJump.Scenes.Game.Gameplay.Resources;
using DuckDuckJump.Scenes.Game.Input;

namespace DuckDuckJump.Scenes.Game.Gameplay.Players;

internal class HumanPlayer : Player
{
    public HumanPlayer(Random random, GameplayResources resources, ItemManager itemManager) : base(random, resources,
        itemManager)
    {
    }

    protected override void MoveX(PlatformManager platformManager, GameInput input)
    {
        if (input.HasFlag(GameInput.Left))
            XSpeed = -XMoveSpeed;
        else if (input.HasFlag(GameInput.Right))
            XSpeed = XMoveSpeed;
        else
            XSpeed = 0;
    }

    protected override void OnJump(Platform? platform)
    {
    }

    protected override void OnPressSpecial()
    {
        ItemManager?.UseItem();
    }

    protected override void ResetSpecialFields()
    {
    }
}