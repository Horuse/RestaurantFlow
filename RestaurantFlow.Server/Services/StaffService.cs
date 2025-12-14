using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Services;

public class StaffService : IStaffService
{
    private readonly RestaurantDbContext _context;
    
    public StaffService(RestaurantDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Staff>> GetStaffAsync()
    {
        return await _context.Staff
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<List<Staff>> GetActiveStaffAsync()
    {
        return await _context.Staff
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<List<Staff>> GetStaffByRoleAsync(StaffRole role)
    {
        return await _context.Staff
            .Where(s => s.IsActive && s.Role == role)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<Staff> CreateStaffAsync(Staff staff)
    {
        staff.CreatedAt = DateTime.UtcNow;
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();
        return staff;
    }
    
    public async Task<Staff> UpdateStaffAsync(Staff staff)
    {
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync();
        return staff;
    }
    
    public async Task DeleteStaffAsync(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff != null)
        {
            staff.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
        }
    }
}