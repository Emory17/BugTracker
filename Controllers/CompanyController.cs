﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Services.Interfaces;
using BugTracker.Extensions;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using BugTracker.Models.ViewModel;

namespace BugTracker.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;
        
        public CompanyController(IBTCompanyService companyService, IBTRolesService rolesService, UserManager<BTUser> userManager)
        {
            _companyService = companyService;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Index()
        {
            Company? company = await _companyService.GetCompanyInfoAsync(User.Identity!.GetCompanyId());

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<BTUser> members = await _companyService.GetCompanyMembersAsync(User.Identity!.GetCompanyId());
            List<IdentityRole> roles = await _rolesService.GetRolesAsync();

            List <ManageUserRolesViewModel> model= new();
            foreach(BTUser member in members)
            {
                if(member.Id != _userManager.GetUserId(User) && !await _rolesService.IsUserInRole(member, nameof(BTRoles.DemoUser)))
                {
                    IEnumerable<string> userRoles = await _rolesService.GetUserRolesAsync(member);

                    ManageUserRolesViewModel viewModel = new()
                    {
                        Roles = new SelectList(roles, "Name", "Name", userRoles.FirstOrDefault()),
                        User = member
                    };

                    model.Add(viewModel);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            string? selectedRole = viewModel.SelectedRole;
            string? userId = viewModel.User?.Id;

            if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(selectedRole))
            {
                return NotFound();
            }

            BTUser? user = await _userManager.FindByIdAsync(userId);

            if (user == null) { return NotFound(); }

            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(user);
            if(await _rolesService.RemoveUserFromRolesAsync(user, currentRoles))
            {
                await _rolesService.AddUserToRoleAsync(user, selectedRole);
            }

            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}
