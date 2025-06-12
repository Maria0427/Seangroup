using Seangroup.Models;

namespace Seangroup.Services
{
    public interface IOrderService
    {
        Order GetOrder(int id);
        int CreateOrder(Order order);
        void MarkPaid(int id);

    }
}
