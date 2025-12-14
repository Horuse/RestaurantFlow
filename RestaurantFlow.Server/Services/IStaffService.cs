using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantFlow.Server.Services;

public interface IStaffService
{
    Task<List<Staff>> GetStaffAsync();
    Task<List<Staff>> GetActiveStaffAsync();
    Task<List<Staff>> GetStaffByRoleAsync(StaffRole role);
    Task<Staff> CreateStaffAsync(Staff staff);
    Task<Staff> UpdateStaffAsync(Staff staff);
    Task DeleteStaffAsync(int id);
}