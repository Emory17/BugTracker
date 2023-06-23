using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BTRolesService(UserManager<BTUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            try
            {
                if (roleName.Equals(nameof(BTRoles.Admin)) || roleName.Equals(nameof(BTRoles.ProjectManager)))
                {
                    List<Project> projects = await _context.Projects.Include(p => p.Members).Where(p => p.Members.Contains(user)).ToListAsync();

                    foreach (Project project in projects)
                    {
                        project.Members.Remove(user);
                    }

                    await _context.SaveChangesAsync();
                }

                return (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                return await _context.Roles.Where(r => r.Name != nameof(BTRoles.DemoUser)).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {
                return (await _userManager.GetUsersInRoleAsync(roleName)).Where(u => u.CompanyId == companyId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsUserInRole(BTUser member, string roleName)
        {
            try
            {
                return await _userManager.IsInRoleAsync(member, roleName);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            try
            {
                if (roleName.Equals(nameof(BTRoles.Developer)))
                {
                    List<Ticket> tickets = await _context.Tickets.Where(t => t.DeveloperUserId == user.Id).ToListAsync();

                    foreach (Ticket ticket in tickets)
                    {
                        ticket.DeveloperUserId = null;
                    }

                    await _context.SaveChangesAsync();
                }

                bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleNames)
        {
            try
            {
                if(roleNames.Contains(nameof(BTRoles.Developer)))
                {
                    List<Ticket> tickets = await _context.Tickets.Where(t => t.DeveloperUserId == user.Id).ToListAsync();

                    foreach (Ticket ticket in tickets) 
                    {
                        ticket.DeveloperUserId = null;
                    }

                    await _context.SaveChangesAsync();
                }

                bool result = (await _userManager.RemoveFromRolesAsync(user, roleNames)).Succeeded;
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
