using System;
using System.Drawing;
using DuckDuckJump.Engine.Text;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;

namespace DuckDuckJump.Engine.Selector;

internal class TextSelector
{
    private const float RectangleSpeed = 0.25f;
    private readonly Renderer _renderer;

    private readonly TextDrawer _textDrawer;


    private Size[] _cachedSizes = new Size[1];
    private Rectangle _currentRectangle;
    private int _currentSelection;

    private Rectangle _previousRectangle;

    private int _previousSelection;

    private float _rectangleT;

    private Rectangle _selectionRectangle;
    private Selection[] _selections = null!;

    public Action<int>? OnSelect;

    public TextSelector(Selection[] selections, Renderer renderer, TextDrawer textDrawer)
    {
        _textDrawer = textDrawer;
        _renderer = renderer;
        Selections = selections;
        Selection = 0;
    }

    public Selection[] Selections
    {
        set
        {
            _selections = value;

            Array.Resize(ref _cachedSizes, _selections.Length);
            RecalculateSizes();
        }
    }

    public int Selection
    {
        set
        {
            _rectangleT = 0.0f;
            _previousSelection = _currentSelection;
            _currentSelection = Math.Clamp(value, 0, _selections.Length - 1);

            _previousRectangle = GetSelectionRectangle(_previousSelection);
            _currentRectangle = GetSelectionRectangle(_currentSelection);
        }
    }

    public void RecalculateSizes()
    {
        for (var i = 0; i < _cachedSizes.Length; i++)
            _cachedSizes[i] = _textDrawer.MeasureText(_selections[i].Caption);
    }

    public void SetSelectionImmediate(int selection)
    {
        _previousSelection = selection;
        _currentSelection = selection;
        _currentRectangle = GetSelectionRectangle(selection);
        _rectangleT = 1.0f;
    }

    public void Update()
    {
        _rectangleT += RectangleSpeed;
        if (_rectangleT > 1.0f)
            _rectangleT = 1.0f;

        _selectionRectangle = new Rectangle(
            (int) Mathematics.Lerp(_previousRectangle.X, _currentRectangle.X, _rectangleT),
            (int) Mathematics.Lerp(_previousRectangle.Y, _currentRectangle.Y, _rectangleT),
            (int) Mathematics.Lerp(_previousRectangle.Width, _currentRectangle.Width, _rectangleT),
            (int) Mathematics.Lerp(_previousRectangle.Height, _currentRectangle.Height, _rectangleT));
    }

    private Rectangle GetSelectionRectangle(int selection)
    {
        var x = 640 / 2 - _cachedSizes[selection].Width / 2;

        var y = _cachedSizes[0].Height;
        for (var i = 1; i <= selection; i++) y += _cachedSizes[i].Height;

        return new Rectangle(x, y, _cachedSizes[selection].Width, _cachedSizes[selection].Height);
    }

    public void Draw()
    {
        _renderer.DrawColor = Color.Blue;
        _renderer.FillRect(_selectionRectangle);

        for (var i = 0; i < _selections.Length; i++)
        {
            var rect = GetSelectionRectangle(i);
            _textDrawer.DrawText(rect.X, rect.Y, _selections[i].Caption, _selections[i].Color);
        }
    }

    public void IncreaseSelection()
    {
        var selection = _currentSelection;

        do
        {
            selection = (selection + 1) % _selections.Length;
        } while (!_selections[selection].Selectable);

        Selection = selection;
    }

    public void Select()
    {
        OnSelect?.Invoke(_currentSelection);
    }

    public void DecreaseSelection()
    {
        var selection = _currentSelection;

        do
        {
            --selection;

            if (selection < 0) selection = _selections.Length - 1;
        } while (!_selections[selection].Selectable);

        Selection = selection;
    }
}