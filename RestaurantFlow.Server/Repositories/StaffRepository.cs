using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Repositories;

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<List<Staff>> GetActiveStaffAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<Staff>> GetStaffByRoleAsync(StaffRole role)
    {
        return await _dbSet
            .Where(s => s.IsActive && s.Role == role)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var staff = await _dbSet.FindAsync(id);
        if (staff != null)
        {
            staff.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public override async Task DeleteAsync(int id)
    {
        await SoftDeleteAsync(id);
    }

    public override async Task<Staff> AddAsync(Staff entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        return await base.AddAsync(entity);
    }
}