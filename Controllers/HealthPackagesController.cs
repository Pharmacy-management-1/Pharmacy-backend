using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Models.DTOs;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthPackagesController : ControllerBase
    {
        private readonly IHealthPackageService _healthPackageService;

        public HealthPackagesController(IHealthPackageService healthPackageService)
        {
            _healthPackageService = healthPackageService;
        }

        /// <summary>Get all active health packages (public).</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var packages = await _healthPackageService.GetAllAsync();
            return Ok(packages);
        }

        /// <summary>Get a health package by id (public).</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pkg = await _healthPackageService.GetByIdAsync(id);
            if (pkg == null) return NotFound(new { message = "Health package not found." });
            return Ok(pkg);
        }

        /// <summary>Create a new health package (Admin only).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateHealthPackageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var pkg = await _healthPackageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = pkg.Id }, pkg);
        }

        /// <summary>Update a health package (Admin only).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHealthPackageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var pkg = await _healthPackageService.UpdateAsync(id, dto);
            if (pkg == null) return NotFound(new { message = "Health package not found." });
            return Ok(pkg);
        }

        /// <summary>Soft-delete a health package (Admin only).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _healthPackageService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Health package not found." });
            return NoContent();
        }
    }
}