using System.Net;

namespace DDD.Application.Exceptions;

public class NotExistsException : Exception
{
    public HttpStatusCode Code { get; set; }
    public Guid[] Ids { get; set; }
    public NotExistsException(string message) : base(message)
    {
        Code = HttpStatusCode.NotFound;
    }
}