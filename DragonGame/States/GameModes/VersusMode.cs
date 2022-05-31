#region

using System;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Selector;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.Input;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes;

internal class VersusSettingsSelector : TextSelector
{
    public enum VersusAction
    {
        None,
        PlayAgain,
        Quit
    }

    private readonly byte[] _scores = new byte[Match.PlayerCount];

    public VersusAction Action;

    public VersusSettingsSelector(Font font) : base(font)
    {
    }


    public void IncreaseScore(byte playerId)
    {
        _scores[playerId] = (byte) Math.Clamp(_scores[playerId] + 1, 0, 99);
    }

    public override void Update()
    {
        Begin();

        Break(30.0f);
        Label($"{_scores[0]} - {_scores[1]}");
        Break(30.0f);

        if (Button("PLAY AGAIN")) Action = VersusAction.PlayAgain;

        if (Button("QUIT")) Action = VersusAction.Quit;
        End();
        base.Update();
    }
}

internal class VersusMode : IGameState
{
    private readonly GameInfo _info;
    private AudioClip _gameMusic;

    private bool _inSelection;

    private Font _selectionFont;
    private VersusSettingsSelector _selector;

    public VersusMode(GameInfo info)
    {
        _info = info;
    }

    public void Initialize()
    {
        _selectionFont = new Font("public-pixel-30");
        _selector = new VersusSettingsSelector(_selectionFont);
        _gameMusic = new AudioClip("gameplay", true);

        Audio.PlayMusic(_gameMusic);
        Match.Initialize(_info);
    }

    public void Exit()
    {
        _selectionFont.Dispose();
        _gameMusic.Dispose();
        MatchAssets.Unload();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if (!_inSelection)
        {
            if (Match.IsOver)
            {
                _selector.IncreaseScore((byte) Match.SetWinner);
                _inSelection = true;
                return;
            }

            Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];

            for (var i = 0; i < Match.PlayerCount; i++) inputs[i] = Settings.MyData.GetInput(i);

            Match.Update(inputs);
        }
        else
        {
            _selector.Update();

            switch (_selector.Action)
            {
                case VersusSettingsSelector.VersusAction.None:
                    break;
                case VersusSettingsSelector.VersusAction.PlayAgain:
                    Match.Initialize(_info);
                    _selector.Action = VersusSettingsSelector.VersusAction.None;
                    _inSelection = false;
                    break;
                case VersusSettingsSelector.VersusAction.Quit:
                    GameFlow.Set(new MainMenuState());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void Draw()
    {
        Match.Draw();
        if (_inSelection) _selector.Draw();
    }
}