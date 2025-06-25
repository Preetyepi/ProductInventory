using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductInventory.Controllers;
using ProductInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestProject
{
    public class ProductTest
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid()) // Ensures unique DB per test run
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsView_WithProductList()
        {
            var context = GetDbContext();
            context.Products.AddRange(
                new Product { Id = 1, Name = "Product A", Price = 10 },
                new Product { Id = 2, Name = "Product B", Price = 20 }
            );
            context.SaveChanges();

            var controller = new ProductsController(context);
            var result = await controller.Index() as ViewResult;

            var model = result?.Model as List<Product>;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(2, model.Count);
        }
        [Fact]
        public async Task Create_Post_ValidProduct_RedirectsToIndex()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            using var context = new ApplicationDbContext(options);
            var controller = new ProductsController(context);

            var product = new Product
            {
                Name = "Test Product",
                Category = "Test",
                Price = 100,
                Quantity = 10
            };

            controller.ModelState.Clear(); // simulate valid input

            // Act
            var result = await controller.Create(product);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var products = context.Products.ToList();
            Assert.Single(products); // should now pass
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);
            controller.ModelState.AddModelError("Name", "Required");

            var product = new Product { Price = 10 };

            var result = await controller.Create(product) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewWithProduct()
        {
            var context = GetDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Product 1", Price = 100 });
            context.SaveChanges();

            var controller = new ProductsController(context);
            var result = await controller.Edit(1) as ViewResult;

            var model = result?.Model as Product;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Product 1", model.Name);
        }

        [Fact]
        public async Task Edit_Post_ValidProduct_RedirectsToIndex()
        {
            var context = GetDbContext();
            var product = new Product { Id = 1, Name = "Old", Price = 20 };
            context.Products.Add(product);
            context.SaveChanges();

            var controller = new ProductsController(context);

            var updated = new Product { Id = 1, Name = "Updated", Price = 30 };
            var result = await controller.Edit(updated) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var updatedProduct = context.Products.Find(1);
            Assert.Equal("Updated", updatedProduct.Name);
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsSuccessJson()
        {
            var context = GetDbContext();
            context.Products.Add(new Product { Id = 1, Name = "To Delete", Price = 20 });
            context.SaveChanges();

            var controller = new ProductsController(context);
            var result = await controller.Delete(1) as JsonResult;

            dynamic data = result?.Value;
            Assert.True(data.success);
            Assert.Empty(context.Products.ToList());
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsErrorJson()
        {
            var context = GetDbContext();
            var controller = new ProductsController(context);

            var result = await controller.Delete(999) as JsonResult;

            dynamic data = result?.Value;
            Assert.False(data.success);
            Assert.Equal("Product not found", (string)data.message);
        }
    }
}