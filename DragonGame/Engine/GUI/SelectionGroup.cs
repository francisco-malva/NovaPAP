﻿using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;

namespace DuckDuckJump.Engine.GUI
{
    internal class SelectionGroup
    {
        private sbyte _selected;
        private readonly Selection[] _selections;

        public SelectionGroup(Selection[] selections)
        {
            _selected = 0;
            _selections = selections;
        }

        public void Tick()
        {
            if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_UP))
            {
                var selection = _selected - 1;
                _selected = (sbyte)(selection < 0 ? _selections.Length - 1 : selection);
            }
            if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
            {
                _selected = (sbyte)((_selected + 1) % _selections.Length);
            }

            while (!_currentSelection.Selectable)
            {
                var selection = _selected + 1;
                _selected = (sbyte)(selection < 0 ? _selections.Length - 1 : selection);
            }

            _currentSelection.OnHovered?.Invoke();

            if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_Z))
            {
                _currentSelection.OnSelect?.Invoke();
            }
        }

        public void Draw()
        {
            Game.Instance.Renderer.SetScale(new Point(2, 2));
            for (int i = 0; i < _selections.Length; i++)
            {
                var color = Color.Yellow;

                if (_selections[i].Selectable)
                {
                    color = _selected == i ? Color.White : new Color(128, 128, 128, 255);
                }
                UI.DrawText(new Point(10, 12 + ((5 + 12) * i)), color, _selections[i].Label);
            }
            Game.Instance.Renderer.SetScale(Point.One);
        }

        private Selection _currentSelection => _selections[_selected];

    }
}
