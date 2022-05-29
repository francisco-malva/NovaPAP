using System.Data.SqlClient;
using Common.Web;
using LiteNetLib;
using LiteNetLib.Utils;

var listener = new EventBasedNetListener();
var server = new NetManager(listener);
server.Start(9000);
var writer = new NetDataWriter();      

server.AutoRecycle = true;

listener.ConnectionRequestEvent += request =>
{
    request.AcceptIfKey("DuckDuckJumpScoring");
};


listener.PeerConnectedEvent += peer =>
{
    Console.WriteLine("We got connection: {0}", peer.EndPoint);

    writer.Reset();
    writer.Put((int)ServerEvent.ServerAck);                               
    peer.Send(writer, DeliveryMethod.ReliableOrdered);       
};


listener.NetworkReceiveEvent += (peer, reader, channel, method) =>
{
    var ev = (ServerEvent)reader.GetInt();
    
    switch(ev)
    {
        case ServerEvent.ServerAck:
            break;
        case ServerEvent.ScoreRequest:
            HandleScoreRequest(reader);
            break;
        case ServerEvent.ScoreData:
           break;
    }
};

while (!Console.KeyAvailable)
{
    server.PollEvents();
    Thread.Sleep(15);
}
server.Stop();

void HandleScoreRequest(NetDataReader reader)
{
    var min = reader.GetInt();
    var max = reader.GetInt();

    using var connection = new SqlConnection("Data Source=db.sqlite;Version=3;");
    connection.Open();

    using var command = connection.CreateCommand();


    command.CommandText = "SELECT Name, Frames FROM Scores ORDER BY Time ASC WHERE LIMIT 10;";

    var sqlReader = command.ExecuteReader();

    writer.Put((int)ServerEvent.ScoreData);
    while (sqlReader.Read())
    {
        writer.Put((string)sqlReader["Name"]);
        writer.Put((int)sqlReader["Frames"]);
    }
    
}