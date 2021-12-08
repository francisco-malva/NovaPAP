using System.Collections.Generic;

namespace DuckDuckJump.Engine.GUI
{
    internal class Selector
    {
        public Stack<SelectionGroup> _selectionStack = new Stack<SelectionGroup>();

        public void Tick()
        {
            _selectionStack.Peek().Tick();
        }

        public void Draw()
        {
            _selectionStack.Peek().Draw();
        }

        public void Push(SelectionGroup group)
        {
            _selectionStack.Push(group);
        }

        public void Pop()
        {
            _selectionStack.Pop();
        }
    }
}
