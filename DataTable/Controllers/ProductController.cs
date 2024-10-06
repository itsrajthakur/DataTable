using DataTable.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace DataTable.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;

        public ProductController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = HttpContext.Request.Form["search[value]"].FirstOrDefault();

                // Retrieve the custom search values for ProductName and Category only
                var productName = HttpContext.Request.Form["columns[1][search][value]"].FirstOrDefault();
                var category = HttpContext.Request.Form["columns[3][search][value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var productsQuery = context.Products.AsQueryable();

                // Apply sorting if needed
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    productsQuery = productsQuery.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                // Apply global search if needed
                if (!string.IsNullOrEmpty(searchValue))
                {
                    productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchValue) ||
                                                             p.Brand.Contains(searchValue) ||
                                                             p.Category.Contains(searchValue));
                }

                // Apply individual column search filters for ProductName and Category
                if (!string.IsNullOrEmpty(productName))
                {
                    productsQuery = productsQuery.Where(p => p.ProductName.Contains(productName));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    productsQuery = productsQuery.Where(p => p.Category.Contains(category));
                }

                // Get total number of records (before pagination and filter)
                int recordsTotal = productsQuery.Count();

                // Pagination
                var data = await productsQuery.Skip(skip).Take(pageSize).ToListAsync();

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                };

                return new JsonResult(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
                return RedirectToAction("Index"); // Redirect to the Index after successful creation
            }
            return View(product); // Return the same view with the product data in case of errors
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            try
            {
                // Check if IDs are passed
                if (ids == null || ids.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No products selected for deletion." });
                }

                var products = context.Products.Where(p => ids.Contains(p.Id)).ToList();

                if (products.Count == 0)
                {
                    return NotFound(new { success = false, message = "Products not found." });
                }

                // Remove the products from the database
                context.Products.RemoveRange(products);
                await context.SaveChangesAsync();

                return Ok(new { success = true, message = "Products successfully deleted." });
            }
            catch (Exception ex)
            {
                // Log error and return error response
                return StatusCode(500, new { success = false, message = "An error occurred while deleting products." });
            }
        }

    }
}
