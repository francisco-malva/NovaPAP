using System.Data.SQLite;
using System.Net;
using System.Text.Json;
using Common;
using ScoringServer.Extensions;

namespace ScoringServer.Paths.Scores;

[Path("/time-attack")]
public class TimeAttackPath : IPath
{
    public async Task OnGet(HttpListenerRequest request, HttpListenerResponse response)
    {
        await using var connection = new SQLiteConnection(Database.ConnectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM TimeAttackScores ORDER BY TimeTaken LIMIT 10";

        await using var reader = command.ExecuteReaderAsync().Result;

        var scores = new List<TimeAttackScore>();

        while (await reader.ReadAsync())
            scores.Add(new TimeAttackScore((string) reader["Name"], (long) reader["TimeTaken"]));

        await response.WriteJson(scores);

        response.Close();
    }

    public async Task OnPost(HttpListenerRequest request, HttpListenerResponse response)
    {
        var score = JsonSerializer.Deserialize<TimeAttackScore>(request.InputStream);

        if (score == null)
        {
            response.StatusCode = 404;
        }
        else
        {
            await using var connection = new SQLiteConnection(Database.ConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();

            command.CommandText = "INSERT INTO TimeAttackScores(Name, TimeTaken) VALUES(@name, @timeTaken)";

            command.Parameters.AddWithValue("@name", score.Name);
            command.Parameters.AddWithValue("@timeTaken", score.TimeTaken);
            await command.PrepareAsync();

            await command.ExecuteNonQueryAsync();
        }

        response.Close();
    }
}