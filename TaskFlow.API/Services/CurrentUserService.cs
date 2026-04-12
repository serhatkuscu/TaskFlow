using System.Security.Claims;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.API.Services;

/// <summary>
/// Reads the current user's identity from the active HTTP request context.
/// Registered as Scoped so each request gets its own instance bound
/// to that request's IHttpContextAccessor.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public int UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            // This should never happen on [Authorize] endpoints:
            // a valid JWT always contains this claim (set by JwtTokenService).
            // If it does happen, UnauthorizedAccessException is caught by
            // ExceptionMiddleware and returned as 401 Unauthorized.
            if (claim is null || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException(
                    "User ID claim is missing or invalid in the token.");

            return userId;
        }
    }
}
