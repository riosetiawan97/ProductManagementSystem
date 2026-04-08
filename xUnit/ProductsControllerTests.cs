using Moq;
using Xunit;
using Backend.Controllers;
using Library.Entities;
using Library.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _cacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<ProductsController>>();

            _controller = new ProductsController(_repoMock.Object, _cacheMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<Product> { new Product { Id = 1, Name = "Laptop" } };
            _repoMock.Setup(r => r.GetAllAsync(null, null, null)).ReturnsAsync(products);

            // Mocking IMemoryCache.TryGetValue
            object? outEntry = null;
            _cacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out outEntry)).Returns(false);
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _controller.GetAll(null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(products, okResult.Value);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenValid()
        {
            // Arrange
            var newProduct = new Product { Id = 2, Name = "Produk Baru", Price = 100 };
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(newProduct);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(2, ((Product)createdResult.Value!).Id);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            int productId = 1;
            _repoMock.Setup(r => r.DeleteAsync(productId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _repoMock.Verify(r => r.DeleteAsync(productId), Times.Once);
        }
    }
}
