using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teck.Shop.SharedKernel.Infrastructure.OpenApi
{
    /// <summary>
    /// Extension to hold a list of openapi documents.
    /// </summary>
    public static class OpenApiDocumentRegistry
    {
        private static readonly List<(string Name, string Url)> _documents = new();

        /// <summary>
        /// Add document to list.
        /// </summary>
        public static void Add(string name, string url) =>
            _documents.Add((name, url));

        /// <summary>
        /// ReadOnly list of openapi documents.
        /// </summary>
        public static IReadOnlyList<(string Name, string Url)> GetAll() =>
            _documents.AsReadOnly();
    }
}
