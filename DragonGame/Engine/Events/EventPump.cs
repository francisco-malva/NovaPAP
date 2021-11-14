using System;
using System.Collections.Generic;
using SDL2;

namespace DragonGame.Engine.Events
{
    internal class EventPump
    {
        private readonly Dictionary<SDL.SDL_EventType, Action<SDL.SDL_Event>> _events = new();

        public void Subscribe(SDL.SDL_EventType eventType, Action<SDL.SDL_Event> callback)
        {
            if (!_events.ContainsKey(eventType))
                _events.Add(eventType, callback);
            else
                _events[eventType] += callback;
        }

        public void Unsubscribe(SDL.SDL_EventType eventType, Action<SDL.SDL_Event> callback)
        {
            _events[eventType] -= callback;

            if (_events[eventType] == null) _events.Remove(eventType);
        }

        public void Dispatch()
        {
            while (SDL.SDL_PollEvent(out var eventData) > 0)
                if (_events.ContainsKey(eventData.type))
                    _events[eventData.type](eventData);
        }
    }
}