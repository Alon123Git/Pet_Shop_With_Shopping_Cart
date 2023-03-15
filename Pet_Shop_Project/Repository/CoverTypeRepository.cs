using Microsoft.EntityFrameworkCore;
using Models;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly MyDbContext _db;

        public CoverTypeRepository(MyDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CoverType obj)
        {
            _db.coverType.Update(obj);
        }
    }
}