using System.Security.Claims;

namespace WebServerMVCv2.Models
{
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        //public List<Claim> Claims { get; set; }
    }
}
