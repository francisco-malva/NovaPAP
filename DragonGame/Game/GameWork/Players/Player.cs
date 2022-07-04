#region

using System;
using System.Collections.Generic;
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

internal class Player
{
    private const float YDamping = 0.45f;
    private const float MaxXVelocity = 15.0f;
    private const float JumpVelocity = -16.25f;
    private static readonly SizeF BodyExtents = new(45, 50);

    private static readonly Vector2[] InitialPositions =
    {
        new(Graphics.Midpoint.X / 2.0f, Graphics.LogicalSize.Height - BodyExtents.Height),
        new(Graphics.Midpoint.X * 1.5f, Graphics.LogicalSize.Height - BodyExtents.Height)
    };

    private static readonly Vector2 JumpMaxPoint = new(0.0f, 480.0f / 1.35f);

    private static readonly ComActionData[] ComActionTable =
    {
        new(1, 1, 1, 1, 1),
        new(1, 1, 1, 1, 2),
        new(1, 1, 1, 2, 2),
        new(1, 1, 2, 2, 2),
        new(1, 2, 2, 2, 2),
        new(2, 2, 2, 2, 2),
        new(2, 2, 2, 2, 3),
        new(2, 2, 2, 3, 3)
    };

    private readonly List<ComPathPoint> _comPath = new();

    private short _comPathIndex;
    private float _comProgress;
    private byte _freezeFrames;

    private short _lastJumpedPlatform;
    private byte _myPlayerIndex;
    private byte _reviveFrames;

    private byte _slowdownFrames;
    private float _targetXVelocity;
    private byte _umbrellaFrames;

    private Vector2 _velocity;
    private float _xVelocity;
    private float _xVelocityDelta;

    private float _yPosDelta;
    private float _yVelocity;

    public bool Lost;
    public Vector2 Position;

    public Player()
    {
        Position = default;
        Lost = false;
        _myPlayerIndex = 0;
        _slowdownFrames = 0;
        _freezeFrames = 0;
        _reviveFrames = 0;
        _lastJumpedPlatform = 0;
        _yPosDelta = 0;
        _comPathIndex = 0;
        _comProgress = 0;
        _velocity = default;
        _xVelocity = 0;
        _targetXVelocity = 0;
        _xVelocityDelta = 0;
        _yVelocity = 0;
        _umbrellaFrames = 0;
    }

    public RectangleF PushBox => new(Position.X, Position.Y, BodyExtents.Width, BodyExtents.Height);

    public RectangleF PlatformBox =>
        new(Position.X + BodyExtents.Width * 0.25f, Position.Y + BodyExtents.Height * 0.5f,
            BodyExtents.Width * 0.5f, BodyExtents.Height * 0.5f);

    private bool Descending => _yPosDelta > 5f;

    public short LastJumpedPlatform => _lastJumpedPlatform;

    private bool NoTick => HasFreeze() || (HasSlowdown() && Match.UpdateFrameCount % 2 == 0);

    public bool IsCom => MyComLevel > 0;
    private byte MyComLevel => Match.Info.ComInfo.Levels[_myPlayerIndex];
    private ref ComActionData MyComData => ref ComActionTable[MyComLevel - 1];

