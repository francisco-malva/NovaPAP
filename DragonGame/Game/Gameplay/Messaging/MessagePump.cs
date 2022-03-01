using System.Collections.Generic;

namespace DuckDuckJump.Game.Gameplay.Messaging;

public class MessagePump
{
    private readonly HashSet<MessagePoint> _messagePoints = new();
    private readonly Queue<Message> _messageQueue = new();

    public void Subscribe(MessagePoint messagePoint)
    {
        _messagePoints.Add(messagePoint);
    }

    public void Unsubscribe(MessagePoint messagePoint)
    {
        _messagePoints.Remove(messagePoint);
    }

    public void HandleMessages()
    {
        while (_messageQueue.Count > 0)
        {
            var message = _messageQueue.Dequeue();
            foreach (var messagePoint in _messagePoints) messagePoint.HandleMessage(message);
        }
    }

    public void SendMessage(Message message)
    {
        _messageQueue.Enqueue(message);
    }
}