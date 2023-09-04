using DDD.Application.Queries.Products;

namespace DDD.Application.Queries.Orders;

public class OrderViewModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public IEnumerable<ProductViewModel> Products { get; set; }
    public bool IsPaid { get; set; }
}