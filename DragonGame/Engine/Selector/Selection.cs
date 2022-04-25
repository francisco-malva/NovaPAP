#region

using System.Drawing;

#endregion

namespace DuckDuckJump.Engine.Selector;

public struct Selection
{
    public readonly string Caption;
    public readonly Color Color;
    public readonly bool Selectable;

    public Selection(string caption, Color color, bool selectable = false)
    {
        Caption = caption;
        Color = color;
        Selectable = selectable;
    }

    public static readonly Selection Nothing = new(" ", Color.White);
}