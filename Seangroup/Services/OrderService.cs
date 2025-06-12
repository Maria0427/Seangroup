using Seangroup.Data;
using Microsoft.EntityFrameworkCore;
using Seangroup.Models;
namespace Seangroup.Services

{
    public class OrderService : IOrderService
    {
        private readonly SeangroupDbContext _db;

        public OrderService(SeangroupDbContext db)
        {
            _db = db;
        }

        public int CreateOrder(Order order)
        {
            _db.Orders.Add(order);
            _db.SaveChanges();
            return order.Id;
        }

        public Order GetOrder(int id)
        {
            return _db.Orders
                     .Include(o => o.OrderItems)
                     .FirstOrDefault(o => o.Id == id);
        }

        public void MarkPaid(int id)
        {
            var order = _db.Orders.Find(id);
            if (order != null)
            {
                order.Status = "Оплачен";
                _db.SaveChanges();
            }
        }
    }
}
