using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seangroup.Data;
using Seangroup.Models;

namespace Seangroup.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Каталог и просмотр

        /// <summary>
        /// Список товаров с опциональной фильтрацией по поисковому запросу.
        /// </summary>
        /// <param name="search">Поисковый запрос</param>
        /// <summary>
        /// Список товаров с опциональной фильтрацией по поисковому запросу (по любым совпадающим символам).
        /// </summary>
        /// <param name="search">Поисковый запрос</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string search)
        {
            // Базовый запрос с Include
            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.ProductDetails);

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Убираем лишние пробелы и приводим к нижнему регистру
                var trimmed = search.Trim();

                // Если серверная БД чувствительна к регистру, 
                // можно либо явно приводить к lower в запросе,
                // либо (что быстрее) использовать EF.Functions.Like на case-insensitive колляции:
                productsQuery = productsQuery.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) &&
                        EF.Functions.Like(p.Name, $"%{trimmed}%"))
                    ||
                    (!string.IsNullOrEmpty(p.Description) &&
                        EF.Functions.Like(p.Description, $"%{trimmed}%"))
                );
            }

            ViewData["CurrentFilter"] = search;
            var products = await productsQuery.ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            var userName = User.Identity.Name;

            var review = new Review
            {
                ProductId = productId,
                UserName = User.Identity.Name,
                Rating = rating,
                Comment = comment,            // может быть пустой строкой
                CreatedDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = productId });
        }
        /// <summary>
        /// Детали товара (страница).
        /// </summary>
        /// <param name="id">ID товара</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                // подгружаем детали
                .Include(p => p.ProductDetails)
                // подгружаем список отзывов
                .Include(p => p.Review)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        /// <summary>
        /// Детали товара в формате JSON (для AJAX/модалок).
        /// </summary>
        /// <param name="id">ID товара</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return Json(new
            {
                name = product.Name,
                description = product.Description,
                price = product.Price.ToString("N2"),
                imageUrl = product.ImageUrl,
                details = product.ProductDetails
                    .Select(d => new { d.AttributeName, d.AttributeValue })
            });
        }

        /// <summary>
        /// Поиск товаров и рендер частичной вьюхи карточек.
        /// </summary>
        /// <param name="search">Поисковый запрос</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProducts(string search)
        {
            var products = await _context.Products
                .Include(p => p.ProductDetails)
                .Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search))
                .ToListAsync();

            return PartialView("_ProductCardsPartial", products);
        }
        [HttpGet]
        public IActionResult GetReviews(int productId, int page = 1)
        {
            int pageSize = 5; // Количество отзывов на страницу
            var reviews = _context.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return PartialView("_ReviewsPartial", reviews);
        }

        #endregion

        #region Административные функции

        /// <summary>
        /// Форма создания нового товара.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Product { ProductDetails = new List<ProductDetail>() });
        }

        /// <summary>
        /// Обработка создания товара.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Исключаем пустые характеристики
            product.ProductDetails = product.ProductDetails?
                .Where(d =>
                    !string.IsNullOrWhiteSpace(d.AttributeName) ||
                    !string.IsNullOrWhiteSpace(d.AttributeValue))
                .ToList();

            if (!ModelState.IsValid)
                return View(product);

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании: {ex.InnerException?.Message ?? ex.Message}");
                return View(product);
            }
        }

        /// <summary>
        /// Форма редактирования товара.
        /// </summary>
        /// <param name="id">ID товара</param>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        /// <summary>
        /// Обработка редактирования товара.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(product);

            try
            {
                var existing = await _context.Products
                    .Include(p => p.ProductDetails)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existing == null)
                    return NotFound();

                // Обновляем основные поля
                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.ImageUrl = product.ImageUrl;

                // Удаляем удалённые характеристики
                var toRemove = existing.ProductDetails
                    .Where(d => !product.ProductDetails.Any(pd => pd.Id == d.Id))
                    .ToList();
                _context.ProductDetails.RemoveRange(toRemove);

                // Добавляем или обновляем характеристики
                foreach (var pd in product.ProductDetails)
                {
                    var existDetail = existing.ProductDetails
                        .FirstOrDefault(d => d.Id == pd.Id);

                    if (existDetail != null)
                    {
                        existDetail.AttributeName = pd.AttributeName;
                        existDetail.AttributeValue = pd.AttributeValue;
                    }
                    else
                    {
                        existing.ProductDetails.Add(new ProductDetail
                        {
                            AttributeName = pd.AttributeName,
                            AttributeValue = pd.AttributeValue
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound();
                throw;
            }
        }

        /// <summary>
        /// Подтверждение удаления товара.
        /// </summary>
        /// <param name="id">ID товара</param>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        /// <summary>
        /// Удаление товара.
        /// </summary>
        /// <param name="id">ID товара</param>
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Вспомогательное

        private bool ProductExists(int id)
            => _context.Products.Any(e => e.Id == id);

        #endregion
    }
}
