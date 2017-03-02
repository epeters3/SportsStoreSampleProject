using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Models;

namespace SportsStore.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private IProductRepository repository;
        public int PageSize = 4;

        // GET: Product
        public ProductController(IProductRepository productRepository)
        {
            this.repository = productRepository;
        }

        public ViewResult List(string category, int page = 1)
        {
            // repository.Products gets all products from the Products table
            // via repository, our IProductRepository.
            // The Skip() method here skips over the products that occur before
            // the start of the current page. Its a part of our pagination.
            ProductsListViewModel model = new ProductsListViewModel
            {
                Products = repository.Products
                // Filter by the product Category that the user selected, or show all categories.
                .Where(p => category == null || p.Category == category)
                .OrderBy(p => p.ProductID)
                // Only show the products for the current page
                .Skip((page - 1) * PageSize)
                .Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = category == null ?
                        repository.Products.Count() :
                        repository.Products.Where(e => e.Category == category).Count()
                },
                CurrentCategory = category
            };
            return View(model);
        }

        public PartialViewResult ProductGrid(string query, string category, int page = 1)
        {
            string processedQuery = query.Trim().ToLower();

            ProductsListViewModel model = new ProductsListViewModel
            {
                Products = repository.Products
                    .Where(p => String.IsNullOrWhiteSpace(category) || p.Category == category)
                    .Where(p => p.Name.ToLower().Contains(processedQuery)
                        || p.Category.ToLower().Contains(processedQuery)
                        || p.Description.ToLower().Contains(processedQuery))
                    .OrderBy(p => p.ProductID),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = category == null ?
                        repository.Products.Count() :
                        repository.Products.Where(e => e.Category == category).Count()
                },
                CurrentCategory = category
            };
            return PartialView("ProductGrid", model);
        }

        public FileContentResult GetImage(int productId)
        {
            Product prod = repository.Products.FirstOrDefault(p => p.ProductID == productId);
            if (prod != null)
            {
                return File(prod.ImageData, prod.ImageMimeType);
            } else
            {
                return null;
            }
        }
    }
}