#region

using System;
using System.Drawing;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Selector;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.GameWork.Banner;
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
        new(new ComInfo(), 10, 0, 2, 60 * 60, BannerWork.MessageIndex.S1, GameInfo.Flags.NoItems),
        new(new ComInfo(), 15, 0, 2, 60 * 60, BannerWork.MessageIndex.S2, GameInfo.Flags.NoItems),
        new(new ComInfo(), 20, 0, 2, 60 * 60, BannerWork.MessageIndex.S3, GameInfo.Flags.NoItems),
        new(new ComInfo(), 25, 0, 2, 60 * 60, BannerWork.MessageIndex.S4, GameInfo.Flags.NoItems),
        new(new ComInfo(), 30, 0, 2, 60 * 60, BannerWork.MessageIndex.S5, GameInfo.Flags.NoItems),
        new(new ComInfo(), 35, 0, 2, 60 * 60, BannerWork.MessageIndex.S6, GameInfo.Flags.NoItems),
        new(new ComInfo(), 40, 0, 2, 60 * 60, BannerWork.MessageIndex.S7, GameInfo.Flags.NoItems),
        new(new ComInfo(), 50, 0, 2, 60 * 60, BannerWork.MessageIndex.S8, GameInfo.Flags.NoItems)
    };

    private byte _comLevel = 1;

    private AudioClip _gameMusic;

    private bool _lost;

    private PauseMenu _pauseMenu;

    private ContinueSelector _selector;
    private Font _selectorFont;
    private byte _stage;
    private Font _stageFont;

    private string _stageString;
    private SizeF _stageStringSize;

    private uint _timer;

    public void Initialize()
    {
        _gameMusic = new AudioClip("gameplay", true);
        _timer = 0;
        _stageFont = new Font("terminator-two-20");
        _selectorFont = new Font("public-pixel-30");

        _pauseMenu = new PauseMenu();

        _selector = new ContinueSelector(_selectorFont);
        Audio.PlayMusic(_gameMusic);

        AdvanceStage();
    }

    public void Exit()
    {
        _gameMusic.Dispose();
        _pauseMenu.Dispose();
        _stageFont.Dispose();
        _selectorFont.Dispose();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if (_lost)
        {
            _selector.Update();


            switch (_selector.Action)
            {
                case ContinueSelectorAction.None:
                    break;
                case ContinueSelectorAction.TryAgain:
                    _selector.Action = ContinueSelectorAction.None;
                    _lost = false;
                    --_stage;
                    _comLevel = (byte)Math.Clamp(_comLevel - 1, 1, 8);
                    AdvanceStage();
                    break;
                case ContinueSelectorAction.Quit:
                    GameFlow.Set(new MainMenuState());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;
        }

        if (Match.IsOver)
        {
            if (Match.SetWinner == MatchWinner.P1)
            {
                _comLevel = (byte)Math.Clamp(_comLevel + 1, 1, 8);

                if (_stage == StageCount)
                {
                    Socket socket = null;
                    try
                    {
                        socket = ScoringServer.ConnectToScoringServer();
                        socket.Send(
                            Encoding.UTF8.GetBytes(
                                $"(\"UpdateScore\" \"{Settings.MyData.Nickname.ToString()}\" {_timer})"));
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                    finally
                    {
                        socket?.Close();
                    }

                    GameFlow.Set(new MainMenuState());
                }
                else
                {
                    AdvanceStage();
                }
            }
            else
            {
                _lost = true;
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
        if (_lost)
        {
            _selector.Draw();
        }
        else
        {
            Match.Draw();
            DrawTimeAttackInfo();
            _pauseMenu.Draw();
        }
    }

    private void AdvanceStage()
    {
        ++_stage;
        var info = StageSettingTable[_stage - 1];
        info.RandomSeed = Environment.TickCount;
        info.ComInfo.Levels[1] = _comLevel;

        Match.Initialize(info);
        Audio.MusicFade = 1.0f;
        SetStageLabel();
    }

    private void DrawTimeAttackInfo()
    {
        if (Match.State != Match.MatchState.InGame)
            return;
        _stageFont.Draw(_stageString,
            Matrix3x2.CreateTranslation(Graphics.LogicalSize.Width - _stageStringSize.Width - 10.0f,
                Graphics.LogicalSize.Height - _stageStringSize.Height - 10.0f), Color.DarkGoldenrod);

        var seconds = _timer == 0 ? 0 : _timer / 60;
        var minutes = seconds == 0 ? 0 : seconds / 60;

        var timerString = $"{minutes:00}:{seconds % 60:00}";
        var timerStringSize = _stageFont.MeasureString(timerString);

        _stageFont.Draw(timerString,
            Matrix3x2.CreateTranslation(timerStringSize.Width / 2.0f,
                Graphics.LogicalSize.Height - timerStringSize.Height - 10.0f), Color.DarkGoldenrod);
    }

    private void SetStageLabel()
    {
        _stageString = $"STAGE {_stage}/{StageCount}";
        _stageStringSize = _stageFont.MeasureString(_stageString);
    }

    private enum ContinueSelectorAction : byte
    {
        None,
        TryAgain,
        Quit
    }

    private class ContinueSelector : TextSelector
    {
        public ContinueSelectorAction Action;

        public ContinueSelector(Font font) : base(font)
        {
        }

        public override void Update()
        {
            Begin();

            Break(20.0f);
            Label("YOU LOST!");
            Break(20.0f);

            if (Button("TRY AGAIN")) Action = ContinueSelectorAction.TryAgain;

            if (Button("QUIT")) Action = ContinueSelectorAction.Quit;

            End();
            base.Update();
        }
    }
}