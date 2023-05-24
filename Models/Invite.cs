using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Invite Date")]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Join Date")]
        public DateTime? JoinDate { get; set; }

        [Display(Name = "Company Token")]
        public Guid CompanyToken { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeFirstName{ get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        [Display(Name = "Valid?")]
        public bool IsValid { get; set; }

        //Foreign Keys
        public int CompanyId { get; set; }
        public int? ProjectId { get; set; }
        public string? InviteeId { get; set;}

        [Required]
        public string? InvitorId { get; set; }

        //Navigation
        public virtual Company? Company { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Invitee { get; set; }
        public virtual BTUser? Invitor { get; set; }
    }
}
