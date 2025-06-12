using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seangroup.Data;
using Seangroup.Models;
using System.Security.Claims;

namespace Seangroup.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Страница оформления заказа
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartItems = await GetCartItemsAsync();
            ViewBag.CartItems = cartItems;

            return View(new Order
            {
                DeliveryMethod = "Самовывоз"
            });
        }

        // Обработка оформления заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            order.UserId = userId;

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                ModelState.AddModelError("", "Корзина пуста.");
                return View(order);
            }

            if (order.DeliveryMethod == "Самовывоз")
            {
                order.DeliveryAddress = "Самовывоз";
            }

            order.OrderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.Product.Id,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
                Quantity = item.Quantity
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await ClearCartAsync();

            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        // Подтверждение оформления заказа
        public IActionResult Confirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Просмотр всех заказов пользователя
        public async Task<IActionResult> UserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            foreach (var order in orders)
            {
                order.Total = order.OrderItems.Sum(item => item.Price * item.Quantity);
            }

            return View(orders);
        }
        public IActionResult Success(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return View(order); // => будет искать Views/Order/Success.cshtml
        }

        // Получение корзины текущего пользователя
        private async Task<List<CartItem>> GetCartItemsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        // Очистка корзины
        private async Task ClearCartAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var items = _context.CartItems.Where(c => c.UserId == userId);

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
