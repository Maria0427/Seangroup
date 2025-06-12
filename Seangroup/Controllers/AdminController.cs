using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seangroup.Data;
using Seangroup.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Seangroup.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SeangroupDbContext _context;

        public AdminController(SeangroupDbContext context)
        {
            _context = context;
        }

        #region Управление заказами

        /// <summary>
        /// Список всех заказов (с пользователями и товарами).
        /// </summary>
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        /// <summary>
        /// Детали конкретного заказа.
        /// </summary>
        /// <param name="id">ID заказа</param>
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        /// <summary>
        /// Обновление статуса заказа.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Статус заказа успешно обновлён!";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        #endregion

        #region CRUD для административных сущностей

        /// <summary>
        /// Главная страница админки.
        /// </summary>
        public IActionResult Index() => View();

        /// <summary>
        /// Подробности записи (например, для любой сущности по ID).
        /// </summary>
        public IActionResult Details(int id) => View();

        /// <summary>
        /// Форма создания.
        /// </summary>
        public IActionResult Create() => View();

        /// <summary>
        /// Обработка создания.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection form)
        {
            if (!ModelState.IsValid)
                return View();

            // TODO: добавить логику сохранения

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Форма редактирования.
        /// </summary>
        public IActionResult Edit(int id) => View();

        /// <summary>
        /// Обработка редактирования.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection form)
        {
            if (!ModelState.IsValid)
                return View();

            // TODO: добавить логику обновления

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Форма удаления.
        /// </summary>
        public IActionResult Delete(int id) => View();

        /// <summary>
        /// Обработка удаления.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // TODO: добавить логику удаления

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}
