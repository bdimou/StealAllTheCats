using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Entities;

namespace DataAccessLayer.RepositoryContracts
{
    public interface ICatsRepository
    {
        /// <summary>
        /// Fetches a list of cats from an external API and saves them to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<int> SaveCats(List<Cat> cat);
        /// <summary>
        /// Gets a paginated list of cats from the database.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of cats.</returns>
        Task<IEnumerable<Cat>> GetCatsPaginated(int page, int pageSize);
        /// <summary>
        /// Gets a paginated list of cats filtered by tag from the database.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="tag">The tag to filter by.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of cats.</returns>
        Task<IEnumerable<Cat>> GetCatsPaginatedByTag(int page, int pageSize, string tag);
        /// <summary>
        /// Gets a cat by its ID from the database.
        /// </summary>
        /// <param name="id">The ID of the cat to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, containing the cat if found; otherwise, null.</returns>
        Task<Cat?> GetCatByCondition(Func<Cat, bool> condition);
    }
}
