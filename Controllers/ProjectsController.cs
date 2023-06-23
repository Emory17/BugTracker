using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using System.ComponentModel.Design;
using BugTracker.Extensions;
using BugTracker.Models.ViewModel;
using System.Collections;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;

        public ProjectsController(UserManager<BTUser> userManager, IBTFileService fileService, IBTProjectService projectService, IBTRolesService rolesService, IBTCompanyService companyService)
        {
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _rolesService = rolesService;
            _companyService = companyService;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string? title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return NotFound();
            }

            List<Project> projects = new();

            if(title == "My Projects")
            {
                projects = await _projectService.GetUserProjectsAsync(_userManager.GetUserId(User)!);
            }
            else if(title == "All Projects")
            {
                projects = await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
            }
            else if (title == "Archived Projects")
            {
                projects = await _projectService.GetArchivedProjectsAsync(User.Identity!.GetCompanyId());
            }
            else if(title == "Unassigned Projects")
            {
                projects = await _projectService.GetUnassignedProjectsAsync(User.Identity!.GetCompanyId());
            }

            ViewData["Heading"] = title;
            return View(projects);
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(int? projectId, string? PMId)
        {
            if (projectId != null)
            {
                if(string.IsNullOrEmpty(PMId))
                {
                    await _projectService.RemoveProjectManagerAsync(projectId.Value, User.Identity!.GetCompanyId());
                }
                else
                {
                    await _projectService.AddProjectManagerAsync(PMId, projectId.Value, User.Identity!.GetCompanyId());
                }

                return RedirectToAction(nameof(Details), new {id = projectId});
            }

            return BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMembers(int? projectId, string? memberId)
        {
            if (projectId is not null)
            {
                Project? project = await _projectService.GetProjectByIdAsync(projectId.Value, User.Identity!.GetCompanyId());

                if (project == null)
                {
                    return BadRequest();
                }

                List<BTUser> members = await _companyService.GetCompanyMembersAsync(User.Identity!.GetCompanyId());

                if (!string.IsNullOrEmpty(memberId))
                {
                    BTUser? member = members.FirstOrDefault(u => u.Id == memberId);
                    if(member is not null)
                    {
                        if (project.Members.Contains(member))
                        {
                            await _projectService.RemoveMemberFromProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
                        }
                        else
                        {
                            await _projectService.AddMemberToProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
                        }
                        
                    }
                }

                return RedirectToAction(nameof(Details), new { id = projectId });
            }

            return BadRequest();
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create()
        {
            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project)
        {
            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {
                project.Created = DateTime.UtcNow;
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                BTUser? user = await _userManager.GetUserAsync(User);
                project.CompanyId = User.Identity!.GetCompanyId();

                if (project.ImageFormFile is not null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                if(User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user!);
                }

                await _projectService.AddProjectAsync(project);
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }

            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());
            if (project == null)
            {
                return NotFound();
            }

            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,StartDate,EndDate,Archived,ImageFormFile,ImageFileData,ImageFileType,CompanyId,ProjectPriorityId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.Created = DateTime.SpecifyKind(project.Created, DateTimeKind.Utc);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                    if (project.ImageFormFile is not null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(project, User.Identity!.GetCompanyId());
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }

            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());
            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project, User.Identity!.GetCompanyId());

                return RedirectToAction(nameof(Details), new { id = project.Id });
            }

            return NotFound();
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());
            if (project is not null)
            {
                await _projectService.RestoreProjectAsync(project, User.Identity!.GetCompanyId());

                return RedirectToAction(nameof(Details), new { id = project.Id });
            }

            return NotFound();
        }
    }
}
