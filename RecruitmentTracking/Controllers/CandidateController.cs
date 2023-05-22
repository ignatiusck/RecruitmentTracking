using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using log4net;

using RecruitmentTracking.Models;
using IndexDb;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace RecruitmentTracking.Controllers;

public class CandidateController : Controller
{
    private readonly DataContex _db = new();
    private readonly ILog _log;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _server;

    public CandidateController(IWebHostEnvironment server, IConfiguration configuration)
    {
        _log = LogManager.GetLogger(typeof(CandidateController));
        _server = server;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;
        if (!ViewBag.IsAuth) return Redirect("/Login");

        string token = Request.Cookies["ActionLogin"]!;
        string email = GetEmail(token);

        Candidate candidate = (await _db.Candidates!.FirstOrDefaultAsync(c => c.Email == email))!;
        CandidateEditProfile profile = new()
        {
            Name = candidate.Name,
            Email = candidate.Email,
            Phone = candidate.Phone,
            LastEducation = candidate.LastEducation,
            GPA = candidate.GPA,
        };

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(CandidateEditProfile profile)
    {
        if (Request.Cookies["ActionLogin"]! == null) return Redirect("/Login");

        string token = Request.Cookies["ActionLogin"]!;

        string email = GetEmail(token);
        Candidate candidate = (await _db.Candidates!.FirstOrDefaultAsync(c => c.Email == email))!;
        candidate.Name = profile.Name;
        candidate.Phone = profile.Phone;
        candidate.LastEducation = profile.LastEducation;
        candidate.GPA = profile.GPA;

        await _db.SaveChangesAsync();

        TempData["success"] = "Successfully Update Profile";
        return Redirect("/Profile");
    }

    [HttpGet]
    public IActionResult ApplyJob(int id)
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;

        Job objJob = _db.Jobs!.Find(id)!;
        ViewBag.JobTitle = objJob.JobTitle;

        string token = Request.Cookies["ActionLogin"]!;
        string email = GetEmail(token);

        Candidate objCandidate = _db.Candidates!.FirstOrDefault(c => c.Email == email)!;

        CandidateEditProfile dataCandidate = new()
        {
            Name = objCandidate.Name,
            Phone = objCandidate.Phone,
            LastEducation = objCandidate.LastEducation,
            GPA = objCandidate.GPA,
        };

        ViewBag.JobId = id;
        return View(dataCandidate);
    }

    [HttpPost]
    public async Task<IActionResult> ApplyJobs(int JobId, CandidateEditProfile updateCandidate)
    {
        if (updateCandidate.FileCV?.Length < 0)
        {
            TempData["warning"] = "Please select a CV file";
            return Redirect($"/ApplyJob/{JobId}");
        }

        string email = GetEmail(Request.Cookies["ActionLogin"]!);

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateCandidate.FileCV!.FileName);

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data", "DataCV");

        Directory.CreateDirectory(uploadsFolder);

        string filePath = Path.Combine(uploadsFolder, fileName);

        Candidate objCandidate = (await _db.Candidates!.FirstOrDefaultAsync(c => c.Email == email))!;
        objCandidate.Name = updateCandidate.Name;
        objCandidate.Phone = updateCandidate.Phone;
        objCandidate.LastEducation = updateCandidate.LastEducation;
        objCandidate.GPA = updateCandidate.GPA;
        objCandidate.StatusInJob = $"{ProcessType.Administration}";

        Job objJob = (await _db.Jobs!.FindAsync(JobId))!;

        CandidateJob objCJ = new()
        {
            Candidate = objCandidate,
            Job = objJob,
            CV = fileName,
        };

        await _db.CandidateJobs!.AddAsync(objCJ);
        await _db.SaveChangesAsync();

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            updateCandidate.FileCV.CopyTo(stream);
        }

        TempData["success"] = "Application Successfully Sent";
        return Redirect("/TrackJob");
    }

    [HttpGet]
    public async Task<IActionResult> TrackJob()
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;

        string email = GetEmail(Request.Cookies["ActionLogin"]!);

        Candidate objCandidate = (await _db.Candidates!.FirstOrDefaultAsync(c => c.Email == email))!;
        List<Job> listJobCandidate = _db.CandidateJobs!
                            .Where(c => c.CandidateId == objCandidate.CandidateId)!
                            .Select(c => c.Job)
                            .ToList()!;

        List<CandidateJobStatus> listData = new();
        foreach (Job job in listJobCandidate)
        {
            CandidateJobStatus status = new()
            {
                CandidateStatus = GetStatusApplication(objCandidate.StatusInJob!).Split(' '), // need migrate db to CandidateJobStatus for status in Job
                JobTitle = job.JobTitle,
            };
            listData.Add(status);
        }
        return View(listData);
    }

    private string GetEmail(string token)
    {
        ClaimsPrincipal claimsPrincipal = new JwtSecurityTokenHandler()
            .ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                                    _configuration.GetSection("AppSettings:TokenCandidate").Value!
                                    )),
                ValidateIssuer = false,
                ValidateAudience = false,
            }, out _);

        return claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value!;
    }

    private string GetStatusApplication(string status)
    {
        List<string> process = new()
        {
            "current-item none none none",
            "none current-item none none",
            "none none current-item none",
            "none none none current-item",
        };

        int step = (int)Enum.Parse(typeof(ProcessType), status);

        return process[step - 1];
    }

    // [HttpGet("/Jobs")]
    // public async Task<IEnumerable<Job>> Job()
    // {
    //     return await _db.Jobs!.Where(Job => Job.IsJobAvailable).ToListAsync();
    // }

    // [HttpPatch("/EditProfile")]
    // public async Task<IActionResult> EditProfile(Candidate objCandidate)
    // {
    //     _db.Entry(objCandidate).State = EntityState.Modified;
    //     await _db.SaveChangesAsync();

    //     _log.Info("Job Updated.");

    //     return Ok(objCandidate);
    // }
}
