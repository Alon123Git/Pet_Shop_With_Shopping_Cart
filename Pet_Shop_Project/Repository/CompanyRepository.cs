using Models;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private MyDbContext _db;

        public CompanyRepository(MyDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company obj)
        {
            _db.company.Update(obj);
        }
    }
}