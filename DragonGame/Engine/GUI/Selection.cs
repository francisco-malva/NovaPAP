using System;

namespace DuckDuckJump.Engine.GUI
{
    internal struct Selection
    {
        public readonly string Label;
        public readonly Action OnSelect;
        public readonly Action OnHovered;
        public readonly bool Selectable;

        public Selection(string label, Action onSelect, Action onHovered, bool selectable = true)
        {
            Label = label;
            OnSelect = onSelect;
            OnHovered = onHovered;
            Selectable = selectable;
        }
    }
}
