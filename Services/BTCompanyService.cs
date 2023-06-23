using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;
        public BTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company?> GetCompanyInfoAsync(int companyId)
        {
            
            try
            {
                return await _context.Companies
                                .Include(c => c.Members)
                                .Include(c => c.Projects)
                                    .ThenInclude(p => p.Tickets)
                                .Include(c => c.Invites)
                                    .ThenInclude(i => i.Project)
                                .Include(c => c.Invites)
                                    .ThenInclude(i => i.Invitor)
                                .FirstOrDefaultAsync(c => c.Id == companyId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetCompanyMembersAsync(int companyId)
        {
            try
            {
                return await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
