using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        public string? PropertyName { get; set; }
        public string? Description { get; set; }
        public DateTime Created { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        //Foreign Keys
        public int TicketId { get; set; }
        public string? UserId { get; set; }

        //Navigation
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
