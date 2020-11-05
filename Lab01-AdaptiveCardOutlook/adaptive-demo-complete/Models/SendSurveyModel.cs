using System.ComponentModel.DataAnnotations;

namespace adaptive_demo.Models
{
    public class SendSurveyModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Username", Description = "Username who will send the survey")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "User Password", Description = "User password")]
        public string UserPassword { get; set; }

        [Required]
        [Display(Name = "Recipient Emails", Description = "List of recipients email address, use comma separated for multiple recipients")]
        public string Recipients { get; set; }

        [Display(Name = "Extra messages", Description = "Extra message in the survey")]
        public string Message { get; set; }
    }
}