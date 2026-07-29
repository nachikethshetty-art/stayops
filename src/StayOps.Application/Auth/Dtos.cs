namespace StayOps.Application.Auth;

public record LoginRequest(string UserNameOrEmail, string Password);

public record RefreshRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);

public record TokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    CurrentUserDto User);

public record CurrentUserDto(
    Guid Id,
    string UserName,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid> AccessibleHotelIds,
    bool IsSuperAdmin);
