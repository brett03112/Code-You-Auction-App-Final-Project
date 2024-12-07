using System.ComponentModel.DataAnnotations;
using CommunityCenter.Models;
using Microsoft.AspNetCore.Http;

namespace CommunityCenter.ViewModels
{
    public class ProfileViewModels
    {
        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
