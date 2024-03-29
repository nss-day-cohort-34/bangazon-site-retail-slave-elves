﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Bangazon.Models.ProductViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Bangazon.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Types()
        {
       
            var model = new ProductTypesViewModel();
            
            model.GroupedProducts = await _context
            .ProductType
            .Select(pt => new GroupedProducts
                {
                    TypeId = pt.ProductTypeId,
                    TypeName = pt.Label,
                    ProductCount = pt.Products.Where(p => p.Active == true).Count(),
                    Products = pt.Products.OrderByDescending(p => p.DateCreated).Where(p => p.Active == true).Take(3)
                }).ToListAsync();

            return View(model);
        }

        // GET: Products
        public async Task<IActionResult> Index(string q)
        {
            if (q == null)
            {
                var applicationDbContext = _context.Product.Include(p => p.ProductType).Include(p => p.User).Where(p => p.Active == true);
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                var applicationDbContext = _context.Product.Include(p => p.ProductType).Include(p => p.User).Where(p => p.Title.Contains(q) || p.City.Contains(q)).Where(p => p.Active == true);
                return View(await applicationDbContext.ToListAsync());
            }
        }
        
        public async Task<IActionResult> GetMyProducts()
        {
            var user = await GetCurrentUserAsync();

            List<Product> userProducts = _context.Product.Include(p => p.ProductType).Include(p => p.OrderProducts).Include(p => p.User).Where(p => p.UserId == user.Id).ToList();
            List<int> userProductIds = userProducts.Select(p => p.ProductId).ToList();

            List<OrderProduct> orderProducts = _context.OrderProduct.ToList();
            List<Order> orders = _context.Order.Include(o => o.OrderProducts).ToList();
            List<Order> completedOrders = orders.Where(o => o.PaymentTypeId != null).ToList();

            //List<OrderProduct> matchingOrderProducts = new List<OrderProduct>();

            //foreach (var op in orderProducts)
            //{
            //    foreach (var p in userProductIds)
            //    {
            //        foreach (var o in completedOrders)
            //        {
            //            foreach (var oop in o.OrderProducts)
            //            {
            //                if (p == op.ProductId && oop == op)
            //                {
            //                    matchingOrderProducts.Add(op);
            //                }

            //            }

            //        }
            //    }
            //}

            //List<OrderProduct> productOrderProducts = new List<OrderProduct>();

            //foreach (var p in userProducts)
            //{
            //    foreach (var op in matchingOrderProducts)
            //    {
            //        if (op.ProductId == p.ProductId)
            //        {
            //            productOrderProducts.Add(op);
            //        }
            //    }
            //}

            

            var applicationDbContext = _context.Product.Include(p => p.ProductType).Include(p => p.OrderProducts).Include(p => p.User).Where(p => p.UserId == user.Id);
            return View(await applicationDbContext.ToListAsync());
        }



        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Types/Details/5
        public async Task<IActionResult> ProductTypeDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = await _context.ProductType
                .Include(p => p.Products)
                .FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        // GET: Products/Create
        [Authorize]
        public async Task<IActionResult> CreateAsync()
        {
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            List<ProductType> categories = await _context.ProductType.ToListAsync();
          

            var updated = categories.Select(c => new SelectListItem(c.Label, c.ProductTypeId.ToString())).ToList();
            updated.Insert(0, new SelectListItem("Select a category", null));


            var viewModel = new ProductCreateViewModel
            {
                productTypes = updated
            };
    

               
            
          
            return View(viewModel);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel viewModel )
        {
            ModelState.Remove("Product.User");
            ModelState.Remove("Product.UserId");
            ModelState.Remove("Product.ProductType");


            var product = viewModel.product;

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                product.UserId = currentUser.Id;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            List<ProductType> categories = await _context.ProductType.ToListAsync();


            var updated = categories.Select(c => new SelectListItem(c.Label, c.ProductTypeId.ToString())).ToList();
            updated.Insert(0, new SelectListItem("Select a category", null));


            viewModel.productTypes = updated;





            return View(viewModel);
        }
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,DateCreated,Description,Title,Price,Quantity,UserId,City,ImagePath,Active,ProductTypeId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            List<Order> orders = _context.Order.Include(o => o.OrderProducts).ToList();
            List<OrderProduct> toBeDeleted = new List<OrderProduct>();
            foreach (var o in orders)
            {
                if (o.PaymentTypeId == null)
                {
                    foreach (var op in o.OrderProducts)
                    {
                        if (op.ProductId == id)
                        {
                            toBeDeleted.Add(op);
                        }
                    }
                }
            }

            foreach (var op in toBeDeleted)
            {
                _context.OrderProduct.Remove(op);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Product.FindAsync(id);
            product.Active = false;
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(GetMyProducts));
        }

        public async Task<IActionResult> MakeActive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("MakeActive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeActive(int id)
        {
            var product = await _context.Product.FindAsync(id);
            product.Active = true;
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(GetMyProducts));
        }


        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }

    }
}
