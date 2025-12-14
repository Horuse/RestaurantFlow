using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Repositories;

public interface IStaffRepository : IRepository<Staff>
{
    Task<List<Staff>> GetActiveStaffAsync();
    Task<List<Staff>> GetStaffByRoleAsync(StaffRole role);
    Task SoftDeleteAsync(int id);
}