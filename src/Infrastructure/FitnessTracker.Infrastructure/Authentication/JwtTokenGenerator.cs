using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FitnessTracker.Infrastructure.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions, UserManager<ApplicationUser> userManager)
        {
            _jwtSettings = jwtOptions.Value;
            _userManager = userManager;
        }

        public async Task<TokenResult> GenerateTokenDetailsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim("uid", user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims, 
                expires: expires,
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(tokenDescriptor);

            return new TokenResult
            {
                Token = tokenString,
                ExpiresOn = expires
            };
        }
    }
}