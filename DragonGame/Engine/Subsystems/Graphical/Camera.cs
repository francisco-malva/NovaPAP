#region

using System.Drawing;
using System.IO;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Utilities;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Graphical;

public class Camera
{
    private Matrix3x2 _matrix = Matrix3x2.Identity;
    private Vector2 _position;

    private bool _regenerateMatrix;

    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            _regenerateMatrix = true;
        }
    }

    public RectangleF Bounds => new(Position.X, Position.Y, Graphics.LogicalSize.Width, Graphics.LogicalSize.Height);

    public Matrix3x2 Matrix
    {
        get
        {
            if (!_regenerateMatrix) return _matrix;

            _matrix = Matrix3x2.CreateTranslation(-_position);
            _regenerateMatrix = false;

            return _matrix;
        }
    }

    public void Save(Stream stream)
    {
        stream.Write(_position);
    }

    public void Load(Stream stream)
    {
        _position = stream.Read<Vector2>();
        _regenerateMatrix = true;
    }
}