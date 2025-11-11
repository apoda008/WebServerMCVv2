using WebServerMVCv2.Entities;
using WebServerMVCv2.Models;
//using WebServerMCV.Services.DataTransferObjects;

namespace WebServerMVCv2.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<User?> LoginAsync(UserDto request);
    }
}
