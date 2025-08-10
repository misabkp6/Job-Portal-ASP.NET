using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using JobPortal.Models.Enums;

namespace JobPortal.Models
{
    public class Application
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string ApplicantName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string ApplicantEmail { get; set; } = string.Empty;

        public string? ResumePath { get; set; }

        public DateTime AppliedOn { get; set; }

        [StringLength(5000)]
        [Display(Name = "Cover Letter")]
        public string? CoverLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Application Status")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [Display(Name = "Viewed By Employer")]
        public bool IsViewed { get; set; } = false;
        
        [Display(Name = "First Viewed On")]
        public DateTime? ViewedOn { get; set; }

        [StringLength(1000)]
        [Display(Name = "Feedback")]
        public string? AdminFeedback { get; set; }

        // Foreign key reference to Job
        public int JobId { get; set; }
        
        [ForeignKey("JobId")]
        public Job? Job { get; set; }
        
        // Foreign key reference to User
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }
    }
}
