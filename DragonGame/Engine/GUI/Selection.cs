using System;

namespace DuckDuckJump.Engine.GUI;

internal class Selection
{
    public string Label;
    public readonly Action<Selection> OnPush;
    public readonly Action<Selection> OnSelect;
    public readonly Action<Selection> OnHovered;
    public readonly bool Selectable;

    public Selection(string label, Action<Selection> onPush, Action<Selection> onSelect, Action<Selection> onHovered, bool selectable = true)
    {
        Label = label;
        OnPush = onPush;
        OnSelect = onSelect;
        OnHovered = onHovered;
        Selectable = selectable;
    }
}