﻿using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token && i.CompanyId == companyId && i.IsValid);

                if (invite == null)
                {
                    return false;
                }

                invite.IsValid = false;
                invite.InviteeId = userId;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                _context.Add(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                return await _context.Invites.Where(i => i.CompanyId == companyId).AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task CancelInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Id == inviteId);

                if(invite is not null)
                {
                    invite.IsValid = false;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(int inviteId, int companyId)
        {  
            try
            {
                return await _context.Invites.Include(i => i.Company).Include(i => i.Project).Include(i => i.Invitor).Include(i => i.Invitee).FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Id == inviteId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                return await _context.Invites.Include(i => i.Company).Include(i => i.Project).Include(i => i.Invitor).Include(i => i.Invitee).FirstOrDefaultAsync(i => i.CompanyId == companyId && i.InviteeEmail == email && i.CompanyToken == token);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateInviteAsync(Invite invite)
        {
            try
            {
                _context.Update(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {
                if(token is null)
                {
                    return false;
                }

                bool result = false;

                Invite? invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if(invite != null)
                {
                    DateTime inviteDate = invite.InviteDate;

                    bool notExpired = (DateTime.UtcNow - inviteDate).TotalDays <= 7;

                    if (notExpired)
                    {
                        result = invite.IsValid;
                    }
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
