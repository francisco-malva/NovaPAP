#region

using System;
using System.Drawing;
using System.Globalization;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.GameWork.Banner;
using DuckDuckJump.Game.GameWork.Players;
using DuckDuckJump.Game.Input;
using DuckDuckJump.Game.Pausing;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes;

public class EndlessClimberMode : IGameState
{
    private float _distanceClimbed;

    private AudioClip _gameMusic;

    private PauseMenu _pauseMenu;

    private Font _stageFont;
    private Font _versusSelectionFont;

    private VersusSettingsSelector _versusSettingsSelector;

    public void Initialize()
    {
        _gameMusic = new AudioClip("watch-mode", true);
        _pauseMenu = new PauseMenu();

        _versusSelectionFont = new Font("public-pixel-30");
        _versusSettingsSelector = new VersusSettingsSelector(_versusSelectionFont, false);
        _stageFont = new Font("terminator-two-20");

        Audio.PlayMusic(_gameMusic);
        Match.Initialize(new GameInfo(new ComInfo(0, 0), 1000, Environment.TickCount, 1, 60 * 60,
            BannerWork.MessageIndex.Climb, GameInfo.Flags.NoItems | GameInfo.Flags.EndlessClimber));
        _distanceClimbed = 0.0f;
    }

    public void Exit()
    {
        _gameMusic.Dispose();
        _pauseMenu.Dispose();
        _stageFont.Dispose();
        _versusSelectionFont.Dispose();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if (Match.IsOver)
        {
            _versusSettingsSelector.SetHeight(_distanceClimbed);
            _versusSettingsSelector.Update();

            switch (_versusSettingsSelector.Action)
            {
                case VersusSettingsSelector.VersusAction.None:
                    break;
                case VersusSettingsSelector.VersusAction.PlayAgain:
                    GameFlow.Set(new EndlessClimberMode());
                    break;
                case VersusSettingsSelector.VersusAction.Quit:
                    Socket socket = null;
                    try
                    {
                        socket = ScoringServer.ConnectToScoringServer();
                        socket.Send(
                            Encoding.UTF8.GetBytes(
                                $"(\"UpdateHeight\" \"{Settings.MyData.Nickname.ToString()}\" {((double)_distanceClimbed).ToString(CultureInfo.InvariantCulture)})"));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    finally
                    {
                        socket?.Close();
                    }

                    GameFlow.Set(new MainMenuState());
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
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

                _distanceClimbed = MathF.Abs(480 - PlayerWork.First.Position.Y);
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
                        GameFlow.Set(new EndlessClimberMode());
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
        DrawClimbingDistance();
        _pauseMenu.Draw();
        _versusSettingsSelector.Draw();
    }

    private void DrawClimbingDistance()
    {
        if (Match.State != Match.MatchState.InGame)
            return;
        var distanceString = $"{MathF.Truncate(_distanceClimbed)}m";

        var size = _stageFont.MeasureString(distanceString);
        _stageFont.Draw(distanceString,
            Matrix3x2.CreateTranslation(size.Width / 2.0f,
                Graphics.LogicalSize.Height - size.Height - 10.0f), Color.DarkGoldenrod);
    }
}