using JobPortal.Models.Enums;

namespace JobPortal.Models
{
    public class JobSearchViewModel
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public JobType? JobType { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string? Company { get; set; }
        public string? Tags { get; set; }
        public bool? Remote { get; set; }
        
        // For pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // For sorting
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = true;
    }
}
