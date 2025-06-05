using System.ComponentModel.DataAnnotations;
using Teck.Shop.SharedKernel.Core.Options;

namespace Teck.Shop.SharedKernel.Infrastructure.Options
{
    /// <summary>
    /// The app options.
    /// </summary>
    public class AppOptions : IOptionsRoot
    {
        /// <summary>
        /// Appsettings name.
        /// </summary>
        public const string Section = "App";

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; } = "Teck.Shop.WebAPI";
    }
}
