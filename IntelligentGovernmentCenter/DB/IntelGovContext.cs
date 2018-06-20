using Microsoft.EntityFrameworkCore;

namespace IntelligentGovernmentCenter.DB
{
    /// <summary>
    /// 智慧政务上下文链接
    /// </summary>
    public class IntelGovContext : DbContext
    {
        public IntelGovContext(DbContextOptions<IntelGovContext> option) : base(option)
        {
        }
        public DbSet<User> UserDb { get; set; }
    }
}
