using System.Net;

namespace ScoringServer;

public interface IPath
{
    Task OnGet(HttpListenerRequest request, HttpListenerResponse response);
    Task OnPost(HttpListenerRequest request, HttpListenerResponse response);
}