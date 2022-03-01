using DuckDuckJump.Game.Gameplay.Messaging;
using DuckDuckJump.Game.Gameplay.Resources;

namespace DuckDuckJump.Game.Gameplay.Items.Behaviors;

internal class Umbrella : ItemBehavior
{
    private const ushort UmbrellaTime = 500;

    private readonly MessagePump _messagePump;
    private ushort _timer;

    public Umbrella(GameplayResources resources, MessagePump pump)
    {
        _messagePump = pump;
        Texture = resources.GetItemTexture(Item.Umbrella);
        _timer = UmbrellaTime;
    }

    public override bool IsDone()
    {
        return _timer == 0;
    }

    public override void OnItemOver()
    {
        _messagePump.SendMessage(new Message(MessageType.PlayerEndUmbrella, null));
    }

    public override void Update()
    {
        if (_timer > 0) --_timer;
    }

    public override void OnUse()
    {
        _messagePump.SendMessage(new Message(MessageType.PlayerBeginUmbrella, null));
    }
}