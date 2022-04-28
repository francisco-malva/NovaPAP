#region

using System.Net;

#endregion

namespace ScoringServer.Paths;

public interface IPath
{
    Task OnGet(HttpListenerRequest request, HttpListenerResponse response);
    Task OnPost(HttpListenerRequest request, HttpListenerResponse response);
}