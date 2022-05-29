#region

using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;

#endregion

namespace DuckDuckJump.Game.GameWork.Camera;

public static class CameraWork
{
    public static readonly Engine.Subsystems.Graphical.Camera Camera;
    public static Vector2 Target;
    private static float _cameraX;
    private static float _cameraY;
    private static float _cameraXVelocity;
    private static float _cameraYVelocity;

    static CameraWork()
    {
        Camera = new Engine.Subsystems.Graphical.Camera();
    }

    public static void Reset()
    {
        Target = Vector2.Zero;
        _cameraX = 0.0f;
        _cameraY = 0.0f;
        _cameraXVelocity = 0.0f;
        _cameraYVelocity = 0.0f;
    }

    public static void UpdateMe()
    {
        _cameraX = Mathematics.SmoothDamp(_cameraX, Target.X, ref _cameraXVelocity, 0.35f, GameFlow.TimeStep);
        _cameraY = Mathematics.SmoothDamp(_cameraY, Target.Y, ref _cameraYVelocity, 0.35f, GameFlow.TimeStep);

        Camera.Position = new Vector2(_cameraX, _cameraY);
    }
}