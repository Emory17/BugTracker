using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        public string? Comment { get; set; }

        public DateTime Created { get; set; }

        //Foreign Keys
        public int TicketId { get; set; }
        public string? UserId { get; set; }

        //Navigation
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
