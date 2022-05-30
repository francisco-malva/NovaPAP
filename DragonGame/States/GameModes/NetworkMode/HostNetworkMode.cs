using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Output;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.GameWork.Banner;
using DuckDuckJump.Game.Input;
using SDL2;

namespace DuckDuckJump.States.GameModes.NetworkMode;

public class HostNetworkMode : IGameState
{
    private Socket _mySocket, _otherSocket;
    
    private long _lockstepTimer;
    private GameInput _mine;
    
    private readonly GameInput[] _inputs = new GameInput[2];
    private readonly Stopwatch _stopwatch = new();
    
    private void ListenThread()
    {
        var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
        var ipAddress = ipHostInfo.AddressList[0];  
        var localEndPoint = new IPEndPoint(ipAddress, 11000);

        var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket other = null;

        socket.Bind(localEndPoint);

        try
        {
            socket.Listen();

            other = socket.Accept();

            var seed = Environment.TickCount;

            Match.Initialize(new GameInfo(new ComInfo(0, 0), 50, seed, 3, 60 * 60,
                BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None));
            
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BitConverter.TryWriteBytes(buffer, seed);

            socket.Send(buffer);
            KickoffLockstep(0);
            
            _mySocket = socket;
            _otherSocket = other;
        }
        catch (Exception e)
        {
            GameFlow.Set(new MainMenuState());
            Error.RaiseMessage(e.Message);
        }
        finally
        {
            socket.Close();
            other?.Close();
        }
    }

    public void Initialize()
    {
        Task.Run(ListenThread);
    }

    public void Exit()
    {
        _mySocket.Close();
        _otherSocket.Close();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }

    public void Update()
    {
        if(_mySocket == null || _otherSocket == null)
            return;

        if (_lockstepTimer == 0)
        {
            Span<byte> buffer = stackalloc byte[1];
            
            _stopwatch.Reset();
            _otherSocket.Receive(buffer);
            _stopwatch.Stop();
            
            _inputs[0] = _mine;
            _inputs[1] = (GameInput) buffer[0];

            KickoffLockstep(_stopwatch.ElapsedMilliseconds / 16);
        }
        else
        {
            --_lockstepTimer;
        }

        Match.Update(_inputs);
    }

    private void KickoffLockstep(long roundTripTime)
    {
        _mine = Settings.MyData.GetInput(0);

        Span<byte> buffer = stackalloc byte[1];
        buffer[0] = (byte) _mine;

        _mySocket.Send(buffer);

        _lockstepTimer = roundTripTime;
    }

    public void Draw()
    {
        Match.Draw();
    }
}