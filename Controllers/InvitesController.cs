using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.Enums;
using BugTracker.Extensions;

namespace BugTracker.Controllers
{
    [Authorize(Roles = nameof(BTRoles.Admin))]
    public class InvitesController : Controller
    {
        private readonly IBTInviteService _inviteService;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<BTUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _protector;
        private readonly string _protectorPurpose;

        public InvitesController(IBTInviteService inviteService, IBTProjectService projectService, IBTCompanyService companyService, IEmailSender emailSender, IConfiguration configuration, UserManager<BTUser> userManager, IDataProtectionProvider protectionProvider)
        {
            _inviteService = inviteService;
            _projectService = projectService;
            _companyService = companyService;
            _emailSender = emailSender;
            _userManager = userManager;
            _configuration = configuration;

            _protectorPurpose = _configuration["ProtectorPurpose"] ?? Environment.GetEnvironmentVariable("ProtectorPurpose")!;
            _protector = protectionProvider.CreateProtector(_protectorPurpose);
        }

        // GET Invites/Create
        public async Task<IActionResult> Create()
        {
            List<Project> companyProjects = await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
            ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");

            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InviteeEmail,InviteeFirstName,InviteeLastName,Message,ProjectId")] Invite invite)
        {
            int companyId = User.Identity!.GetCompanyId();

            ModelState.Remove(nameof(invite.InvitorId));

            if(ModelState.IsValid)
            {
                try
                {
                    Guid guid = Guid.NewGuid();
                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DateTime.UtcNow;
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;

                    await _inviteService.AddNewInviteAsync(invite);

                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail!);
                    string company = _protector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, Request.Scheme);

                    string body = $@"<h4>You've been invited to join the bug tracker!</h4><br />
                                        {invite.Message}<br /><br />
                                        <a href=""{callbackUrl}"">Click here</a> to join our team.";

                    string subject = "You've been invited to join the bug tracker";

                    await _emailSender.SendEmailAsync(invite.InviteeEmail!, subject, body);

                    return RedirectToAction("Index", "Home"); //new {SwalMessage = "Invite Sent!"}
                }
                catch (Exception)
                {
                    throw;
                }
            }

            List<Project> companyProjects = await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
            ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name", invite.ProjectId);

            return View(invite);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string? token, string? email, string? company)
        {
            if(string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company))
            {
                NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string inviteeEmail = _protector.Unprotect(email);
            int companyId = int.Parse(_protector.Unprotect(company));

            try
            {
                Invite? invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if (invite == null)
                {
                    return NotFound();
                }

                return View(invite);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> CancelInvite(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            await _inviteService.CancelInviteAsync(id.Value, User.Identity!.GetCompanyId());

            return RedirectToAction("Index", "Company");
        }
    }
}
