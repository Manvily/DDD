namespace DDD.Application.Cache;

public static class CacheKeys
{
    public const string CustomersList = "CustomersList";
    public const string ProductsList = "ProductsList";
    
    public static string CustomerOrders(Guid customerId) => $"CustomerOrders:{customerId}";
}