using System.Net;

namespace Netrin.Position.Domain.Model.Auxiliar
{
    public class DefaultResult<T>
    {
        public DefaultResult(bool succeded, HttpStatusCode statusCode, T? data = default, string? message = null)
        {
            Succeded = succeded;
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }

        public bool Succeded { get; }
        public HttpStatusCode StatusCode { get; }
        public string? Message { get; }
        public T? Data { get; }
    }
}