using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.RepositoryContracts
{
    public interface ITagsRepository
    {
        /// <summary>
        /// Fetches a list of tags from an external API and saves them to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task FetchTagsAsync();
        /// <summary>
        /// Gets a paginated list of tags from the database.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of tags.</returns>
        Task<IEnumerable<string>> GetTagsPaginated(int page, int pageSize);
    }
}
