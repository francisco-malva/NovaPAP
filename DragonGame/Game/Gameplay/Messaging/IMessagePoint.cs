namespace DuckDuckJump.Game.Gameplay.Messaging;

public abstract class MessagePoint
{
    protected readonly MessagePump MessagePump;

    protected MessagePoint(MessagePump pump)
    {
        MessagePump = pump;
        MessagePump.Subscribe(this);
    }

    ~MessagePoint()
    {
        MessagePump.Unsubscribe(this);
    }


    public abstract void HandleMessage(Message message);
}