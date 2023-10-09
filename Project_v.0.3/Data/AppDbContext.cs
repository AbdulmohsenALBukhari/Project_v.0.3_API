using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project_v._0._3.Model;
using Project_v._0._3.ModelViews;

namespace Project_v._0._3.Data
{
    public class AppDbContext : IdentityDbContext<AccountUserModel, AccountRoleModel, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
    }
}
