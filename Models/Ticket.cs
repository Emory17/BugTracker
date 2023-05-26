using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Ticket Name")]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set;}

        [Display(Name = "Archived?")]
        public bool Archived { get; set; }

        [Display(Name = "Project Finished?")]
        public bool ArchivedByProject { get; set; }

        //Foreign Keys
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Type")]
        public int TicketTypeId { get; set; }

        [Display(Name = "Status")]
        public int TicketStatusId { get; set; }

        [Display(Name = "Priority")]
        public int TicketPriorityId { get; set;}

        [Display(Name = "Developer")]
        public string? DeveloperUserId { get; set; }

        [Required]
        public string? SubmitterUserId;

        //Navigation
        public virtual Project? Project { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        public virtual TicketType? TicketType { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }
        public virtual BTUser? DeveloperUser { get; set; }
        public virtual BTUser? SubmitterUser { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}
