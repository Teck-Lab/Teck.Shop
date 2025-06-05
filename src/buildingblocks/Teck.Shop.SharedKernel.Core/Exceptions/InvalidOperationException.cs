using System.Net;

namespace Teck.Shop.SharedKernel.Core.Exceptions
{
    /// <summary>
    /// The invalid operation exception.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="InvalidOperationException"/> class.
    /// </remarks>
    /// <param name="message">The message.</param>
    public class InvalidOperationException(string message) : CustomException(message, HttpStatusCode.InternalServerError)
    {
    }
}
