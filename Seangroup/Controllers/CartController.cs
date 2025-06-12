using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seangroup.Data;
using Seangroup.Models;

namespace Seangroup.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Добавление и обработка временной корзины

        /// <summary>
        /// POST: Добавляет товар в корзину. Если пользователь не авторизован — сохраняет временные данные и перенаправляет на логин.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Add(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["TempProductId"] = productId;
                TempData["TempQuantity"] = quantity;
                var returnUrl = Url.Action("ProcessTempCart", "Cart");
                return Redirect($"/Identity/Account/Login?returnUrl={returnUrl}");
            }

            AddOrUpdateCartItem(userId, productId, quantity, product.Price);

            // Возврат на предыдущую страницу
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index", "Catalog");
        }

        /// <summary>
        /// GET: Обработка временной корзины после авторизации.
        /// </summary>
        [HttpGet]
        public IActionResult ProcessTempCart()
        {
            var productId = TempData["TempProductId"] as int?;
            var quantity = TempData["TempQuantity"] as int?;

            if (productId == null || quantity == null)
                return RedirectToAction("Index", "Home");

            // Перенаправляем на GET Add (ниже)
            return RedirectToAction("AddAfterLogin", new { productId, quantity });
        }

        /// <summary>
        /// GET: Добавляет товар в корзину после возврата с логина.
        /// </summary>
        [HttpGet]
        public IActionResult AddAfterLogin(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

            AddOrUpdateCartItem(userId, productId, quantity, product.Price);
            return RedirectToAction(nameof(Index));
        
        }

        #endregion

        #region Просмотр и управление корзиной

        /// <summary>
        /// Отображает текущие элементы корзины пользователя.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(items);
        }

        /// <summary>
        /// Удаляет элемент из корзины.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Обновляет количество товара в корзине.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity <= 0)
                return RedirectToAction(nameof(Index));

            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                item.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Добавляет новый элемент в корзину или увеличивает количество существующего.
        /// </summary>
        private void AddOrUpdateCartItem(string userId, int productId, int quantity, decimal price)
        {
            var existing = _context.CartItems
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = price
                });

            _context.SaveChanges();
        }

        #endregion
    }
}
