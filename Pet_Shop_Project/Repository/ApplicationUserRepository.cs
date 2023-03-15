using Microsoft.EntityFrameworkCore;
using Models;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly MyDbContext _db;

        public ApplicationUserRepository(MyDbContext db) : base(db)
        {
            _db = db;
        }
    }
}