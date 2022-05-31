#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Subsystems.Output;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.Input;
using SDL2;

#endregion

namespace DuckDuckJump.States.GameModes.NetworkMode;

public abstract class NetworkMode : IGameState
{
    private readonly GameInput[] _inputs = new GameInput[Match.PlayerCount];
    private readonly Stopwatch _stopwatch = new();

    protected readonly object SocketLock = new();

    private Font _font;

    private bool _hasConnection;

    private long _lockstepTimer;
    private GameInput _mine;
    private string _pingString = string.Empty;
    private Size _size;
    protected byte MyInputIndex, OtherInputIndex;
    protected Socket MySocket, OtherSocket;

    public void Initialize()
    {
        _font = new Font("public-pixel-30");
        Task.Run(ListenThread);
    }

    public void Exit()
    {
        _font.Dispose();
        lock (SocketLock)
        {
            MySocket?.Close();
            OtherSocket?.Close();
        }
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        try
        {
            if (Match.IsOver)
            {
                GameFlow.Set(new MainMenuState());
                return;
            }

            if (!_hasConnection)
                return;

            if (_lockstepTimer == 0)
            {
                Span<byte> buffer = stackalloc byte[1];

                _stopwatch.Restart();
                lock (SocketLock)
                {
                    OtherSocket.Receive(buffer);
                }

                _stopwatch.Stop();

                _pingString = $"{_stopwatch.ElapsedMilliseconds} ms";
                _size = _font.MeasureString(_pingString);

                _inputs[MyInputIndex] = _mine;
                _inputs[OtherInputIndex] = (GameInput) buffer[0];

                KickoffLockstep(_stopwatch.ElapsedMilliseconds / 16);
            }
            else
            {
                --_lockstepTimer;
            }

            Match.Update(_inputs);
        }
        catch (Exception e)
        {
            Error.RaiseException(e);
        }
    }

    public void Draw()
    {
        if (_hasConnection)
        {
            Match.Draw();
            _font.Draw(_pingString,
                Matrix3x2.CreateScale(0.5f, 0.5f) *
                Matrix3x2.CreateTranslation(0.0f, Graphics.LogicalSize.Height - _size.Height),
                Color.Red);
        }

        else
        {
            _font.Draw("CONNECTING", Matrix3x2.Identity, Color.White);
        }
    }

    protected abstract void EstablishConnection();

    private void ListenThread()
    {
        try
        {
            _hasConnection = false;
            EstablishConnection();
            KickoffLockstep(0);
            _hasConnection = true;
        }
        catch (Exception e)
        {
            Error.RaiseException(e);
        }
    }

    private void KickoffLockstep(long roundTripTime)
    {
        _mine = Settings.MyData.GetInput(0);

        Span<byte> buffer = stackalloc byte[1];
        buffer[0] = (byte) _mine;

        lock (SocketLock)
        {
            OtherSocket.Send(buffer);
        }

        _lockstepTimer = roundTripTime;
    }
}