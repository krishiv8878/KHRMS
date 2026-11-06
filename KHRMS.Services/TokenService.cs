using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KHRMS.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace KHRMS.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GeneratePasswordResetToken(string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var secretKey = _configuration["Jwt:PasswordResetSecret"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("Jwt:PasswordResetSecret is not configured. Please supply a valid 256-bit secret.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:issuer"],
                audience: _configuration["Jwt:audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidatePasswordResetToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:PasswordResetSecret"];
            if (string.IsNullOrWhiteSpace(secretKey)) return null;
            var key = Encoding.UTF8.GetBytes(secretKey);
            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = Convert.ToBoolean(_configuration["Jwt:ValidateIssuerSigningKey"]),
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = Convert.ToBoolean(_configuration["Jwt:ValidateIssuer"]),
                    ValidateAudience = Convert.ToBoolean(_configuration["Jwt:ValidateAudience"]),
                    ValidIssuer = _configuration["Jwt:ValidIssuer"],
                    ValidAudience = _configuration["Jwt:ValidAudience"],
                    ValidateLifetime = Convert.ToBoolean(_configuration["Jwt:ValidateLifetime"]),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
        }

        public string GenerateInvitationToken(string email, long employeeId, int expiryHours = 48)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("EmployeeId", employeeId.ToString()),
                new Claim("token_type", "employee_invitation"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var secretKey = _configuration["Jwt:PasswordResetSecret"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("Jwt:PasswordResetSecret is not configured. Please supply a valid 256-bit secret.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var issuer = _configuration["Jwt:issuer"] ?? _configuration["Jwt:ValidIssuer"];
            var audience = _configuration["Jwt:audience"] ?? _configuration["Jwt:ValidAudience"];

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateInvitationToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:PasswordResetSecret"];
            if (string.IsNullOrWhiteSpace(secretKey)) return null;
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                var tokenType = principal.FindFirst("token_type")?.Value;
                if (!string.Equals(tokenType, "employee_invitation", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
