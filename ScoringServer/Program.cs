#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Parsers;
using Common.Web;

#endregion

if (!File.Exists("db.sqlite")) SQLiteConnection.CreateFile("db.sqlite");

await using var connection = new SQLiteConnection("Data Source=db.sqlite;");

await connection.OpenAsync();

{
    await using var command = connection.CreateCommand();

    command.CommandText =
        "CREATE TABLE IF NOT EXISTS Times (Id int PRIMARY KEY, Name varchar(16) NOT NULL , Time int NOT NULL)";
    await command.ExecuteNonQueryAsync();
}

{
    await using var command = connection.CreateCommand();

    command.CommandText =
        "CREATE TABLE IF NOT EXISTS Heights (Id int PRIMARY KEY, Name varchar(16) NOT NULL , Height real NOT NULL)";
    await command.ExecuteNonQueryAsync();
}


var endpoint = new IPEndPoint(IpUtilities.GetIpFromName(Dns.GetHostName()), 12168);
Console.WriteLine($"Listening on endpoint: {endpoint}");
var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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
            UpdateScores(list[1] as string ?? throw new InvalidOperationException(), (int)list[2]);
            break;
        case "UpdateHeight":
            UpdateHeights(list[1] as string ?? throw new InvalidOperationException(), (double)list[2]);
            break;
        case "GetScores":
            GetScores(sock);
            break;
        case "GetHeights":
            GetHeights(sock);
            break;
    }
}

async void GetHeights(Socket sock)
{
    var str = "(";

    await using var cmd = connection.CreateCommand();

    cmd.CommandText = "SELECT Name, Height FROM Heights ORDER BY Height DESC LIMIT 10";

    await using var reader = cmd.ExecuteReader();

    while (await reader.ReadAsync())
        str += $"(\"{reader.GetString("Name")}\" {Math.Truncate(reader.GetDouble("Height"))})";

    str += ")";

    sock.Send(Encoding.UTF8.GetBytes(str));
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
    Console.WriteLine("Updated Scores");
}

async void UpdateHeights(string name, double height)
{
    await using var cmd = connection.CreateCommand();

    cmd.CommandText = "INSERT INTO Heights VALUES (NULL, @name, @height)";

    cmd.Parameters.Add("@name", DbType.String).Value = name;
    cmd.Parameters.Add("@height", DbType.Double).Value = height;

    cmd.Prepare();

    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Updated Heights");
}

string GetStringFromSocket(Socket sock)
{
    Span<byte> buffer = stackalloc byte[4096];
    var byteCount = sock.Receive(buffer);
    return Encoding.UTF8.GetString(buffer[..byteCount]);
}