using Microsoft.EntityFrameworkCore;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _db;

        public UnitOfWork(MyDbContext db)
        {
            _db = db;
            Animal = new AnimalRepository(_db);
            CoverType = new CoverTypeRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            shoppingCart = new ShoppingCartRepository(_db);
            applicationUser = new ApplicationUserRepository(_db);
            orderHeader = new OrderHeaderRepository(_db);
            orderDetail = new OrderDetailRepository(_db);
        }

        public IAnimalRepository Animal { get; private set; }
        public ICoverTypeRepository CoverType { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository shoppingCart { get; private set; }
        public IApplicationUserRepository applicationUser { get; private set; }

        public IOrderHeaderRepository orderHeader { get; private set; }

        public IOrderDetailRepository orderDetail { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}