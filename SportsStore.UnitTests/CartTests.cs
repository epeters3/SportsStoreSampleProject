﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportsStore.Domain.Entities;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.WebUI.Controllers;
using System.Web.Mvc;
using SportsStore.WebUI.Models;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            // Arrange - Create Test Products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            CartLine[] results = target.Lines.ToArray();

            //Assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Product, p1);
            Assert.AreEqual(results[1].Product, p2);
        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            // Arrange - Create Test Products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);
            CartLine[] results = target.Lines.OrderBy(c => c.Product.ProductID).ToArray();

            //Assert
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Quantity, 11);
            Assert.AreEqual(results[1].Quantity, 1);
        }

        [TestMethod]
        public void Can_Remove_Line()
        {
            // Arrange - Create Test Products
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Arrange - Add some products to the cart
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            //Act
            target.RemoveLine(p2);

            //Assert
            Assert.AreEqual(target.Lines.Where(c => c.Product == p2).Count(), 0);
            Assert.AreEqual(target.Lines.Count(), 2);
        }

        [TestMethod]
        public void Calculate_Cart_Total()
        {
            // Arrange - Create Test Products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Act
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal result = target.ComputeTotalValue();

            // Assert
            Assert.AreEqual(result, 450M);
        }

        [TestMethod]
        public void Can_Clear_Contents()
        {
            // Arrange - Create Test Products
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            // Arrange - Create a new cart
            Cart target = new Cart();

            // Arrange - Add some items
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);

            // Act - Reset the cart
            target.Clear();

            // Assert
            Assert.AreEqual(target.Lines.Count(), 0);
        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            // Arrange - Create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name = "P1", Category = "Apples" },
            }.AsQueryable());

            // Arrange - Create a new cart
            Cart cart = new Cart();

            // Arrange - Create the controller
            CartController target = new CartController(mock.Object, null);

            // Act - Add a product to the cart
            target.AddToCart(cart, 1, null);

            //Assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            // Arrange - Create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name = "P1", Category = "Apples" },
            }.AsQueryable());

            // Arrange - Create a new cart
            Cart cart = new Cart();

            // Arrange - Create the controller
            CartController target = new CartController(mock.Object, null);

            // Act - Add a product to the cart
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            // Assert
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            // Arrange - Create a new cart
            Cart cart = new Cart();

            // Arrange - Create the controller
            CartController target = new CartController(null, null);

            // Act - Call the index method
            CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            // Assert
            Assert.AreEqual(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            // Arrange - Create the mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            // Arrange - Create a new cart
            Cart cart = new Cart();
            // Arrange - create shipping details
            ShippingDetails shippingDetails = new ShippingDetails();
            // Arrange - create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            // Act
            ViewResult result = target.Checkout(cart, shippingDetails);

            //Assert - check that the order hasn't been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());

            //Assert - Check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);

            // Assert - Check that I am passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            // Arrange - Create the mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // Arrange - Create a new cart
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            // Arrange - create an instance of the controller
            CartController target = new CartController(null, mock.Object);
            // Arrange - Add an error to the model
            target.ModelState.AddModelError("error", "error");

            // Act - try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            //Assert - check that the order hasn't been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());

            //Assert - Check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);

            // Assert - Check that I am passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Can_Checkout_And_Submit_Order()
        {
            // Arrange - Create the mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // Arrange - Create a new cart
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            // Arrange - create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            // Act - try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            //Assert - check that the order has been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once());

            //Assert - Check that the method is returning the Completed view
            Assert.AreEqual("Completed", result.ViewName);

            // Assert - Check that I am passing a valid model to the view
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }


    }
}
