using EbayClone.API.DTOs.Auth;
using EbayClone.API.Helpers;
using EbayClone.API.Models;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AuthService(IUserRepository userRepository, JwtHelper jwtHelper) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (user.ModerationStatus != "Active" || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = jwtHelper.CreateToken(user);
        return new LoginResponseDto(token, user.Id, user.Email, user.Role);
    }
}
