#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Events;

/// <summary>
///     A class that redirects SDL events to C# callbacks.
/// </summary>
internal class EventPump
{
    /// <summary>
    ///     All currently registered event callbacks.
    /// </summary>
    private readonly Dictionary<SDL.SDL_EventType, Action<SDL.SDL_Event>> _eventCallbacks = new();

    /// <summary>
    ///     Register a callback to a specific SDL event.
    /// </summary>
    /// <param name="eventType">The event the callback will be raised by.</param>
    /// <param name="callback">The callback to register.</param>
    public void Subscribe(SDL.SDL_EventType eventType, Action<SDL.SDL_Event> callback)
    {
        if (!_eventCallbacks.ContainsKey(eventType))
            _eventCallbacks.Add(eventType, callback);
        else
            _eventCallbacks[eventType] += callback;
    }

    /// <summary>
    ///     Unsubscribe a callback from an event type.
    /// </summary>
    /// <param name="eventType">The event to unsubscribe from.</param>
    /// <param name="callback">The previously registered event callback.</param>
    public void Unsubscribe(SDL.SDL_EventType eventType, Action<SDL.SDL_Event> callback)
    {
        if (!_eventCallbacks.ContainsKey(eventType))
            throw new InvalidOperationException();

        Debug.Assert(_eventCallbacks != null, nameof(_eventCallbacks) + " != null");
        _eventCallbacks[eventType] -= callback;
    }

    /// <summary>
    ///     Process all polled SDL events and call the corresponding callbacks.
    /// </summary>
    public void HandleEvents()
    {
        while (SDL.SDL_PollEvent(out var eventData) > 0)
            if (_eventCallbacks.ContainsKey(eventData.type))
                _eventCallbacks[eventData.type](eventData);
    }
}