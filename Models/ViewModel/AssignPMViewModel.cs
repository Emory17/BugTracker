using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModel
{
    public class AssignPMViewModel
    {
        public Project? Project { get; set; }
        public SelectList? PMList { get; set; }
        public string? PMId { get; set; }
    }
}
