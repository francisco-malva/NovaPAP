using DuckDuckJump.Game.Gameplay.Messaging;
using DuckDuckJump.Game.Gameplay.Resources;

namespace DuckDuckJump.Game.Gameplay.Items.Behaviors;

internal class DoubleJump : ItemBehavior
{
    private readonly MessagePump _pump;

    public DoubleJump(GameplayResources resources, MessagePump pump)
    {
        Texture = resources.GetItemTexture(Item.DoubleJump);
        _pump = pump;
    }

    public override bool IsDone()
    {
        return true;
    }

    public override void OnItemOver()
    {
    }

    public override void OnUse()
    {
        _pump.SendMessage(new Message(MessageType.PlayerDoubleJump, null));
    }

    public override void Update()
    {
    }
}