using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using JobPortal.Models;
using JobPortal.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using JobPortal.Models.Enums;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System.Security.Claims;

namespace JobPortal.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(JobSearchViewModel search)
        {
            // Pass the search model back to view to maintain filter state
            ViewBag.Search = search;
            
            // Create base query
            var jobsQuery = _context.Jobs.AsQueryable();
            
            // Filter by keyword (search in job title, company name, location, description, or tags)
            if (!string.IsNullOrEmpty(search.Keyword))
            {
                jobsQuery = jobsQuery.Where(j => 
                    j.Title.Contains(search.Keyword) || 
                    j.Company.Contains(search.Keyword) || 
                    j.Location.Contains(search.Keyword) || 
                    j.Description.Contains(search.Keyword) ||
                    (j.Tags != null && j.Tags.Contains(search.Keyword)));
            }
            
            // Filter by location
            if (!string.IsNullOrEmpty(search.Location))
            {
                jobsQuery = jobsQuery.Where(j => j.Location.Contains(search.Location));
            }
            
            // Filter by job type
            if (search.JobType.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.JobType == search.JobType.Value);
            }
            
            // Filter by salary range
            if (search.MinSalary.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Salary >= search.MinSalary.Value);
            }
            
            if (search.MaxSalary.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Salary <= search.MaxSalary.Value);
            }
            
            // Filter by company
            if (!string.IsNullOrEmpty(search.Company))
            {
                jobsQuery = jobsQuery.Where(j => j.Company.Contains(search.Company));
            }
            
            // Filter by tags
            if (!string.IsNullOrEmpty(search.Tags))
            {
                jobsQuery = jobsQuery.Where(j => j.Tags != null && j.Tags.Contains(search.Tags));
            }
            
            // Filter by remote status (if it's a job type)
            if (search.Remote.HasValue && search.Remote.Value)
            {
                jobsQuery = jobsQuery.Where(j => j.JobType == JobType.Remote);
            }
            
            // Apply sorting
            if (!string.IsNullOrEmpty(search.SortBy))
            {
                switch (search.SortBy.ToLower())
                {
                    case "date":
                        jobsQuery = search.SortAscending 
                            ? jobsQuery.OrderBy(j => j.PostedDate) 
                            : jobsQuery.OrderByDescending(j => j.PostedDate);
                        break;
                    case "title":
                        jobsQuery = search.SortAscending 
                            ? jobsQuery.OrderBy(j => j.Title) 
                            : jobsQuery.OrderByDescending(j => j.Title);
                        break;
                    case "company":
                        jobsQuery = search.SortAscending 
                            ? jobsQuery.OrderBy(j => j.Company) 
                            : jobsQuery.OrderByDescending(j => j.Company);
                        break;
                    case "salary":
                        jobsQuery = search.SortAscending 
                            ? jobsQuery.OrderBy(j => j.Salary) 
                            : jobsQuery.OrderByDescending(j => j.Salary);
                        break;
                    default:
                        jobsQuery = jobsQuery.OrderByDescending(j => j.PostedDate);
                        break;
                }
            }
            else
            {
                // Default sorting by most recent
                jobsQuery = jobsQuery.OrderByDescending(j => j.PostedDate);
            }
            
            // Set default values for pagination
            int page = search.Page < 1 ? 1 : search.Page;
            int pageSize = search.PageSize < 1 ? 10 : search.PageSize;
            
            // Get jobs with pagination
            var jobsList = jobsQuery.ToList();
            var jobs = new X.PagedList.StaticPagedList<Job>(
                jobsList.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                jobsList.Count
            );
            
            // Populate dropdowns for filters
            ViewBag.JobTypes = Enum.GetValues(typeof(JobType))
                .Cast<JobType>()
                .Select(t => new SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString(),
                    Selected = search.JobType.HasValue && t == search.JobType.Value
                });
            
            // Get unique companies for dropdown
            ViewBag.Companies = _context.Jobs
                .Select(j => j.Company)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            
            // Get unique locations for dropdown
            ViewBag.Locations = _context.Jobs
                .Select(j => j.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToList();
                
            // Load all job tags for filtering
            var tagsJobs = _context.Jobs
                .Where(j => j.Tags != null && j.Tags != string.Empty)
                .Select(j => j.Tags)
                .ToList();
                
            var allTags = new List<string>();
            foreach (var tagString in tagsJobs)
            {
                if (!string.IsNullOrEmpty(tagString))
                {
                    var tags = tagString.Split(',');
                    foreach (var tag in tags)
                    {
                        var trimmedTag = tag.Trim();
                        if (!string.IsNullOrEmpty(trimmedTag) && !allTags.Contains(trimmedTag))
                        {
                            allTags.Add(trimmedTag);
                        }
                    }
                }
            }
            
            ViewBag.Tags = allTags.OrderBy(t => t).ToList();
            
            return View(jobs);
        }

        [Authorize(Roles = "Admin,Employer")]
        public IActionResult Post()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> Post(Job job)
        {
            // Set PostedDate to current time
            job.PostedDate = DateTime.Now;
            
            // Set EmployerId consistently based on role
            if (User.IsInRole("Employer"))
            {
                // For employers, use their email as the EmployerId
                var userName = User.Identity?.Name;
                var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Debug info
                System.Diagnostics.Debug.WriteLine($"Post job - User.Identity.Name: {userName}");
                System.Diagnostics.Debug.WriteLine($"Post job - NameIdentifier: {nameIdentifier}");
                
                // IMPORTANT: We'll use User.Identity.Name (email) as the EmployerId to match existing data
                if (!string.IsNullOrEmpty(userName))
                {
                    job.EmployerId = userName;
                    System.Diagnostics.Debug.WriteLine($"Setting job.EmployerId to: {job.EmployerId}");
                }
            }
            else if (User.IsInRole("Admin"))
            {
                // For admins, if they don't set an EmployerId, mark it as admin-created
                if (string.IsNullOrEmpty(job.EmployerId))
                {
                    job.EmployerId = "admin@example.com"; // Default admin account
                    System.Diagnostics.Debug.WriteLine($"Admin creating job - Setting EmployerId to: {job.EmployerId}");
                }
            }
            
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"Job saved with ID: {job.Id} and EmployerId: {job.EmployerId}");
            
            // Redirect based on role
            if (User.IsInRole("Employer"))
            {
                TempData["SuccessMessage"] = "Your job has been posted successfully!";
                return RedirectToAction("Jobs", "Employer");
            }
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = "Employer")]
        public IActionResult MyJobs(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;
            
            // Get jobs posted by the current employer - use email for consistency
            string userEmail = User.Identity?.Name ?? string.Empty;
            
            // Enhanced debug info
            System.Diagnostics.Debug.WriteLine($"MyJobs - User.Identity.Name: {userEmail}");
            
            // Get all jobs for debugging
            var allJobs = _context.Jobs.ToList();
            System.Diagnostics.Debug.WriteLine($"Total jobs in system: {allJobs.Count}");
            
            // Debug the first few jobs
            foreach (var job in allJobs.Take(3))
            {
                System.Diagnostics.Debug.WriteLine($"Job {job.Id}: '{job.Title}', EmployerId: '{job.EmployerId}'");
            }
            
            var jobsQuery = _context.Jobs
                .Where(j => j.EmployerId == userEmail)
                .OrderByDescending(j => j.PostedDate);
            
            var pagedJobs = jobsQuery.ToList();
            System.Diagnostics.Debug.WriteLine($"Found {pagedJobs.Count} jobs for current employer: {userEmail}");
            
            var jobs = new X.PagedList.StaticPagedList<Job>(
                pagedJobs.Skip((pageNumber - 1) * pageSize).Take(pageSize), 
                pageNumber, 
                pageSize, 
                pagedJobs.Count);
            
            return View(jobs);
        }
        
        [Authorize]
        public IActionResult Details(int id)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();

            return View(job);
        }
    }
}
