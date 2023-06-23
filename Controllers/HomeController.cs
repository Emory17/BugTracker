using System.Diagnostics;
using BugTracker.Extensions;
using BugTracker.Models.ViewModel;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly IBTProjectService _projectService;
		private readonly IBTTicketService _ticketService;
		private readonly IBTCompanyService _companyService;

		public HomeController(ILogger<HomeController> logger, IBTProjectService projectService, IBTTicketService ticketService, IBTCompanyService companyService)
        {
            _logger = logger;
            _projectService = projectService;
            _ticketService = ticketService;
            _companyService = companyService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            int companyId = User.Identity!.GetCompanyId();

            DashboardViewModel dbvm = new DashboardViewModel()
            {
                Company = await _companyService.GetCompanyInfoAsync(companyId),
                Members = await _companyService.GetCompanyMembersAsync(companyId),
                Projects = await _projectService.GetProjectsByCompanyIdAsync(companyId),
                Tickets = await _ticketService.GetTicketsByCompanyIdAsync(companyId)
            };

            return View(dbvm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}