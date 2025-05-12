using BusinessLogicLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.ServiceContracts
{
    public interface ICatsService
    {
        /// <summary>
        /// Fetches a list of cats from an external API and saves them to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<int> FetchCatsAsync();
        /// <summary>
        /// Gets a paginated list of cats from the database.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of cats.</returns>
        Task<PaginatedList<CatResponse>> GetCatsPaginated(string page, string pageSize);
        /// <summary>
        /// Gets a paginated list of cats filtered by tag from the database.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="tag">The tag to filter by.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of cats.</returns>
        Task<PaginatedList<CatResponse>> GetCatsPaginatedByTag(string page, string pageSize, string tag);
        /// <summary>
        /// Gets a cat by its ID from the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CatResponse?> GetCatById(string id);
    }
}
