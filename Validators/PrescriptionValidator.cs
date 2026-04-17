using PharmacyApi.Models.Domain;
using PharmacyApi.Services; // IProductService (Team B)

namespace PharmacyApi.Validators;

public class PrescriptionValidator
{
    private readonly IProductService _productService;

    public PrescriptionValidator(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<bool> ValidateOrderPrescriptionsAsync(int userId, int? prescriptionId, List<int> productIds)
    {
        // If no product requires prescription, prescription is optional
        bool requiresPrescription = await _productService.AnyProductRequiresPrescription(productIds);
        if (!requiresPrescription)
            return true; // No prescription needed

        // If prescription is required but not provided
        if (prescriptionId == null)
            return false;

        // Check that prescription exists, belongs to user, and is approved
        // This will be done in the service with repository access
        return true; // Actual check will be inside OrderService
    }
}