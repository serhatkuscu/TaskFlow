using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TaskFlowDbContext _context;

    public UserRepository(TaskFlowDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

    public async Task<bool> ExistsAsync(string username)
        => await _context.Users.AnyAsync(x => x.Username == username);

    public async Task AddAsync(AppUser user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
