#region

using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Subsystems.Graphical;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Selector;

public abstract class TextSelector
{
    private readonly List<QueuedDraw> _drawList = new();
    private readonly Font _font;
    private uint _currentState;

    private float _currentYStride;

    private bool _keepTextAlive;

    private int _mouseX;
    private int _mouseY;

    private uint _previousState;

    private TextInputData _textInputData;

    protected TextSelector(Font font)
    {
        _font = font;
    }

    public bool LeftClick =>
        (_previousState & SDL.SDL_BUTTON_LMASK) == 0 && (_currentState & SDL.SDL_BUTTON_LMASK) != 0;

    public void OnEvent(ref SDL.SDL_Event @event)
    {
        if (@event.type == SDL.SDL_EventType.SDL_TEXTINPUT && _textInputData != null)
            unsafe
            {
                fixed (byte* textPtr = @event.text.text)
                {
                    var length = 0;
                    var ptr = textPtr;

                    while (*ptr++ != 0) ++length;

                    _textInputData.Text += new string(Encoding.UTF8.GetString(textPtr, length));


                    if (_textInputData.Text.Length > _textInputData.MaxLength)
                    {
                        _textInputData.Text = _textInputData.Text.Remove(_textInputData.MaxLength);
                        StopTextInput();
                    }
                }
            }
    }

    public virtual void Update()
    {
        _previousState = _currentState;
        _currentState = SDL.SDL_GetMouseState(out _mouseX, out _mouseY);
    }

    protected void Begin()
    {
        _keepTextAlive = false;
        _drawList.Clear();
        _currentYStride = 0.0f;
    }

    protected void Label(string caption)
    {
        Label(caption, Color.White);
    }

    protected void Label(string caption, Color color, float scale = 1.0f, Font.OnTextDraw onTextDraw = null)
    {
        var size = _font.MeasureString(caption) * scale;

        _drawList.Add(new QueuedDraw(caption, color,
            new Vector2(Graphics.LogicalSize.Width / 2.0f - size.Width / 2.0f, _currentYStride), scale, onTextDraw));

        _currentYStride += size.Height;
    }

    protected void Break(float space)
    {
        _currentYStride += space;
    }

    public void Refresh()
    {
        StopTextInput();
    }

    protected bool Button(string caption, float scale = 1.0f, Font.OnTextDraw onTextDraw = null)
    {
        var size = _font.MeasureString(caption) * scale;

        var x = Graphics.LogicalSize.Width / 2.0f - size.Width / 2.0f;
        var y = _currentYStride;
        var rect = new RectangleF(x, y,
            size.Width, size.Height);

        var inMouseRange = rect.Contains(_mouseX, _mouseY);

        Label(caption, inMouseRange ? Color.White : Color.Gray, scale, onTextDraw);
        return inMouseRange && LeftClick;
    }

    private void StopTextInput()
    {
        SDL.SDL_StopTextInput();
        _textInputData = null;
    }

    protected void Text(TextInputData input, float scale = 1.0f, Font.OnTextDraw onTextDraw = null)
    {
        var selected = _textInputData == input;
        var str = "[" + input.Text + new string(selected ? '_' : ' ', input.MaxLength - input.Text.Length) + "]";

        if (_textInputData == input)
        {
            _keepTextAlive = true;
            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE) && _textInputData.Text.Length > 0)
                _textInputData.Text = _textInputData.Text[..^1];
            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_RETURN)) StopTextInput();
            Label(str, Color.White, scale, onTextDraw);
        }
        else
        {
            if (!Button(str, scale, onTextDraw)) return;

            SDL.SDL_StartTextInput();
            _textInputData = input;
            _keepTextAlive = true;
        }
    }

    protected void End()
    {
        if (!_keepTextAlive) StopTextInput();
    }

    public void Draw()
    {
        foreach (var draw in _drawList)
        {
            var transformation = Matrix3x2.CreateScale(draw.Scale) * Matrix3x2.CreateTranslation(draw.Position);
            _font.Draw(draw.Caption, transformation, draw.Color, draw.OnTextDraw);
        }
    }

    protected class TextInputData
    {
        public int MaxLength;
        public string Text;
    }

    private readonly struct QueuedDraw
    {
        public readonly string Caption;
        public readonly Color Color;
        public readonly Vector2 Position;
        public readonly float Scale;
        public readonly Font.OnTextDraw OnTextDraw;

        public QueuedDraw(string caption, Color color, Vector2 position, float scale, Font.OnTextDraw onTextDraw)
        {
            Caption = caption;
            Color = color;
            Position = position;
            Scale = scale;
            OnTextDraw = onTextDraw;
        }
    }
}