using System.Net;

namespace ScoringServer.Paths;

public interface IPath
{
    Task OnGet(HttpListenerRequest request, HttpListenerResponse response);
    Task OnPost(HttpListenerRequest request, HttpListenerResponse response);
}