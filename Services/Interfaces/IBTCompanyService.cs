﻿using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        Task<Company?> GetCompanyInfoAsync(int companyId);
        Task<List<BTUser>> GetCompanyMembersAsync(int companyId);
    }
}
