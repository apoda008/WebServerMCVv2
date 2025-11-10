using Microsoft.EntityFrameworkCore;
using WebServerMVCv2.Entities;

namespace WebServerMVCv2.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)    
    {
        public DbSet<User> Users { get; set; }
    }
}
