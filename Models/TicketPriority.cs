﻿using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Priority")]
        public string? Name { get; set; }
    }
}
