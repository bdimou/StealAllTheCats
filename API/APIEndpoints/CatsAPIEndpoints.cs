using Microsoft.AspNetCore.Cors.Infrastructure;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace API.APIEndpoints
{
    public static class CatsAPIEndpoints
    {
        /// <summary>
        /// Maps all cat-related API endpoints
        /// </summary>
        /// <param name="app">The endpoint route builder</param>
        /// <returns>The endpoint route builder for chaining</returns>
        public static IEndpointRouteBuilder MapCatsAPIEndpoints(this IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Retrieves a paginated list of cats, optionally filtered by tag
            /// </summary>
            /// <param name="catsService">The cats service for handling business logic</param>
            /// <param name="page">Page number</param>
            /// <param name="pageSize">Number of items per page</param>
            /// <param name="tag">Optional tag to filter cats</param>
            /// <returns>A paginated list of cats</returns>
            /// <response code="200">Returns the paginated list of cats</response>
            /// <response code="404">No cats found</response>
           
            app.MapGet("/api/cats", async (ICatsService catsService, string page = "1", string pageSize = "10", string? tag = null) =>
            {
                PaginatedList<CatResponse>? paginatedCats = null;

                if (tag == null)
                {
                    // Get paginated cats without filtering by tag
                    paginatedCats = await catsService.GetCatsPaginated(page, pageSize);
                }
                else
                {
                    // Get paginated cats filtered by tag
                    paginatedCats = await catsService.GetCatsPaginatedByTag(page, pageSize, tag);
                }

                // Check if paginatedCats is not null and has any items and return
                return paginatedCats is not null ? Results.Ok(paginatedCats) : Results.NotFound();
            })
            .WithName("GetCats")
            .Produces<PaginatedList<CatResponse>>(StatusCodes.Status200OK)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Retrieves a paginated list of cats";
                operation.Description = "Gets a paginated list of cats, optionally filtered by tag";
                operation.Parameters[0].Description = "Page number";
                operation.Parameters[1].Description = "Number of items per page";
                operation.Parameters[2].Description = "Optional tag to filter cats";
                return operation;
            });

            /// <summary>
            /// Retrieves a specific cat by its ID
            /// </summary>
            /// <param name="catsService">The cats service for handling business logic</param>
            /// <param name="id">The unique identifier of the cat</param>
            /// <returns>The cat with the specified ID</returns>
            /// <response code="200">Returns the requested cat</response>
            /// <response code="404">Cat with the specified ID was not found</response>
         
            app.MapGet("/api/cats/{id}", async (ICatsService catsService, string id) =>
            {
                // Get cat by ID  
                CatResponse? cat = await catsService.GetCatById(id);
                return cat is not null ? Results.Ok(cat) : Results.NotFound($"Cat with Id {id} was not found");
            })
            .WithName("GetCatById")
            .Produces<CatResponse>(StatusCodes.Status200OK)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Retrieves a specific cat by ID";
                operation.Description = "Gets detailed information about a specific cat";
                operation.Parameters[0].Description = "The unique identifier of the cat";
                return operation;
            });

            /// <summary>
            /// Fetches new cats from TheCatAPI and adds them to the database
            /// </summary>
            /// <param name="catsService">The cats service for handling business logic</param>
            /// <returns>Number of unique cats added to the database</returns>
            /// <response code="200">Returns the number of unique cats added</response>

            app.MapPost("/api/cats/fetch", async (ICatsService catsService) =>
            {
                return await catsService.FetchCatsAsync();
            })
            .WithName("FetchCats")
            .Produces<FetchCatsResponse>(StatusCodes.Status200OK)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Fetches new cats from TheCatAPI";
                operation.Description = "Fetches new cats from TheCatAPI and adds them to the database";
                return operation;
            });

            return app;
        }
    }
}
