using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;

namespace SportsStore.WebUI.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private IProductRepository repository;

        public AdminController(IProductRepository repo)
        {
            repository = repo;
        }

        // GET: Admin
        public ViewResult Index()
        {
            return View(repository.Products);
        }

        public ViewResult Edit(int productID)
        {
            Product product = repository.Products.FirstOrDefault(p => p.ProductID == productID);
            return View(product);
        }

        [HttpPost]
        public ActionResult Edit(Product product, HttpPostedFileBase image = null)
        {
            if (ModelState.IsValid)
            {
                if(image != null)
                {
                    // There is an image to save to the database
                    product.ImageMimeType = image.ContentType;
                    product.ImageData = new byte[image.ContentLength];
                    image.InputStream.Read(product.ImageData, 0, image.ContentLength);
                }
                repository.SaveProduct(product);
                TempData["message"] = string.Format("{0} has been saved", product.Name);
                return RedirectToAction("Index");
            } else
            {
                // There's something wrong with the data values. Go back to the Edit view
                return View(product);
            }
        }

        public ViewResult Create()
        {
            return View("Edit", new Product());
        }

        [HttpPost]
        public ActionResult Delete(int productID)
        {
            Product deletedProduct = repository.DeleteProduct(productID);
            if (deletedProduct != null)
            {
                TempData["message"] = string.Format("{0} was deleted", deletedProduct.Name);

            }
            return RedirectToAction("Index");
        }

        public PartialViewResult LoadEditPartial(int productId)
        {
            Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);
            return PartialView("_Edit", product);
        }

        public PartialViewResult LoadProductAdminListPartial()
        {
            return PartialView("_ProductAdminList", repository.Products);
        }

        [HttpPost]
        public JsonResult EditAsync(Product product)
        {
            repository.SaveProduct(product);
            return Json(new { Success = true });
        }
    }
}