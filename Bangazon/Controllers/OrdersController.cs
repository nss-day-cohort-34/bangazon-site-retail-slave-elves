using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Bangazon.Models.OrderViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Bangazon.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Order.Include(o => o.PaymentType).Include(o => o.User);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> DeleteOrderProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderProduct = await _context.OrderProduct
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderProductId == id);
            if (orderProduct == null)
            {
                return NotFound();
            }

            return View(orderProduct);
        }

        // POST: OrderProducts/Delete/5
        [HttpPost, ActionName("DeleteOrderProduct")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteODConfirmed(int id)
        {
            var orderProduct = await _context.OrderProduct.FindAsync(id);
            _context.OrderProduct.Remove(orderProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewCart", "Orders", new { id = orderProduct.OrderId });
        }





        // GET: Orders/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = await _context.Order
               .Include(o => o.PaymentType)
               .Include(o => o.User)
               .Include(o => o.OrderProducts)
               .ThenInclude(op => op.Product)
               .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderProducts = await _context.OrderProduct.Where(op => op.OrderId == id).ToListAsync();
            foreach (var item in orderProducts)
            {
                _context.OrderProduct.Remove(item);
            }
            await _context.SaveChangesAsync();
            var order = await _context.Order.FindAsync(id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ViewCart));
        }
        [Authorize]
        public async Task<IActionResult> ViewCart(int id)
        {

            var viewModel = new OrderDetailViewModel();

            var currentUser = await GetCurrentUserAsync();
            Order order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .ThenInclude(U => U.PaymentTypes)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(m => m.UserId == currentUser.Id.ToString() && m.PaymentTypeId == null);

            viewModel.Order = order;

            if (order == null || order.PaymentType != null)
            {
                return View(viewModel);
            }
            viewModel.LineItems = order.OrderProducts
                .GroupBy(op => op.Product)
                .Select(g => new OrderLineItem
                {
                    Product = g.Key,
                    Units = g.Select(x => x.Product).Count(),
                    Cost = g.Key.Price * g.Select(x => x.ProductId).Count()
                }).ToList();
            return View(viewModel);
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }


        public async Task<IActionResult> AddToOrder(int Id)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                List<Order> activeOrders = _context.Order.Where(o => o.UserId == user.Id && o.PaymentType == null).ToList();
                
                
                if (!activeOrders.Any())
                {
                    _context.Add(new Order { UserId = user.Id });
                    await _context.SaveChangesAsync();
                    List<Order> newActiveOrder = _context.Order.Where(o => o.UserId == user.Id && o.PaymentType == null).ToList();
                    Order currentOrder = newActiveOrder[0];
                    _context.OrderProduct.Add(new OrderProduct { ProductId = Id, OrderId = currentOrder.OrderId});
                }
                else
                {
                    Order currentOrder = activeOrders[0];
                    _context.OrderProduct.Add(new OrderProduct { ProductId = Id, OrderId = currentOrder.OrderId});
                }


                  
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Details", "Products");
        }


        // GET: Orders/CompletePayment/2
        public async Task<IActionResult> CloseOrder(int id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var order =_context.Order.FirstOrDefault(o => o.OrderId == id);
            var paymentTypes = _context.PaymentType.Where(p => p.UserId == user.Id).ToList();
            var viewModel = new OrderCloseViewModel();
            viewModel.PaymentTypes = paymentTypes.Select(a => new SelectListItem
            {
                Value = a.PaymentTypeId.ToString(),
                Text = a.AccountNumber
            }).ToList();
            viewModel.Order = order;

            return View(viewModel);

        }
        [HttpPost]
        public async Task<IActionResult> CloseOrder(int id, OrderCloseViewModel viewModel)
        {
            var currentOrder = _context.Order.Include(o => o.OrderProducts).FirstOrDefault(o => o.OrderId == id);

            var OrderProducts = _context.OrderProduct.Include(o => o.Product).Where(o => o.OrderId == id).ToList();

            currentOrder.DateCompleted = DateTime.Now;
            currentOrder.PaymentTypeId = viewModel.Order.PaymentTypeId;

//remove from quantity and update DB
            foreach (var item in OrderProducts)
            {
                item.Product.Quantity = item.Product.Quantity - 1;
                _context.Update(item.Product);
            }

            ModelState.Remove("Order.UserId");
            ModelState.Remove("Order.User");
            if (ModelState.IsValid)
            {
                _context.Update(currentOrder);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }



            return View(viewModel);
        }


    }
}
