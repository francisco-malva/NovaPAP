using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CircularBuffer;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.Input;
using LiteNetLib;
using LiteNetLib.Utils;
using SDL2;

namespace DuckDuckJump.States.GameModes;

public class NetworkMode : IGameState
{
    private const int Port = 9050;
    private NetDataWriter _writer;
    private NetManager _endpoint;
    private EventBasedNetListener _listener;

    private bool _isHost;
    private string _ip;
    public NetworkMode(bool isHost, string ip)
    {
        _listener = new EventBasedNetListener();
        _endpoint = new NetManager(_listener);
        _writer = new NetDataWriter();

        _isHost = isHost;
        _ip = ip;
    }

    public void Initialize()
    {
        _endpoint.AutoRecycle = true;

        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnPeerConnected;
        _listener.NetworkReceiveEvent += OnReceive;
        _listener.NetworkErrorEvent += OnError;

        if (_isHost)
        {
            _endpoint.Start(Port);
        }
        else
        {
            _endpoint.Start();
            _endpoint.Connect(_ip, Port, nameof(NetworkMode));
        }
    }

    private bool _running = true;

    private CircularBuffer<Pair<GameInput>> _buffer = new(16);
    
    private void OnError(IPEndPoint endpoint, SocketError socketerror)
    {
        GameFlow.Set(new MainMenuState());
    }

    private void OnPeerConnected(NetPeer peer)
    {
        if(!_isHost)
            return;
        
        _writer.Reset();

        var info = new GameInfo(new ComLevels(0, 0), 100, Environment.TickCount, 4, true, 99 * 60,
            Match.BannerWork.MessageIndex.NoBanner);
        
        _writer.Put(info);
        _writer.Put((byte) GameInput.None);
        peer.Send(_writer, DeliveryMethod.ReliableOrdered);
        
        Match.Initialize(info);
        _initialized = true;

    }

    private bool _initialized;

    private void OnReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod)
    {
        if (!_initialized)
        {
            Match.Initialize(reader.Get<GameInfo>());
            _initialized = true;
            EnqueuePendingInputs(peer, reader);
        }
        else
        {
            EnqueuePendingInputs(peer, reader);
        }
    }

    private ConcurrentQueue<Pair<GameInput>> _inputs = new ConcurrentQueue<Pair<GameInput>>();
    
    private void EnqueuePendingInputs(NetPeer peer, NetDataReader reader)
    {
        var foreign = (GameInput) reader.GetByte();
        _writer.Reset();
        var mine = Settings.MyData.GetInput(0);

        _writer.Reset();
        _writer.Put((byte)mine);
        peer.Send(_writer, DeliveryMethod.ReliableOrdered);

        _inputs.Enqueue(new Pair<GameInput>(mine, foreign));
    }
    
    private void OnConnectionRequest(ConnectionRequest request)
    {
        if(_endpoint.ConnectedPeersCount == 1)
            request.Reject();
        else
        {
            request.AcceptIfKey(nameof(NetworkMode));
        }
    }

    public void Exit()
    {
        _endpoint.Stop();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
    }
    
    public void Update()
    {
        _endpoint.PollEvents();
        if (_initialized)
        {
            if (_inputs.TryDequeue(out var pair))
            {
                Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];
                inputs[0] = _isHost ? pair.First : pair.Second;
                inputs[1] = _isHost ? pair.Second : pair.First;
                Match.Update(inputs);
            }
            
        }
    }

    public void Draw()
    {
        if (_initialized)
        {
            Match.Draw();
        }
    }
}