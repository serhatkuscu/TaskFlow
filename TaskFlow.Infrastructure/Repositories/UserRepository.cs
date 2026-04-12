using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    // SQL Server unique constraint violation error numbers.
    // 2601: Cannot insert duplicate key row in object with unique index.
    // 2627: Violation of UNIQUE KEY constraint.
    private static readonly IReadOnlySet<int> UniqueConstraintErrorNumbers =
        new HashSet<int> { 2601, 2627 };

    private readonly TaskFlowDbContext _context;

    public UserRepository(TaskFlowDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

    public async Task<bool> ExistsAsync(string username)
        => await _context.Users.AnyAsync(x => x.Username == username);

    public async Task<bool> AddAsync(AppUser user)
    {
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx
                  && UniqueConstraintErrorNumbers.Contains(sqlEx.Number))
        {
            // Race condition: two concurrent registrations passed the ExistsAsync
            // check simultaneously and both tried to insert. The DB unique index
            // caught the second insert. We treat this identically to a known duplicate.
            _context.ChangeTracker.Clear();
            return false;
        }
    }
}
