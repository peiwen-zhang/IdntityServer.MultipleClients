using Microsoft.EntityFrameworkCore;

namespace MyIdentityServer.DB
{
    /// <summary>
    /// 智慧政务上下文链接
    /// </summary>
    public class IntelContext : DbContext
    {
        public IntelContext(DbContextOptions<IntelContext> option) : base(option)
        {
        }
        public DbSet<User> UserDb { get; set; }
    }
}
