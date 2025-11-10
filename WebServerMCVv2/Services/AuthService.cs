using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebServerMVCv2.Data;
using WebServerMVCv2.Entities;
using WebServerMVCv2.Models;

namespace WebServerMVCv2.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<User> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if(user == null)
            {
                //wrong user name
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                //password failed
                return null;
            }

            //string token = CreateToken(user);

            return user;
        }

        public async Task<User?> RegisterAsync(UserDto request) 
        {
            if (await context.Users.AnyAsync(u => u.Username == request.Username)) 
            {
                //same user name or already exists
                return null;
            }

            var user = new User();

            var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;
            user.Claims = new List<Claim>
                { 
                    //claim == key value pair
                    new Claim(ClaimTypes.Name, "jdoe"),

                };
            context.Users.Add(user);
            await context.SaveChangesAsync();


            return user;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings: Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings: Audience"),
                audience: configuration.GetValue<string>("Appsettings: Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
