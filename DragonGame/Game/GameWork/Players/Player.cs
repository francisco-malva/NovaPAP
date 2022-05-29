#region

using System;
using System.Drawing;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.GameWork.Background;
using DuckDuckJump.Game.GameWork.Camera;
using DuckDuckJump.Game.GameWork.Platforming;
using DuckDuckJump.Game.GameWork.Rng;
using DuckDuckJump.Game.GameWork.Sound;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game.GameWork.Players;

internal unsafe struct Player
{
    private static readonly SizeF BodyExtents = new(45, 50);
    public Vector2 Position;
    public RectangleF PushBox => new(Position.X, Position.Y, BodyExtents.Width, BodyExtents.Height);

    public RectangleF PlatformBox =>
        new(Position.X + BodyExtents.Width * 0.25f, Position.Y + BodyExtents.Height * 0.5f,
            BodyExtents.Width * 0.5f, BodyExtents.Height * 0.5f);

    private bool Descending => _yPosDelta > 5f;

    private static readonly Vector2[] InitialPositions =
    {
        new(Graphics.Midpoint.X / 2.0f, Graphics.LogicalSize.Height - BodyExtents.Height),
        new(Graphics.Midpoint.X * 1.5f, Graphics.LogicalSize.Height - BodyExtents.Height)
    };

    public bool Lost;
    private byte _myPlayerIndex;

    public void Reset(byte playerIndex)
    {
        _myPlayerIndex = playerIndex;
        Lost = false;

        if (IsCom) ResetComFields();


        _lastJumpedPlatform = -1;

        _velocity = Vector2.Zero;
        _xVelocity = 0.0f;
        _targetXVelocity = 0.0f;
        _xVelocityDelta = 0.0f;
        _yVelocity = 0.0f;

        Position = InitialPositions[playerIndex];
    }

    public void ClampPosition()
    {
        Position.X = Math.Clamp(Position.X, 0.0f, Graphics.LogicalSize.Width - BodyExtents.Width);

        if (!(Position.Y > Graphics.LogicalSize.Height - BodyExtents.Height)) return;
        Position.Y = Graphics.LogicalSize.Height - BodyExtents.Height;
        Jump();
    }

    public void UpdateMe(GameInput input)
    {
        switch (Match.State)
        {
            case Match.MatchState.InGame:
                if (IsCom)
                {
                    ComGameUpdate();
                }
                else
                {
                    HumanGameUpdate(input);
                    PlatformDetectionUpdate();
                }

                FallDetectionUpdate();
                break;

            case Match.MatchState.Winner:
                break;
            case Match.MatchState.GetReady:
                break;
            case Match.MatchState.NotInitialized:
                break;
            case Match.MatchState.Over:
                break;
            case Match.MatchState.TimeUp:
                break;
            case Match.MatchState.BeginningMessage:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FallDetectionUpdate()
    {
        if (!CameraWork.Camera.Bounds.IntersectsWith(PushBox) && Position.Y > CameraWork.Camera.Bounds.Bottom)
            Lost = true;
    }

    private void PlatformDetectionUpdate()
    {
        if (!Descending)
            return;

        if (!PlatformWork.GetIntersectingPlatform(ref this, out _lastJumpedPlatform)) return;

        ref var platform = ref PlatformWork.GetPlatform(_lastJumpedPlatform);

        Position.Y = platform.Position.Y - BodyExtents.Height;

        CorrectCameraHeight(_lastJumpedPlatform);
        Jump();
    }

    private short _lastJumpedPlatform;

    public short LastJumpedPlatform => _lastJumpedPlatform;

    private void CorrectCameraHeight(short targetId)
    {
        BackgroundWork.SetTarget(targetId);

        ref var newTarget = ref PlatformWork.GetPlatform(targetId);

        var transformedY = newTarget.Position.Y - Graphics.Midpoint.Y * 1.5f;
        if (transformedY < CameraWork.Target.Y)
            CameraWork.Target = CameraWork.Target with { Y = transformedY };
    }

    private float _yPosDelta;

    private static readonly Vector2 JumpMaxPoint = new(0.0f, 480.0f / 1.35f);

    private Vector2 GetTargetPosition(short targetIndex)
    {
        if (targetIndex < 0) return InitialPositions[_myPlayerIndex];

        return PlatformWork.GetPlatform(targetIndex).Position +
               Vector2.UnitX * Platform.Extents.Width / 2.0f -
               Vector2.UnitX * BodyExtents.Width / 2.0f -
               Vector2.UnitY * BodyExtents.Height;
    }

    private void ApplyHumanSpeed()
    {
        _xVelocity = Mathematics.SmoothDamp(_xVelocity, _targetXVelocity, ref _xVelocityDelta, 0.1f,
            GameFlow.TimeStep);
        _yVelocity += YDamping;

        _velocity = new Vector2(_xVelocity, _yVelocity);
        Position += _velocity;
    }

    public void ApplySpeed()
    {
        var prev = Position;

        if (IsCom)
            ApplyComSpeed();
        else
            ApplyHumanSpeed();

        _yPosDelta = Position.Y - prev.Y;
    }

    private void Jump(float multiplier = 1.0f)
    {
        JumpSfx();
        var velocity = JumpVelocity * multiplier;
        _yVelocity = velocity;
    }

    private static void JumpSfx()
    {
        SoundEffectWork.Queue(MatchAssets.SfxIndex.Jump, 0.25f);
    }

    private static readonly Color ComColor = Color.FromArgb(128, 128, 128, 128);

    public void DrawMe()
    {
        var color = IsCom ? ComColor : Color.White;
        Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.Player), null,
            Matrix3x2.CreateTranslation(Position), color);
    }

