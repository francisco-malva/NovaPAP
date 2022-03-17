using System.Net;
using System.Reflection;
using ScoringServer.Paths;

namespace ScoringServer;

internal static class Program
{
    private const string Url = "http://localhost:8000/";

    private static async Task HandleIncomingConnections(HttpListener listener)
    {
        var paths = new Dictionary<string, IPath>();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.GetInterface(nameof(IPath)) == null) continue;

            var attribute = (PathAttribute) type.GetCustomAttribute(typeof(PathAttribute))!;
            paths.Add(attribute.PathName, (IPath) Activator.CreateInstance(type)!);
        }

        while (true)
        {
            // Will wait here until we hear from a connection
            var ctx = await listener.GetContextAsync();

            // Peel out the requests and response objects
            var req = ctx.Request;
            var resp = ctx.Response;

            // Print out some info about the request
            Console.WriteLine(req.Url?.ToString());
            Console.WriteLine(req.HttpMethod);
            Console.WriteLine(req.UserHostName);
            Console.WriteLine(req.UserAgent);
            Console.WriteLine();


            var absolutePath = req.Url?.AbsolutePath ?? string.Empty;

            if (!paths.ContainsKey(absolutePath))
            {
                resp.StatusCode = 404;
                resp.Close();
            }
            else
            {
                switch (req.HttpMethod)
                {
                    case "GET":
                        await paths[absolutePath].OnGet(req, resp);
                        break;
                    case "POST":
                        await paths[absolutePath].OnPost(req, resp);
                        break;
                }
            }
        }
    }

    public static async Task Main(string[] args)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(Url);
        listener.Start();
        Console.WriteLine("Listening for connections on {0}", Url);

        // Handle requests
        await HandleIncomingConnections(listener);

        // Close the listener
        listener.Close();
    }
}