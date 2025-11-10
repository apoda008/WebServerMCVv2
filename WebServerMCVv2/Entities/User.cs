using System.Security.Claims;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace WebServerMVCv2.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public List<Claim> Claims; //= { new Claim (ClaimTypes.Role, Role) } 
    }
}
