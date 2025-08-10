using Microsoft.AspNetCore.Mvc;
using JobPortal.Models;
using JobPortal.Data;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace JobPortal.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApplicationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Apply(int jobId)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null) return NotFound();

            ViewBag.JobTitle = job.Title;
            ViewBag.JobId = job.Id;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(Application application, IFormFile ResumeFile)
        {
            // Get current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            // Basic validation
            if (!ModelState.IsValid)
            {
                var job = _context.Jobs.FirstOrDefault(j => j.Id == application.JobId);
                if (job == null) return NotFound();
                
                ViewBag.JobTitle = job.Title;
                ViewBag.JobId = job.Id;
                return View(application);
            }
            
            // Resume validation
            if (ResumeFile == null || ResumeFile.Length == 0)
            {
                ModelState.AddModelError("ResumeFile", "Resume file is required");
                var job = _context.Jobs.FirstOrDefault(j => j.Id == application.JobId);
                if (job == null) return NotFound();
                
                ViewBag.JobTitle = job.Title;
                ViewBag.JobId = job.Id;
                return View(application);
            }
            
            // Size validation (max 5MB)
            if (ResumeFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ResumeFile", "File size exceeds the maximum limit of 5MB");
                var job = _context.Jobs.FirstOrDefault(j => j.Id == application.JobId);
                if (job == null) return NotFound();
                
                ViewBag.JobTitle = job.Title;
                ViewBag.JobId = job.Id;
                return View(application);
            }
            
            // File type validation
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
            var fileExtension = Path.GetExtension(ResumeFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ResumeFile", "Invalid file type. Please upload a PDF, DOCX, or DOC file");
                var job = _context.Jobs.FirstOrDefault(j => j.Id == application.JobId);
                if (job == null) return NotFound();
                
                ViewBag.JobTitle = job.Title;
                ViewBag.JobId = job.Id;
                return View(application);
            }
            
            // Set application properties
            application.UserId = userId;
            application.AppliedOn = DateTime.Now;
            application.Status = JobPortal.Models.Enums.ApplicationStatus.Submitted;
            application.LastUpdated = DateTime.Now;
            
            // Save resume file
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ResumeFile.CopyToAsync(fileStream);
            }

            // Save relative path in the database
            application.ResumePath = "/resumes/" + uniqueFileName;
            
            try
            {
                _context.Applications.Add(application);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Your application has been submitted successfully! You can track its status in your profile.";
                return RedirectToAction("Index", "Job");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while submitting your application. Please try again later.");
                // Log the exception
                Console.WriteLine(ex.Message);
                
                var job = _context.Jobs.FirstOrDefault(j => j.Id == application.JobId);
                if (job == null) return NotFound();
                
                ViewBag.JobTitle = job.Title;
                ViewBag.JobId = job.Id;
                return View(application);
            }
        }
        
        [Authorize]
        public IActionResult MyApplications(int page = 1)
        {
            // Get current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            int pageSize = 10;
            
            var applicationsList = _context.Applications
                .Include(a => a.Job)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppliedOn)
                .ToList();
                
            var applications = new X.PagedList.StaticPagedList<Application>(
                applicationsList.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                applicationsList.Count
            );
            
            // Summary statistics for the dashboard
            ViewBag.TotalApplications = applicationsList.Count;
            ViewBag.PendingApplications = applicationsList.Count(a => a.Status == Models.Enums.ApplicationStatus.Submitted || 
                                                                     a.Status == Models.Enums.ApplicationStatus.UnderReview);
            ViewBag.SuccessfulApplications = applicationsList.Count(a => a.Status == Models.Enums.ApplicationStatus.Accepted);
            ViewBag.RejectedApplications = applicationsList.Count(a => a.Status == Models.Enums.ApplicationStatus.Rejected);
                
            return View(applications);
        }
        
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            // Get current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            // Get application with job details
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
                
            if (application == null)
            {
                return NotFound();
            }
            
            // Get application timeline/history
            var statusTimeline = new List<KeyValuePair<string, DateTime>>();
            
            // Always add the initial submission
            statusTimeline.Add(new KeyValuePair<string, DateTime>("Application Submitted", application.AppliedOn));
            
            // Add when the application was first viewed by employer
            if (application.IsViewed && application.ViewedOn.HasValue)
            {
                statusTimeline.Add(new KeyValuePair<string, DateTime>("Viewed by Employer", application.ViewedOn.Value));
            }
            
            // If status has been updated, add that to timeline
            if (application.Status != Models.Enums.ApplicationStatus.Submitted && application.LastUpdated.HasValue)
            {
                statusTimeline.Add(new KeyValuePair<string, DateTime>($"Status updated to {application.Status}", application.LastUpdated.Value));
            }
            
            ViewBag.StatusTimeline = statusTimeline;
            
            return View(application);
        }
    }
}
