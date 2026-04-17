using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Models.DTOs;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        /// <summary>Get all active offers (public).</summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOffers()
        {
            var offers = await _offerService.GetActiveOffersAsync();
            return Ok(offers);
        }

        /// <summary>Get all offers (Admin).</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var offers = await _offerService.GetAllOffersAsync();
            return Ok(offers);
        }

        /// <summary>Get a single offer by id (public).</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var offer = await _offerService.GetOfferByIdAsync(id);
            if (offer == null) return NotFound(new { message = "Offer not found." });
            return Ok(offer);
        }

        /// <summary>Create a seasonal offer (Admin only).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSeasonalOfferDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var offer = await _offerService.CreateOfferAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = offer.Id }, offer);
        }

        /// <summary>Update a seasonal offer (Admin only).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSeasonalOfferDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var offer = await _offerService.UpdateOfferAsync(id, dto);
            if (offer == null) return NotFound(new { message = "Offer not found." });
            return Ok(offer);
        }

        /// <summary>Soft-delete an offer (Admin only).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _offerService.DeleteOfferAsync(id);
            if (!success) return NotFound(new { message = "Offer not found." });
            return NoContent();
        }
    }
}