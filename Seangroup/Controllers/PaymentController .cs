using Microsoft.AspNetCore.Mvc;
using Yandex.Checkout.V3;
using Seangroup.Services;
using Microsoft.EntityFrameworkCore;
using Seangroup.Data;

namespace Seangroup.Controllers
{
    public class PaymentController : Controller
    {
        private readonly Yandex.Checkout.V3.Client _client;
        private readonly ApplicationDbContext _context;
        public PaymentController(Yandex.Checkout.V3.Client client) // Внедрение зависимостей
        {
            _client = client;
        }

        public async Task<IActionResult> CreatePayment(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            var total = order.OrderItems.Sum(i => i.Price * i.Quantity);

            var asyncClient = _client.MakeAsync();

            var payment = await asyncClient.CreatePaymentAsync(new NewPayment
            {
                Amount = new Amount
                {
                    Value = total,
                    Currency = "RUB"
                },
                Confirmation = new Confirmation
                {
                    Type = ConfirmationType.Redirect,
                    ReturnUrl = $"https://localhost:7059/Order/Success/{order.Id}"
                },
                Description = $"Оплата заказа №{order.Id}"
            });

            return Redirect(payment.Confirmation.ConfirmationUrl);
        }

    }
}