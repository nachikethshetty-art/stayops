namespace StayOps.Application.Auth;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);
    Task<TokenResponse> RefreshAsync(RefreshRequest request, string? ipAddress, CancellationToken ct = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);
    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
