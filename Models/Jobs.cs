using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal.Models.Enums;

namespace JobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string Company { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string Location { get; set; } = string.Empty;
        
        [Required]
        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Salary { get; set; }
        
        [Required]
        public JobType JobType { get; set; } = JobType.FullTime;
        
        [Required]
        public JobStatus Status { get; set; } = JobStatus.Active;
        
        [Required]
        public DateTime PostedDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        [StringLength(200)]
        public string? CompanyLogoUrl { get; set; }
        
        [StringLength(2000)]
        public string? Requirements { get; set; }
        
        [StringLength(256)]
        public string? EmployerId { get; set; }
        
        [StringLength(2000)]
        public string? Benefits { get; set; }
        
        // Tags for improved search and categorization
        public string? Tags { get; set; }
    }
}
