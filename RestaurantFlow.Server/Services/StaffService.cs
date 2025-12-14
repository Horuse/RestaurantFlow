using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;
using RestaurantFlow.Server.Repositories;

namespace RestaurantFlow.Server.Services;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    
    public StaffService(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }
    
    public async Task<List<Staff>> GetStaffAsync()
    {
        return await _staffRepository.GetAllAsync();
    }
    
    public async Task<List<Staff>> GetActiveStaffAsync()
    {
        return await _staffRepository.GetActiveStaffAsync();
    }
    
    public async Task<List<Staff>> GetStaffByRoleAsync(StaffRole role)
    {
        return await _staffRepository.GetStaffByRoleAsync(role);
    }
    
    public async Task<Staff> CreateStaffAsync(Staff staff)
    {
        return await _staffRepository.AddAsync(staff);
    }
    
    public async Task<Staff> UpdateStaffAsync(Staff staff)
    {
        return await _staffRepository.UpdateAsync(staff);
    }
    
    public async Task DeleteStaffAsync(int id)
    {
        await _staffRepository.SoftDeleteAsync(id);
    }
}