﻿using System.Collections.Generic;

namespace DuckDuckJump.Engine.GUI;

internal class Selector
{
    public Stack<SelectionGroup> _selectionStack = new();

    public void Tick()
    {
        _selectionStack.Peek().Tick();
    }

    public void Draw()
    {
        _selectionStack.Peek().Draw();
    }

    public void Push(Selection[] selections)
    {
        var group = new SelectionGroup(selections);
        group.Init();
        _selectionStack.Push(group);
    }

    public void Set(Selection[] selections)
    {
        while (_selectionStack.Count > 0) Pop();
        Push(selections);
    }

    public void Pop()
    {
        _selectionStack.Pop();
    }
}