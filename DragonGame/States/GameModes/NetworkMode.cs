#region

using System;
using System.Collections.Concurrent;
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

#endregion

namespace DuckDuckJump.States.GameModes;

public class NetworkMode : IGameState
{
    private const int Port = 9050;
    private readonly NetManager _endpoint;

    private readonly bool _isHost;
    private readonly EventBasedNetListener _listener;
    private readonly NetDataWriter _writer;

    private CircularBuffer<Pair<GameInput>> _buffer = new(16);

    private bool _initialized;

    private ConcurrentQueue<Pair<GameInput>> _inputs = new();
    private string _ip;

    private bool _running = true;

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

        _endpoint.StartInManualMode(Port);
        if (!_isHost)
        {
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
    }

    public void Draw()
    {
        if (_initialized) Match.Draw();
    }

    private void OnError(IPEndPoint endpoint, SocketError socketerror)
    {
        GameFlow.Set(new MainMenuState());
    }

    private void OnPeerConnected(NetPeer peer)
    {
        if (!_isHost)
            return;

        _writer.Reset();

        var info = new GameInfo(new ComLevels(0, 0), 100, Environment.TickCount, 4, 99 * 60,
            Match.BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None);

        _writer.Put(info);
        _writer.Put((byte)GameInput.None);
        peer.Send(_writer, DeliveryMethod.ReliableOrdered);

        Match.Initialize(info);
        _initialized = true;
    }

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

    private void EnqueuePendingInputs(NetPeer peer, NetDataReader reader)
    {
        var foreign = (GameInput)reader.GetByte();
        _writer.Reset();
        var mine = Settings.MyData.GetInput(0);

        _writer.Reset();
        _writer.Put((byte)mine);
        peer.Send(_writer, DeliveryMethod.ReliableOrdered);

        if (_initialized)
        {
            Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];
            inputs[0] = _isHost ? mine : foreign;
            inputs[1] = _isHost ? foreign : mine;
            Match.Update(inputs);
        }
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        if (_endpoint.ConnectedPeersCount == 1)
            request.Reject();
        else
            request.AcceptIfKey(nameof(NetworkMode));
    }
}