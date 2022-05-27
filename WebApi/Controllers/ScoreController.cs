using Common.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Sql
namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ScoreController : Controller
{
    private readonly ILogger<ScoreController> _logger;
    
    public ScoreController(ILogger<ScoreController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet(Name = "GetScores")]
    public IEnumerable<Score> Get(int start, int end)
    {
        using var conn =
    }
}