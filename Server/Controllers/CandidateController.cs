using System.Net;
using MainLogger;
using Microsoft.AspNetCore.Mvc;
using Server.Models;

namespace Server.Controllers;

public class CandidateController
{
    private readonly Log4net<CandidateController> _logger = new();

    [HttpGet]
    [Route("/profile/{id}")]
    public async Task<Candidate>? GetProfile()
    {
        return default!;
    }

    // [HttpPost]
    // [Route("/profile]
}
