#region

using System.Data;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common.Parsers;

#endregion

if (!File.Exists("db.sqlite")) SQLiteConnection.CreateFile("db.sqlite");

await using var connection = new SQLiteConnection("Data Source=db.sqlite;");

await connection.OpenAsync();

await using var command = connection.CreateCommand();

command.CommandText =
    "CREATE TABLE IF NOT EXISTS Times (Id int PRIMARY KEY, Name varchar(16) NOT NULL , Time int NOT NULL)";
command.ExecuteNonQuery();

var addresses = Dns.GetHostAddresses(Dns.GetHostName());
var endpoint = new IPEndPoint(addresses[0], 12168);
var socket = new Socket(addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

socket.Bind(endpoint);
socket.Listen();

while (true)
{
    var otherSocket = await socket.AcceptAsync();
    await Task.Run(() => { HandleSocket(otherSocket); });
}


void HandleSocket(Socket sock)
{
    var str = GetStringFromSocket(sock);

    var list = SExpressionParser.Parse(str) as List<object>;

    switch (list?[0] as string)
    {
        case "UpdateScore":
            UpdateScores(list[1] as string ?? throw new InvalidOperationException(), (int) list[2]);
            break;
        case "GetScores":
            GetScores(sock);
            break;
    }
}

async void GetScores(Socket sock)
{
    var str = "(";

    await using var cmd = connection.CreateCommand();

    cmd.CommandText = "SELECT Name, Time FROM Times ORDER BY Time LIMIT 10";

    await using var reader = cmd.ExecuteReader();

    while (await reader.ReadAsync()) str += $"(\"{reader.GetString("Name")}\" {reader.GetInt32("Time")})";

    str += ")";

    sock.Send(Encoding.UTF8.GetBytes(str));
}

async void UpdateScores(string name, int time)
{
    await using var cmd = connection.CreateCommand();

    cmd.CommandText = "INSERT INTO Times VALUES (NULL, @name, @time)";

    cmd.Parameters.Add("@name", DbType.String).Value = name;
    cmd.Parameters.Add("@time", DbType.Int32).Value = time;

    cmd.Prepare();

    cmd.ExecuteNonQuery();
}

string GetStringFromSocket(Socket sock)
{
    Span<byte> buffer = stackalloc byte[4096];
    sock.Receive(buffer);
    return Encoding.UTF8.GetString(buffer);
}