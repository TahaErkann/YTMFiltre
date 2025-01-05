using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YTM.Core.Entities;
using YTM.Core.Services;
using YTM.Core.Models;
using YTM.Core.Iterators;

namespace YTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("Getting all products");
                var products = await _productService.GetAllProductsAsync();
                
                if (!products.Any())
                {
                    _logger.LogInformation("No products found");
                    return Ok(new List<Product>()); // Boş liste dön
                }
                
                _logger.LogInformation($"Found {products.Count()} products");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting products: {ex}");
                return StatusCode(500, new { message = "Ürünler yüklenirken bir hata oluştu", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Ürün bulunamadı" });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product: {ex}");
                return StatusCode(500, new { message = "Ürün yüklenirken bir hata oluştu", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new product: {@request}", request);

                // Kategori validasyonu
                if (!ProductCategories.GetAllCategories().Contains(request.Category))
                {
                    return BadRequest(new { message = "Geçersiz kategori" });
                }

                // Numara validasyonu
                foreach (var size in request.Sizes)
                {
                    if (size.Size < 30 || size.Size > 45)
                    {
                        return BadRequest(new { message = "Numara 30-45 aralığında olmalıdır" });
                    }
                }

                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Brand = request.Brand,
                    ImageUrl = request.ImageUrl,
                    Category = request.Category,
                    Sizes = request.Sizes,
                    IsActive = true
                };

                await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating product: {ex}");
                return StatusCode(500, new { message = "Ürün oluşturulurken bir hata oluştu", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest(new { message = "ID uyuşmazlığı" });
                }

                await _productService.UpdateProductAsync(id, product);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex}");
                return StatusCode(500, new { message = "Ürün güncellenirken bir hata oluştu", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex}");
                return StatusCode(500, new { message = "Ürün silinirken bir hata oluştu", error = ex.Message });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts([FromQuery] string brand = "", [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null)
        {
            try
            {
                var allProducts = await _productService.GetAllProductsAsync();
                var iterator = new FilteredProductIterator(allProducts, brand, minPrice, maxPrice);
                
                var filteredProducts = new List<Product>();
                while (iterator.HasNext())
                {
                    filteredProducts.Add(iterator.Next());
                }
                
                return Ok(filteredProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ürünler filtrelenirken hata oluştu: {ex.Message}");
                return StatusCode(500, "Ürünler filtrelenirken bir hata oluştu");
            }
        }
    }
} 