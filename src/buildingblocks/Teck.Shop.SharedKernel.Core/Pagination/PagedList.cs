namespace Teck.Shop.SharedKernel.Core.Pagination
{
    /// <summary>
    /// Represents a paginated list of items with metadata about paging.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// Gets the items on the current page.
        /// </summary>
        public IList<T> Items { get; }

        /// <summary>
        /// Gets the current page number (1-based).
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// Gets the size of the page (number of items per page).
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Gets the total number of items across all pages.
        /// </summary>
        public int TotalItems { get; }

        /// <summary>
        /// Gets a value indicating whether this is the first page.
        /// </summary>
        public bool IsFirstPage => Page == 1;

        /// <summary>
        /// Gets a value indicating whether this is the last page.
        /// </summary>
        public bool IsLastPage => Page == TotalPages && TotalPages > 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="Items">The items on the current page.</param>
        /// <param name="TotalItems">The total number of items in the full dataset.</param>
        /// <param name="Page">The current page number (1-based).</param>
        /// <param name="Size">The number of items per page.</param>
        public PagedList(IEnumerable<T> Items, int TotalItems, int Page, int Size)
        {
            this.Page = Page;
            this.Size = Size;
            this.TotalItems = TotalItems;

            if (TotalItems > 0)
            {
                TotalPages = (int)Math.Ceiling(TotalItems / (double)Size);
            }

            this.Items = Items as IList<T> ?? new List<T>(Items);
        }
    }
}
