using Library.Entities;
using Library.Repositories;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Controllers
{
    [Authorize] // Proteksi endpoint
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductsController> _logger;
        private const string ProductCacheKey = "ProductList";

        public ProductsController(IProductRepository repository, IMemoryCache cache, ILogger<ProductsController> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            _logger.LogInformation("Fetching products");

            if (!_cache.TryGetValue(ProductCacheKey, out IEnumerable<Product> products))
            {
                // Panggil repository secara async
                products = await _repository.GetAllAsync(name, minPrice, maxPrice);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(ProductCacheKey, products, cacheOptions);
            }

            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState); // Data Validation

            await _repository.AddAsync(product);
            _cache.Remove(ProductCacheKey); // Invalidate cache agar data selalu fresh

            return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id) return BadRequest();

            await _repository.UpdateAsync(product);
            _cache.Remove(ProductCacheKey); // Invalidate cache
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            _cache.Remove(ProductCacheKey); // Invalidate cache
            return NoContent();
        }
    }
}
