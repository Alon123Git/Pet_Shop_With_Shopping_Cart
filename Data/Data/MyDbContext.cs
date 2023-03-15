using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Pet_Shop_Project
{
    public class MyDbContext : IdentityDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) 
        { 
        }

        public DbSet<Animal> Animals { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<CoverType> coverType { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<ApplicationUser> applicationUsers { get; set; }
        public DbSet<Company> company { get; set; }
        public DbSet<ShoppingCart> shoppingCart { get; set; }
        public DbSet<OrderHeader> orderHeader { get; set; }
        public DbSet<OrderDetail> orderDetails { get; set; }
     }
}
