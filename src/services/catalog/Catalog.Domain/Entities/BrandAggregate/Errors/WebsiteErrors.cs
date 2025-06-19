using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace Catalog.Domain.Entities.BrandAggregate.Errors
{
    /// <summary>
    /// Contains validation errors related to website properties.
    /// </summary>
    public static class WebsiteErrors
    {
        /// <summary>
        /// Error indicating that the website field is empty.
        /// </summary>
        public static Error Empty => Error.Validation(
            code: "Website.Empty",
            description: "Website cannot be empty.");

        /// <summary>
        /// Error indicating that the website URL is invalid.
        /// </summary>
        public static Error Invalid => Error.Validation(
            code: "Website.Invalid",
            description: "Website must be a valid URL starting with http:// or https://.");
    }
}
