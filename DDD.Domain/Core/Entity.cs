namespace DDD.Domain.Core
{
    public class Entity<T>
    {
        public T Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
    }
}
