using System.Net;

namespace Teck.Shop.SharedKernel.Core.Exceptions
{
    /// <summary>
    /// The custom exception.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CustomException"/> class.
    /// </remarks>
    /// <param name="message">The message.</param>
    /// <param name="statusCode">The status code.</param>
    public class CustomException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : Exception(message)
    {
        /// <summary>
        /// Gets the status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}
