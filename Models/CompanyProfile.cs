using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace JobPortal.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500, ErrorMessage = "Company description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? LogoUrl { get; set; }
        
        [StringLength(100)]
        public string? Industry { get; set; }
        
        [StringLength(200)]
        public string? WebsiteUrl { get; set; }
        
        [StringLength(50)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        // Foreign key reference to User (Employer)
        [Required]
        public string EmployerId { get; set; } = string.Empty;
        
        [ForeignKey("EmployerId")]
        public virtual IdentityUser? Employer { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastUpdated { get; set; }
        
        [StringLength(1000)]
        public string? SocialMediaLinks { get; set; }
    }
}
