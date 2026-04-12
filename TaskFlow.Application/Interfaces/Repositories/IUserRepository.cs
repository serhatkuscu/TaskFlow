using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<bool> ExistsAsync(string username);
    Task AddAsync(AppUser user);
}
