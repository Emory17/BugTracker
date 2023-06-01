using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModel
{
    public class AssignDeveloperViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DevList { get; set; }
        public string? DevId { get; set; }
    }
}
