using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModel
{
    public class ManageUserRolesViewModel
    {
        public BTUser? User { get; set; }
        public SelectList? Roles { get; set; }
        public string? SelectedRole { get; set; }
    }
}
