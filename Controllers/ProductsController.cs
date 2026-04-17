using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.DTOs;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new product with inventory
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDTO productDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(productDto);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { error = "An error occurred while creating the product" });
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDTO productDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, productDto);
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the product" });
            }
        }

        /// <summary>
        /// Delete (soft delete) a product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound(new { error = $"Product with ID {id} not found" });

            return NoContent();
        }

        /// <summary>
        /// Get product by ID with inventory details
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { error = $"Product with ID {id} not found" });

            return Ok(product);
        }

        /// <summary>
        /// Get all products with filtering, sorting, and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDTO filter)
        {
            var (products, totalCount) = await _productService.GetProductsAsync(filter);

            return Ok(new
            {
                data = products,
                totalCount,
                page = filter.Page,
                pageSize = filter.PageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            });
        }

        /// <summary>
        /// Update product inventory (add or remove stock)
        /// </summary>
        [HttpPatch("{id}/inventory")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] InventoryUpdateDTO inventoryUpdate)
        {
            if (id != inventoryUpdate.ProductId)
                return BadRequest(new { error = "Product ID mismatch" });

            try
            {
                var result = await _productService.UpdateInventoryAsync(inventoryUpdate);
                return Ok(new { message = "Inventory updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory for product {ProductId}", id);
                return StatusCode(500, new { error = "An error occurred while updating inventory" });
            }
        }

        /// <summary>
        /// Get inventory details for a specific product
        /// </summary>
        [HttpGet("{id}/inventory")]
        public async Task<IActionResult> GetInventory(int id)
        {
            var inventory = await _productService.GetInventoryByProductIdAsync(id);
            if (inventory == null)
                return NotFound(new { error = $"Inventory for product ID {id} not found" });

            return Ok(inventory);
        }

        /// <summary>
        /// Get all products with low stock
        /// </summary>
        [HttpGet("inventory/low-stock")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            var products = await _productService.GetLowStockProductsAsync(threshold);
            return Ok(products);
        }

        /// <summary>
        /// Reserve stock for an order
        /// </summary>
        [HttpPost("{id}/inventory/reserve")]
        public async Task<IActionResult> ReserveStock(int id, [FromBody] ReserveStockRequest request)
        {
            try
            {
                var result = await _productService.ReserveStockAsync(id, request.Quantity);
                return Ok(new { message = $"Successfully reserved {request.Quantity} units" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Release reserved stock
        /// </summary>
        [HttpPost("{id}/inventory/release")]
        public async Task<IActionResult> ReleaseReservedStock(int id, [FromBody] ReleaseStockRequest request)
        {
            try
            {
                var result = await _productService.ReleaseReservedStockAsync(id, request.Quantity);
                return Ok(new { message = $"Successfully released {request.Quantity} units" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class ReserveStockRequest
    {
        public int Quantity { get; set; }
    }

    public class ReleaseStockRequest
    {
        public int Quantity { get; set; }
    }
