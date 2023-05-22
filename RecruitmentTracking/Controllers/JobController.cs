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

public class JobController : Controller
{
    private readonly DataContex _db = new();
    private readonly ILog _log;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _server;

    public JobController(IWebHostEnvironment server, IConfiguration configuration)
    {
        _log = LogManager.GetLogger(typeof(JobController));
        _server = server;
        _configuration = configuration;
    }


    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;

        if (ViewBag.IsAuth)
        {
            string token = Request.Cookies["ActionLogin"]!;
            JwtSecurityToken dataJwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            ViewBag.IsAdmin = dataJwt.Claims.Count() != 2 ? "admin" : null;
        }

        List<JobData> listJob = new();
        foreach (Job job in _db.Jobs!.Where(j => j.IsJobAvailable).ToList())
        {
            JobData data = new()
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,
                JobRequirement = job.JobRequirement,
                Location = job.Location,
                JobPostedDate = job.JobPostedDate,
                JobExpiredDate = job.JobExpiredDate,
            };

            listJob.Add(data);
        }
        return View(listJob);
    }

    [HttpGet]
    public IActionResult DetailJob(int id)
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;

        if (ViewBag.IsAuth)
        {
            string token = Request.Cookies["ActionLogin"]!;
            JwtSecurityToken dataJwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            ViewBag.IsAdmin = dataJwt.Claims.Count() != 2 ? "admin" : null;
        }

        Job objJob = _db.Jobs!.Find(id)!;

        JobData data = new()
        {
            JobId = objJob.JobId,
            JobTitle = objJob.JobTitle,
            JobDescription = objJob.JobDescription,
            JobRequirement = objJob.JobRequirement,
            Location = objJob.Location,
            JobPostedDate = objJob.JobPostedDate,
            JobExpiredDate = objJob.JobExpiredDate,
        };

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> CloseTheJob(int id)
    {
        Job objJob = _db.Jobs!.Find(id)!;

        objJob.IsJobAvailable = false;
        await _db.SaveChangesAsync();

        TempData["success"] = "Successfully Close a Job";
        return Redirect("/Admin");
    }

    [HttpPost]
    public async Task<IActionResult> ActivateTheJob(int id)
    {
        Job objJob = _db.Jobs!.Find(id)!;

        objJob.IsJobAvailable = true;
        await _db.SaveChangesAsync();

        TempData["success"] = "Successfully Activate a Job";
        return Redirect("/JobClosed");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteJob(int id)
    {
        Job objJob = _db.Jobs!.Find(id)!;

        _db.Jobs.Remove(objJob);
        await _db.SaveChangesAsync();

        TempData["success"] = "Successfully Delete a Job";
        return Redirect("/JobClosed");
    }

    [HttpGet]
    public IActionResult CreateJob()
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;
        ViewBag.IsAdmin = "admin";

        string token = Request.Cookies["ActionLogin"]!;
        GetDataAdmin(token, out _, out string adminName);

        ViewBag.AdminName = adminName;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateJobs(JobCreate objJob)
    {
        string token = Request.Cookies["ActionLogin"]!;
        GetDataAdmin(token, out string email, out _);

        Admin admin = _db.Admins!.FirstOrDefault(a => a.Email == email)!;
        Job newJob = new()
        {
            JobTitle = objJob.JobTitle,
            JobDescription = objJob.JobDescription,
            JobExpiredDate = objJob.JobExpiredDate,
            JobRequirement = objJob.JobRequirement!.Replace("\r\n", "\n"),
            JobPostedDate = DateTime.Now,
            Location = objJob.Location,
            IsJobAvailable = true,
            Admin = admin,
        };
        _db.Jobs!.Add(newJob);
        await _db.SaveChangesAsync();

        _log.Info("Job Added.");

        TempData["success"] = "Successfully Created a Job";
        return Redirect("/Admin");
    }

    [HttpGet]
    public IActionResult EditJob(int id)
    {
        ViewBag.IsAuth = Request.Cookies["ActionLogin"]! != null;
        ViewBag.IsAdmin = "admin";

        string token = Request.Cookies["ActionLogin"]!;
        GetDataAdmin(token, out _, out string adminName);

        ViewBag.AdminName = adminName;

        Job objJob = _db.Jobs!.FirstOrDefault(j => j.JobId == id)!;

        JobData data = new()
        {
            JobId = objJob.JobId,
            JobTitle = objJob.JobTitle,
            JobDescription = objJob.JobDescription,
            JobRequirement = objJob.JobRequirement!.Replace("\r\n", "\n"),
            Location = objJob.Location,
            JobPostedDate = objJob.JobPostedDate,
            JobExpiredDate = objJob.JobExpiredDate,
        };

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> EditJobs(JobCreate objJob)
    {
        string token = Request.Cookies["ActionLogin"]!;
        GetDataAdmin(token, out string email, out _);

        Job updateJob = _db.Jobs!.FirstOrDefault(j => j.JobId == objJob.JobId)!;
        updateJob.JobTitle = objJob.JobTitle;
        updateJob.JobDescription = objJob.JobDescription;
        updateJob.JobExpiredDate = objJob.JobExpiredDate;
        updateJob.JobRequirement = objJob.JobRequirement;
        updateJob.Location = objJob.Location;

        await _db.SaveChangesAsync();
        _log.Info("Job Updated.");

        TempData["success"] = "Successfully Edit a Job";
        return Redirect("/Admin");
    }

    private void GetDataAdmin(string token, out string email, out string name)
    {
        ClaimsPrincipal claimsPrincipal = new JwtSecurityTokenHandler()
            .ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                                    _configuration.GetSection("AppSettings:TokenAdmin").Value!
                                    )),
                ValidateIssuer = false,
                ValidateAudience = false,
            }, out _);

        email = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value!;
        name = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value!;
    }

}