    public void Reset(byte playerIndex)
    {
        _myPlayerIndex = playerIndex;
        Lost = false;

        if (IsCom) ResetComFields();


        _slowdownFrames = 0;
        _freezeFrames = 0;
        _reviveFrames = 0;
        _umbrellaFrames = 0;

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

    public void Update(GameInput input)
    {
        switch (Match.State)
        {
            case Match.MatchState.InGame:
                UpdateInGame(input);
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

    private void UpdateInGame(GameInput input)
    {
        TickItemTimers();

        if (NoTick)
            return;

        if (IsCom)
            ComGameUpdate();
        else
            UpdateHumanPlayer(input);

        FallDetectionUpdate();
    }

    private void UpdateHumanPlayer(GameInput input)
    {
        UpdateHumanVelocity(input);
        PlatformDetectionUpdate();
    }


    private void TickItemTimers()
    {
        if (HasRevive()) --_reviveFrames;
        if (HasSlowdown()) --_slowdownFrames;
        if (HasUmbrella()) --_umbrellaFrames;
        if (HasFreeze()) --_freezeFrames;
    }

    private bool HasUmbrella()
    {
        return _umbrellaFrames > 0;
    }

    private bool HasRevive()
    {
        return _reviveFrames > 0;
    }

    private bool HasFreeze()
    {
        return _freezeFrames > 0;
    }

    private bool HasSlowdown()
    {
        return _slowdownFrames > 0;
    }

    private void FallDetectionUpdate()
    {
        if (CameraWork.Camera.Bounds.IntersectsWith(PushBox) || !(Position.Y > CameraWork.Camera.Bounds.Bottom)) return;

        if (HasRevive())
        {
            _reviveFrames = 0;
            Jump(1.5f);
        }
        else
        {
            Lost = true;
        }
    }

    private void PlatformDetectionUpdate()
    {
        if (!Descending)
            return;

        if (!PlatformWork.GetIntersectingPlatform(this, out _lastJumpedPlatform)) return;

        ref var platform = ref PlatformWork.GetPlatform(_lastJumpedPlatform);

        Position.Y = platform.Position.Y - BodyExtents.Height;

        CorrectCameraHeight(_lastJumpedPlatform);
        Jump();
    }

    private static void CorrectCameraHeight(short targetId)
    {
        BackgroundWork.SetTarget(targetId);

        ref var newTarget = ref PlatformWork.GetPlatform(targetId);

        var transformedY = newTarget.Position.Y - Graphics.Midpoint.Y * 1.5f;
        if (transformedY < CameraWork.Target.Y)
            CameraWork.Target = CameraWork.Target with { Y = transformedY };
    }

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
        _yVelocity += HasUmbrella() ? YDamping * 0.85F : YDamping;

        _velocity = new Vector2(_xVelocity, _yVelocity);
        Position += _velocity;
    }

    public void ApplySpeed()
    {
        if (NoTick)
            return;

        var prev = Position;

        if (IsCom)
            ApplyComSpeed();
        else
            ApplyHumanSpeed();

        _yPosDelta = Position.Y - prev.Y;
    }

    public void Jump(float multiplier = 1.0f)
    {
        JumpSfx();
        var velocity = JumpVelocity * multiplier;
        _yVelocity = velocity;
    }

    private static void JumpSfx()
    {
        SoundEffectWork.Queue(MatchAssets.SfxIndex.Jump, 0.25f);
    }

    public void DrawMe()
    {
        Color color;
        if (HasFreeze())
            color = Color.Aqua;
        else if (HasSlowdown())
            color = Color.DarkGray;
        else
            color = Color.White;

        var humanSprite = _myPlayerIndex == 0 ? MatchAssets.TextureIndex.Player : MatchAssets.TextureIndex.Player2;
        var comSprite = _myPlayerIndex == 0 ? MatchAssets.TextureIndex.PlayerAi : MatchAssets.TextureIndex.PlayerAi2;
        Graphics.Draw(MatchAssets.Texture(IsCom ? comSprite : humanSprite), null,
            Matrix3x2.CreateTranslation(Position), color);

        if (HasUmbrella())
            Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.UmbrellaItem), null,
                Matrix3x2.CreateTranslation(Position.X + 32.0f, Position.Y - 32.0f), Color.White);
    }

    private void GenerateComPath()
    {
        _comPath.Clear();
        InsertIntoPath(-1);
        InsertIntoPath(0);
        short currentProgress = 0;

        var offset = RandomWork.Next(0, MyComData.BehaviorTable.Length);
        while (currentProgress < Match.Info.PlatformCount)
        {
            var next = MyComData.BehaviorTable[offset];
            offset = RandomWork.Next(0, MyComData.BehaviorTable.Length);

            var nextPlatform =
                (short)(currentProgress +
                        next);

            InsertIntoPath(nextPlatform);
            currentProgress = nextPlatform;
        }
    }

    private void InsertIntoPath(short position)
    {
        _comPath.Add(new ComPathPoint(position, RandomWork.Next(-20.0f, 20.0f)));
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
        _lastJumpedPlatform = _comPath[_comPathIndex].Index;

        if (_lastJumpedPlatform >= 0)
            CorrectCameraHeight(_lastJumpedPlatform);
    }

    public void ApplyFreeze()
    {
        _freezeFrames = 40;
    }

    public void ApplySlowdown()
    {
        _slowdownFrames = 60;
    }

    public void ApplyUmbrella()
    {
        _umbrellaFrames = 60 * 3;
    }

    public void ApplyRevive()
    {
        _reviveFrames = 30;
    }

    private void ApplyComSpeed()
    {
        var progress = Math.Clamp(_comProgress, 0.0f, 1.0f);

        var beginIndex = Math.Clamp(_comPathIndex, -1, _comPath.Count - 1);

        var begin = GetTargetPosition(_comPath[beginIndex].Index);
        begin.X += _comPath[beginIndex].Offset;

        var endIndex = Math.Clamp(_comPathIndex + 1, 0, _comPath.Count - 1);

        var end = GetTargetPosition(_comPath[endIndex].Index);
        end.X += _comPath[endIndex].Offset;

        var p2 = begin - JumpMaxPoint;
        var p3 = end - JumpMaxPoint;

        Position = Mathematics.CubicBezier(begin, p2, p3, end, progress);
    }

    private void UpdateHumanVelocity(GameInput input)
    {
        _targetXVelocity = 0.0f;

        if (input.HasFlag(GameInput.Left)) _targetXVelocity -= MaxXVelocity;
        if (input.HasFlag(GameInput.Right)) _targetXVelocity += MaxXVelocity;
    }

    private readonly struct ComActionData
    {
        public readonly sbyte[] BehaviorTable;

        public ComActionData(params sbyte[] behaviorTable)
        {
            BehaviorTable = behaviorTable;
        }
    }
}

internal struct ComPathPoint
{
    public readonly short Index;
    public readonly float Offset;

    public ComPathPoint(short index, float offset)
    {
        Index = index;
        Offset = offset;
    }
}