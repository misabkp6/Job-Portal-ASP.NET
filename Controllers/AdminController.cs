using X.PagedList;
using X.PagedList.Mvc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data;
using JobPortal.Models;
using System.Linq;
using System;

namespace JobPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Redirect /Admin to Dashboard
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        // ✅ Admin Dashboard Overview with charts
        public IActionResult Dashboard()
        {
            var totalJobs = _context.Jobs.Count();
            var totalApplications = _context.Applications.Count();
            var totalUsers = _context.Users.Count();

            ViewBag.TotalJobs = totalJobs;
            ViewBag.TotalApplications = totalApplications;
            ViewBag.TotalUsers = totalUsers;

            // ✅ Jobs per month (last 5 months)
            var jobStats = _context.Jobs
                .Where(j => j.PostedDate >= DateTime.Now.AddMonths(-5))
                .AsEnumerable() // move to client-side for ToString
                .GroupBy(j => j.PostedDate.ToString("MMM yyyy"))
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.JobMonths = jobStats.Select(x => x.Month).ToList();
            ViewBag.JobCounts = jobStats.Select(x => x.Count).ToList();

            // ✅ User roles distribution
            var roles = _context.Roles.ToList();
            var roleCounts = new List<int>();
            var roleLabels = new List<string>();

            foreach (var role in roles)
            {
                var usersInRole = _context.UserRoles.Count(r => r.RoleId == role.Id);
                roleLabels.Add(role.Name ?? "Unnamed Role");
                roleCounts.Add(usersInRole);
            }

            ViewBag.RoleLabels = roleLabels;
            ViewBag.RoleCounts = roleCounts;

            return View();
        }

        // ✅ View All Job Posts with pagination
        public IActionResult Jobs(string? searchTerm = null, int page = 1)
        {
            int pageSize = 5;
            ViewBag.SearchTerm = searchTerm;
            
            var jobsQuery = _context.Jobs.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Search in job title, company name, location, or description
                jobsQuery = jobsQuery.Where(j => 
                    j.Title.Contains(searchTerm) || 
                    j.Company.Contains(searchTerm) || 
                    j.Location.Contains(searchTerm) || 
                    j.Description.Contains(searchTerm));
                    
                ViewBag.SearchCount = jobsQuery.Count();
            }
            
            var jobsList = jobsQuery.OrderByDescending(j => j.PostedDate).ToList();
            var jobs = new X.PagedList.StaticPagedList<Job>(
                jobsList.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                jobsList.Count
            );

            return View(jobs);
        }

        // ✅ View All Applications with pagination
        public IActionResult Applications(int page = 1)
        {
            int pageSize = 5;
            
            // Using a projection to avoid the UserId field issue
            var appsQuery = _context.Applications
                .Include(a => a.Job)
                .OrderByDescending(a => a.AppliedOn)
                .Select(a => new Application
                {
                    Id = a.Id,
                    ApplicantName = a.ApplicantName,
                    ApplicantEmail = a.ApplicantEmail,
                    ResumePath = a.ResumePath,
                    AppliedOn = a.AppliedOn,
                    JobId = a.JobId,
                    Job = a.Job
                    // Deliberately exclude UserId since it's not in the database
                })
                .AsQueryable();
                
            var appsList = appsQuery.ToList();
            var apps = new X.PagedList.StaticPagedList<Application>(
                appsList.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                appsList.Count
            );
            
            return View(apps);
        }

        // ✅ Delete a Job Post
        public IActionResult DeleteJob(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                _context.SaveChanges();
            }
            return RedirectToAction("Jobs");
        }
        
        // ✅ Delete an Application
        public IActionResult DeleteApplication(int id)
        {
            // Instead of using Find() which tries to use the complete model including UserId,
            // directly query the database with SQL to avoid the missing column issue
            var application = _context.Applications
                .Where(a => a.Id == id)
                .Select(a => new Application
                {
                    Id = a.Id,
                    ApplicantName = a.ApplicantName,
                    ApplicantEmail = a.ApplicantEmail,
                    ResumePath = a.ResumePath,
                    AppliedOn = a.AppliedOn,
                    JobId = a.JobId
                    // Deliberately exclude UserId since it's not in the database
                })
                .FirstOrDefault();
                
            if (application != null)
            {
                // Optionally delete the resume file if needed
                if (!string.IsNullOrEmpty(application.ResumePath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", application.ResumePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception)
                        {
                            // Log the error but continue
                        }
                    }
                }

                // Use ExecuteSqlRaw for direct SQL command to avoid Entity Framework issues with missing columns
                _context.Database.ExecuteSqlRaw("DELETE FROM Applications WHERE Id = {0}", id);
                
                TempData["SuccessMessage"] = "Application deleted successfully.";
            }
            return RedirectToAction("Applications");
        }
    }
}
