using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using webapi.User;

namespace FunctionsApi.Services
{
    public class LoginService
    {
        public UserModel Authenticate(UserLogin userLogin)
        {
            var currentUser = UserCredentials.Users
                .FirstOrDefault(u =>
                u.UserName.ToLower() == userLogin.UserName.ToLower() &&
                u.Password == userLogin.Password);

            if (currentUser != null)
            {
                return currentUser;
            }
            return null;
        }

        public string Generate(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QJF0R46v4nYFL6PilN2bMd1VhxvG3uJHpdSH28mg"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.GivenName, user.GivenName),
                new Claim(ClaimTypes.Surname, user.SurName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken(
                "https://localhost:4200",
                "https://localhost:4200",
                claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool IsAuthenticated(string authorizationHeader)
        {
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                string token = authorizationHeader.Substring(7);

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QJF0R46v4nYFL6PilN2bMd1VhxvG3uJHpdSH28mg")),
                    ValidateIssuer = true,
                    ValidIssuer = "https://localhost:4200",
                    ValidateAudience = true,
                    ValidAudience = "https://localhost:4200"
                };

                try
                {
                    SecurityToken validatedToken;
                    ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                    // If the validation succeeds, the token is valid.
                    return true;
                }
                catch (SecurityTokenException)
                {
                    // Token validation failed.
                    return false;
                }
            }
            return false;
        }
    }
}
