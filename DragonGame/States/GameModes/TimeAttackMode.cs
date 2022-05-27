#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.GameWork;
using DuckDuckJump.Game.Input;
using DuckDuckJump.Game.Pausing;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes;

public class TimeAttackMode : IGameState
{
    private const byte StageCount = 8;

    private static readonly GameInfo[] StageSettingTable =
    {
        new(new ComLevels(0, 1), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.TimeAttackStart, GameInfo.Flags.None),
        new(new ComLevels(0, 2), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None),
        new(new ComLevels(0, 3), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None),
        new(new ComLevels(0, 4), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None),
        new(new ComLevels(0, 5), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None),
        new(new ComLevels(0, 6), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None),
        new(new ComLevels(0, 7), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None),
        new(new ComLevels(0, 8), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None)
    };

    private PauseMenu _pauseMenu;
    private byte _stage;
    private Font _stageFont;

    private string _stageString;
    private SizeF _stageStringSize;

    private State _state = State.InMatch;
    private uint _timer;

    public void Initialize()
    {
        _timer = 0;
        _stageFont = new Font("terminator-two-20", 20);
        _pauseMenu = new PauseMenu();

        AdvanceStage();
    }

    public void Exit()
    {
        _pauseMenu.Dispose();
        _stageFont.Dispose();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if (Match.IsOver)
        {
            if (Match.SetWinner == MatchWinner.P1)
            {
                if (_stage == StageCount)
                    GameFlow.Set(new MainMenuState());
                else
                    AdvanceStage();
            }
            else
            {
                --_stage;
                AdvanceStage();
            }
        }
        else
        {
            if (Match.State == Match.MatchState.InGame) _pauseMenu.Update();


            if (!_pauseMenu.Paused)
            {
                Span<GameInput> inputs = stackalloc GameInput[2];
                inputs[0] = Settings.MyData.GetInput(0);
                Match.Update(inputs);

                if (Match.State == Match.MatchState.InGame) ++_timer;
            }
            else
            {
                switch (_pauseMenu.Action)
                {
                    case PauseMenu.PauseAction.None:
                        break;
                    case PauseMenu.PauseAction.Resume:
                        break;
                    case PauseMenu.PauseAction.Quit:
                        GameFlow.Set(new MainMenuState());
                        break;
                    case PauseMenu.PauseAction.Reset:
                        GameFlow.Set(new TimeAttackMode());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public void Draw()
    {
        Match.Draw();
        DrawTimeAttackInfo();
        _pauseMenu.Draw();
    }

    private void AdvanceStage()
    {
        ++_stage;
        var info = StageSettingTable[_stage - 1];
        info.RandomSeed = Environment.TickCount;
        Match.Initialize(info);
        SetStageLabel();
    }

    private void DrawTimeAttackInfo()
    {
        _stageFont.Draw(_stageString,
            Matrix3x2.CreateTranslation(Graphics.LogicalSize.Width - _stageStringSize.Width - 10.0f,
                Graphics.LogicalSize.Height - _stageStringSize.Height - 10.0f), Color.Gray);

        var seconds = _timer == 0 ? 0 : _timer / 60;
        var minutes = seconds == 0 ? 0 : seconds / 60;

        var timerString = $"{minutes:00}:{seconds:00}";
        var timerStringSize = _stageFont.MeasureString(timerString);

        _stageFont.Draw(timerString,
            Matrix3x2.CreateTranslation(timerStringSize.Width / 2.0f,
                Graphics.LogicalSize.Height - timerStringSize.Height - 10.0f), Color.Gray);
    }

    private void SetStageLabel()
    {
        _stageString = $"STAGE {_stage}";
        _stageStringSize = _stageFont.MeasureString(_stageString);
    }

    private enum State : byte
    {
        InMatch,
        Continue
    }
}