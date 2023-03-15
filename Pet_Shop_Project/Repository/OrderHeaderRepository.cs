using Microsoft.EntityFrameworkCore;
using Models;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly MyDbContext _db;

        public OrderHeaderRepository(MyDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader obj)
        {
            _db.orderHeader.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.orderHeader.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (paymentStatus != null)
                {
                    orderFromDb.PatmentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.orderHeader.FirstOrDefault(u => u.Id == id);
            orderFromDb.PatmentDueDate = DateTime.Now;
            orderFromDb.SessionId = sessionId;
            orderFromDb.PaymenIntentId = paymentIntentId;
        }
    }
}