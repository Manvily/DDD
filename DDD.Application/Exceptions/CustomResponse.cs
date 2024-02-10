namespace DDD.Application.Exceptions;

public class CustomResponse
{
    public int Code { get; set; }

    public string Message { get; set; }
    public string Status { get; set; }
    public string? Details { get; set; }
    public Guid[]? Ids { get; set; }
}