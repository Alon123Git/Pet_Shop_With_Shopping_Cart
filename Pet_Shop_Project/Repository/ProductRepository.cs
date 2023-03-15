using Models;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly MyDbContext _db;

        public ProductRepository(MyDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            var UpdateMoreThanOne = _db.products.FirstOrDefault(a => a.Id == obj.Id);
            if (UpdateMoreThanOne != null)
            {
                UpdateMoreThanOne.Name = obj.Name;
                UpdateMoreThanOne.OwnerPhoneNumber = obj.OwnerPhoneNumber;
                if (obj.ImageUrl != null)
                {
                    UpdateMoreThanOne.ImageUrl = obj.ImageUrl;
                }
            }
            _db.products.Update(obj);
        }
    }
}