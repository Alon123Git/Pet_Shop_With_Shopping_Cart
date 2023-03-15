using Microsoft.EntityFrameworkCore;
using Models;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly MyDbContext _db;

        public OrderDetailRepository(MyDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.orderDetails.Update(obj);
        }
    }
}