    private readonly struct ComActionData
    {
        public readonly byte PlatformRange;
        public readonly float StoppedChance;

        public ComActionData(byte platformRange, float stoppedChance)
        {
            PlatformRange = platformRange;
            StoppedChance = stoppedChance;
        }
    }

    private static readonly ComActionData[] ComActionTable =
    {
        new(1, 0.5f),
        new(1, 0.25f),
        new(2, 0.015f),
        new(2, 0.005f),
        new(3, 0.005f),
        new(3, 0.0025f),
        new(4, 0.0015f),
        new(4, 0.0f)
    };

    public bool IsCom => Match.Info.ComLevels[_myPlayerIndex] > 0;
    private byte MyComLevel => Match.Info.ComLevels[_myPlayerIndex];
    private ref ComActionData MyComData => ref ComActionTable[MyComLevel - 1];
    private short _comPathIndex;
    private float _comProgress;


    private const short MaxPathSize = 2048;
#pragma warning disable CS0649
    private fixed short _comPath[MaxPathSize];
#pragma warning restore CS0649
#pragma warning disable CS0649
    private fixed float _randomOffsets[MaxPathSize];
#pragma warning restore CS0649
    private short _comPathLength;

    private void GenerateComPath()
    {
        _comPathLength = 0;

        InsertIntoPath(-1);
        InsertIntoPath(0);
        short currentProgress = 0;

        while (currentProgress < Match.Info.PlatformCount)
        {
            var shouldStop = MyComData.StoppedChance != 0.0f &&
                             RandomWork.Next(0.0f, 1.0f) <= MyComData.StoppedChance;

            if (shouldStop)
            {
                GenerateComStopped(currentProgress);
            }
            else
            {
                ++currentProgress;
                var nextPlatform =
                    (short)(currentProgress + RandomWork.Next((byte)1, MyComData.PlatformRange));
                InsertIntoPath(currentProgress);
                InsertIntoPath(nextPlatform);
                currentProgress = nextPlatform;
            }
        }

        for (var i = 0; i < _comPathLength; i++) _randomOffsets[i] = RandomWork.Next(-20.0f, 20.0f);
    }

    private void GenerateComStopped(short position)
    {
        InsertIntoPath(position);
        InsertIntoPath(position);
    }

    private void InsertIntoPath(short position)
    {
        _comPath[_comPathLength++] = position;
    }

    private void ResetComFields()
    {
        _comPathIndex = 0;
        _comProgress = 0.0f;

        GenerateComPath();
    }

    private void ComGameUpdate()
    {
        _comProgress += GameFlow.TimeStep;

        if (!(_comProgress >= 1.0f)) return;

        JumpSfx();
        _comProgress %= 1.0f;
        ++_comPathIndex;
        _lastJumpedPlatform = _comPath[_comPathIndex];

        if (_lastJumpedPlatform >= 0)
            CorrectCameraHeight(_lastJumpedPlatform);
    }

    private void ApplyComSpeed()
    {
        var progress = Math.Clamp(_comProgress, 0.0f, 1.0f);

        var begin = GetTargetPosition(_comPath[_comPathIndex]);
        begin.X += _randomOffsets[_comPathIndex];

        var endIndex = Math.Clamp(_comPathIndex + 1, 0, _comPathLength - 1);

        var end = GetTargetPosition(_comPath[endIndex]);
        end.X += _randomOffsets[endIndex];

        var p2 = begin - JumpMaxPoint;
        var p3 = end - JumpMaxPoint;

        Position = Mathematics.CubicBezier(begin, p2, p3, end, progress);
    }

    private Vector2 _velocity;
    private float _xVelocity;
    private float _targetXVelocity;
    private float _xVelocityDelta;
    private float _yVelocity;
    private const float YDamping = 0.45f;
    private const float MaxXVelocity = 15.0f;
    private const float JumpVelocity = -16.25f;

    private void HumanGameUpdate(GameInput input)
    {
        _targetXVelocity = 0.0f;

        if (input.HasFlag(GameInput.Left)) _targetXVelocity -= MaxXVelocity;
        if (input.HasFlag(GameInput.Right)) _targetXVelocity += MaxXVelocity;
    }
}