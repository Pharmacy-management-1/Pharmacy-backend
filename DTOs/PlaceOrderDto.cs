namespace PharmacyApi.DTOs;

public class PlaceOrderDto
{
    public int? PrescriptionId { get; set; }          // required if any product needs prescription
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}