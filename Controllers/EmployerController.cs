using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data;
using JobPortal.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using System.Collections.Generic;

namespace JobPortal.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployerController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Redirect /Employer to Dashboard
        public IActionResult Index()
        {
            // Always redirect to Dashboard first
            return RedirectToAction("Dashboard");
        }

        // Employer Dashboard Overview
        public async Task<IActionResult> Dashboard()
        {
            System.Diagnostics.Debug.WriteLine("ENTERING EMPLOYER DASHBOARD ACTION");
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Current user is null!");
                return Challenge();
            }

            var userId = currentUser.Id;
            var userEmail = currentUser.Email ?? User.Identity?.Name;
            
            // Debug info about user identifiers
            System.Diagnostics.Debug.WriteLine($"Dashboard - User ID: {userId}");
            System.Diagnostics.Debug.WriteLine($"Dashboard - User Email: {userEmail}");
            
            // No company profile functionality needed
            // Get employer's jobs statistics - using email as identifier
            var totalJobs = await _context.Jobs.CountAsync(j => j.EmployerId == userEmail);
            var activeJobs = await _context.Jobs.CountAsync(j => j.EmployerId == userEmail && j.Status == Models.Enums.JobStatus.Active);
            var expiredJobs = await _context.Jobs.CountAsync(j => j.EmployerId == userEmail && j.Status == Models.Enums.JobStatus.Expired);
            
            // Initialize default values
            int totalApplications = 0;
            int newApplications = 0;
            int reviewingApplications = 0;
            List<Application> recentApplications = new List<Application>();
            
            // Only get application data if employer has jobs
            if (totalJobs > 0)
            {
                // Get applications for employer's jobs - using email as identifier
                var jobIds = await _context.Jobs
                    .Where(j => j.EmployerId == userEmail)
                    .Select(j => j.Id)
                    .ToListAsync();
                
                if (jobIds.Any())
                {
                    totalApplications = await _context.Applications.CountAsync(a => jobIds.Contains(a.JobId));
                    newApplications = await _context.Applications.CountAsync(a => 
                        jobIds.Contains(a.JobId) && 
                        a.Status == Models.Enums.ApplicationStatus.Submitted);
                    
                    reviewingApplications = await _context.Applications.CountAsync(a => 
                        jobIds.Contains(a.JobId) && 
                        a.Status == Models.Enums.ApplicationStatus.UnderReview);
                    
                    // Get recent applications - use more direct approach
                    try
                    {
                        // Log some debug information
                        System.Diagnostics.Debug.WriteLine($"JobIds count: {jobIds.Count}");
                        System.Diagnostics.Debug.WriteLine($"First few JobIds: {string.Join(", ", jobIds.Take(5))}");
                        
                        // Get all applications for debugging
                        var allApps = await _context.Applications.ToListAsync();
                        System.Diagnostics.Debug.WriteLine($"Total applications in DB: {allApps.Count}");
                        
                        // Use direct approach without projection
                        recentApplications = await _context.Applications
                            .Include(a => a.Job)
                            .Where(a => jobIds.Contains(a.JobId))
                            .OrderByDescending(a => a.AppliedOn)
                            .Take(5)
                            .ToListAsync();
                            
                        System.Diagnostics.Debug.WriteLine($"Found {recentApplications.Count} recent applications");
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                        System.Diagnostics.Debug.WriteLine($"Exception getting applications: {ex.Message}");
                        // Continue with empty list
                        recentApplications = new List<Application>();
                    }
                }
            }

            // Pass data to view
            ViewBag.TotalJobs = totalJobs;
            ViewBag.ActiveJobs = activeJobs;
            ViewBag.ExpiredJobs = expiredJobs;
            ViewBag.TotalApplications = totalApplications;
            ViewBag.NewApplications = newApplications;
            ViewBag.ReviewingApplications = reviewingApplications;
            ViewBag.RecentApplications = recentApplications;
            
            // Get employer's name from the AspNetUsers table
            var employerName = await _userManager.GetUserNameAsync(currentUser);
            ViewBag.EmployerName = employerName;

            return View();
        }

        // Manage Jobs
        public async Task<IActionResult> Jobs(string? searchTerm = null, int page = 1)
        {
            int pageSize = 10;
            ViewBag.SearchTerm = searchTerm;
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var userId = currentUser.Id;
            var userEmail = currentUser.Email ?? User.Identity?.Name;
            
            // Debug info
            System.Diagnostics.Debug.WriteLine($"Current User ID: {userId}");
            System.Diagnostics.Debug.WriteLine($"Current User Email: {userEmail}");
            
            // Get all jobs for debugging
            var allJobs = await _context.Jobs.ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total jobs in system: {allJobs.Count}");
            
            // Debug the first 5 jobs
            foreach (var job in allJobs.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"Job {job.Id}: Title={job.Title}, EmployerId={job.EmployerId}");
            }
            
            // CRITICAL FIX: Use Email as EmployerId to match how jobs are stored
            var jobsQuery = _context.Jobs
                .Where(j => j.EmployerId == userEmail)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                jobsQuery = jobsQuery.Where(j => 
                    j.Title.Contains(searchTerm) || 
                    j.Location.Contains(searchTerm) || 
                    j.Description.Contains(searchTerm));
                    
                ViewBag.SearchCount = await jobsQuery.CountAsync();
            }
            
            var jobsList = await jobsQuery
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();
                
            var jobs = new StaticPagedList<Job>(
                jobsList.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                jobsList.Count);

            return View(jobs);
        }

        // Manage Applications for this employer's jobs
        public async Task<IActionResult> Applications(string? searchTerm = null, int page = 1)
        {
            int pageSize = 10;
            ViewBag.SearchTerm = searchTerm;
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var userId = currentUser.Id;
            
            // Add diagnostic info
            ViewBag.EmployerId = userId;
            
            // Debug: Check for userId consistency in DB
            var userEmail = currentUser.Email ?? User.Identity?.Name;
            System.Diagnostics.Debug.WriteLine($"Current employer userId: {userId}");
            System.Diagnostics.Debug.WriteLine($"Current employer userEmail: {userEmail}");
            
            ViewBag.UserEmail = userEmail; // Add to ViewBag for debugging
            
            // Get this employer's job IDs with debug output
            var allJobs = await _context.Jobs.ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total jobs in system: {allJobs.Count}");
            
            // Using email as identifier
            var employerJobs = allJobs.Where(j => j.EmployerId == userEmail).ToList();
            System.Diagnostics.Debug.WriteLine($"Jobs found for employer {userEmail}: {employerJobs.Count}");
            
            var jobIds = employerJobs.Select(j => j.Id).ToList();
            
            // More detailed diagnostics about jobs
            foreach (var job in employerJobs.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"Job {job.Id}: '{job.Title}' with EmployerId: '{job.EmployerId}'");
            }
                
            // Add diagnostic info
            ViewBag.JobCount = jobIds.Count;
            ViewBag.JobIds = string.Join(", ", jobIds);
            
            // Get total applications count for debugging
            var totalAppsInSystem = await _context.Applications.CountAsync();
            ViewBag.TotalAppsInSystem = totalAppsInSystem;
            
            // More direct query for debugging with detailed output
            var allApplications = await _context.Applications.Include(a => a.Job).ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total applications in system: {allApplications.Count}");
            
            var employerApplications = allApplications
                .Where(a => jobIds.Contains(a.JobId))
                .ToList();
            
            System.Diagnostics.Debug.WriteLine($"Applications found for employer jobs: {employerApplications.Count}");
            
            // Detailed debugging for application/job matches
            foreach (var app in allApplications.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"App {app.Id} for JobId {app.JobId} - Job exists: {app.Job != null}");
                if (app.Job != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Job EmployerId: {app.Job.EmployerId}");
                }
            }
            
            var directAppCount = employerApplications.Count;
            
            // Improved approach - ensure we're getting complete data
            var applicationsQuery = _context.Applications
                .Include(a => a.Job)
                .AsQueryable();
                
            // First check if we have any jobs before filtering
            if (jobIds.Any())
            {
                applicationsQuery = applicationsQuery.Where(a => jobIds.Contains(a.JobId));
            }
            else
            {
                // No jobs found for this employer - log this issue
                System.Diagnostics.Debug.WriteLine("WARNING: No jobs found for this employer!");
            }
                
            if (!string.IsNullOrEmpty(searchTerm))
            {
                applicationsQuery = applicationsQuery.Where(a => 
                    a.ApplicantName.Contains(searchTerm) || 
                    a.ApplicantEmail.Contains(searchTerm) || 
                    (a.Job != null && a.Job.Title.Contains(searchTerm)));
                    
                ViewBag.SearchCount = await applicationsQuery.CountAsync();
            }
            
            // Use ToList() to execute the query and then do in-memory operations
            var applicationsList = await applicationsQuery
                .OrderByDescending(a => a.AppliedOn)
                .ToListAsync();
                
            // Additional verification of job-application relationship
            foreach (var app in applicationsList.Take(3))
            {
                System.Diagnostics.Debug.WriteLine($"Verified app {app.Id} for job {app.JobId} will be included in results");
            }
                
            var applications = new StaticPagedList<Application>(
                applicationsList.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                applicationsList.Count);

            return View(applications);
        }

        // View Application Details
        public async Task<IActionResult> ApplicationDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var userId = currentUser.Id;
            var userEmail = currentUser.Email ?? User.Identity?.Name;
            
            // Debug info
            System.Diagnostics.Debug.WriteLine($"ApplicationDetails - User ID: {userId}");
            System.Diagnostics.Debug.WriteLine($"ApplicationDetails - User Email: {userEmail}");
            
            // Get this employer's job IDs using email as identifier
            var jobIds = await _context.Jobs
                .Where(j => j.EmployerId == userEmail)
                .Select(j => j.Id)
                .ToListAsync();
                
            // Find the application directly instead of using projection so we can update it
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id && jobIds.Contains(a.JobId));
                
            if (application != null && !application.IsViewed)
            {
                // Mark as viewed if this is the first time
                application.IsViewed = true;
                application.ViewedOn = DateTime.Now;
                await _context.SaveChangesAsync();
                
                // Log that the application was marked as viewed
                System.Diagnostics.Debug.WriteLine($"Application {id} marked as viewed by employer {userEmail}");
            }
                
            if (application == null)
            {
                return NotFound();
            }
            
            return View(application);
        }

        // Update Application Status
        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStatus(int id, Models.Enums.ApplicationStatus status, string feedback)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var userId = currentUser.Id;
            var userEmail = currentUser.Email ?? User.Identity?.Name;
            
            // Debug info
            System.Diagnostics.Debug.WriteLine($"UpdateApplicationStatus - User ID: {userId}");
            System.Diagnostics.Debug.WriteLine($"UpdateApplicationStatus - User Email: {userEmail}");
            
            // Get this employer's job IDs using email as identifier
            var jobIds = await _context.Jobs
                .Where(j => j.EmployerId == userEmail)
                .Select(j => j.Id)
                .ToListAsync();
                
            // Find application without projection to update it directly
            var application = await _context.Applications
                .Where(a => a.Id == id && jobIds.Contains(a.JobId))
                .FirstOrDefaultAsync();
                
            if (application == null)
            {
                return NotFound();
            }
            
            application.Status = status;
            application.AdminFeedback = feedback;
            application.LastUpdated = DateTime.Now;
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(ApplicationDetails), new { id = id });
        }

        // Company Profile Management completely removed
    }
}
