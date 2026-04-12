namespace TaskFlow.Application.Interfaces.Services;

/// <summary>
/// Provides identity information about the currently authenticated user.
/// The interface lives in Application so handlers can depend on it
/// without touching any HTTP or infrastructure concern.
/// The implementation lives in the API layer (IHttpContextAccessor).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Returns the authenticated user's database ID extracted from the JWT token.
    /// Throws <see cref="UnauthorizedAccessException"/> if the claim is absent or malformed.
    /// Only call this from endpoints protected by [Authorize].
    /// </summary>
    int UserId { get; }

    /// <summary>
    /// True when a valid, authenticated principal is present in the current request.
    /// Use this before accessing UserId in endpoints that allow anonymous access.
    /// </summary>
    bool IsAuthenticated { get; }
}
