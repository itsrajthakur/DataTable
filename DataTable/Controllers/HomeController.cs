using DataTable.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Dynamic.Core;

namespace DataTable.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
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

        /*   [HttpPost]
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

                   // Retrieve the custom search values for productName and category
                   var productName = HttpContext.Request.Form["productName"].FirstOrDefault();
                   var category = HttpContext.Request.Form["category"].FirstOrDefault();

                   int pageSize = length != null ? Convert.ToInt32(length) : 0;
                   int skip = start != null ? Convert.ToInt32(start) : 0;

                   var products = context.Products.AsQueryable();

                   // Apply sorting if needed
                   if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                   {
                       products = products.OrderBy(sortColumn + " " + sortColumnDirection);
                   }

                   // Apply global search if needed
                   if (!string.IsNullOrEmpty(searchValue))
                   {
                       products = products.Where(p => p.ProductName.Contains(searchValue) ||
                                                      p.Brand.Contains(searchValue) ||
                                                      p.Category.Contains(searchValue));
                   }

                   // Apply additional custom filters for productName and category
                   if (!string.IsNullOrEmpty(productName))
                   {
                       products = products.Where(p => p.ProductName.Contains(productName));
                   }

                   if (!string.IsNullOrEmpty(category))
                   {
                       products = products.Where(p => p.Category.Contains(category));
                   }

                   // Get total number of records
                   int recordsTotal = products.Count();

                   // Pagination
                   var data = products.Skip(skip).Take(pageSize).ToList();

                   var jsonData = new
                   {
                       draw = draw,
                       recordsFiltered = recordsTotal,
                       recordsTotal = recordsTotal,
                       data = data
                   };

                   return new JsonResult(jsonData);
               }
               catch (Exception)
               {
                   throw;
               }
           }*/

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
                // Log exception if needed
                _logger.LogError(ex, "Error occurred while fetching data.");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found." });
                }

                context.Products.Remove(product);
                await context.SaveChangesAsync();

                return Ok(new { success = true, message = "Product successfully deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product.");
                return BadRequest(new { success = false, message = "Error occurred while deleting product." });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
