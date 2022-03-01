namespace DuckDuckJump.Game.Gameplay.Messaging;

public enum MessageType
{
    RequestItemUsage,
    PlayerDoubleJump,
    PlayerBeginUmbrella,
    PlayerEndUmbrella
}

public readonly struct Message
{
    public readonly MessageType Type;
    public readonly object? Data;

    public Message(MessageType type, object? data)
    {
        Type = type;
        Data = data;
    }
}