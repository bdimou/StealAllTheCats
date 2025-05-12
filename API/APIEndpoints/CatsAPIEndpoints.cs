using Microsoft.AspNetCore.Cors.Infrastructure;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.APIEndpoints
{
    public static class CatsAPIEndpoints
    {
        public static IEndpointRouteBuilder MapCatsAPIEndpoints(this IEndpointRouteBuilder app)
        {
            
            //GET /api/cats with pagination  
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
            });

            //GET /api/cats/id  
            app.MapGet("/api/cats/{id}", async (ICatsService catsService, string id) =>
            {
                // Get cat by ID  
                CatResponse? cat = await catsService.GetCatById(id);
                return cat is not null ? Results.Ok(cat) : Results.NotFound($"Cat with Id {id} was not found");
            });

            app.MapPost("/api/cats/fetch", async (ICatsService catsService) =>
            {
                // fetch 25 cats from the Caas API
                int uniqueCats = await catsService.FetchCatsAsync();
                return Results.Ok($"Cats were fetched, {uniqueCats} photos were added to the db");
            });

            return app;
        }
    }
}
