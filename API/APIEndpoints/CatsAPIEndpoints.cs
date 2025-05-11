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
            /*
            //GET /api/cats with pagination  
            app.MapGet("/api/cats", async (ICatsService catsService, int page = 1, int pageSize = 10, string? tag = null) =>
            {
                if (page < 1 || pageSize < 1)
                {
                    return Results.BadRequest("Page and pageSize must be greater than 0.");
                }

                if (tag == null)
                {
                    // Get paginated cats without filtering by tag
                    var paginatedCats = await catsService.GetCatsPaginated(page, pageSize);
                }
                else
                {
                    // Get paginated cats filtered by tag
                    var paginatedCats = await catsService.GetCatsPaginatedByTag(page, pageSize, tag);
                }

                // Check if paginatedCats is not null and has any items and return
                return paginatedCats is not null && paginatedCats.Any() ? Results.Ok(paginatedCats) : Results.NotFound();
            });

            //GET /api/cats/id  
            app.MapGet("/api/cats/{id}", async (ICatsService catsService, string id) =>
            {
                // Get cat by ID  
                CatResponse? cat = await catsService.GetCatByCondition(temp => temp.Id == int.Parse(id));
                return cat is not null ? Results.Ok(cat) : Results.NotFound();
            });
            */

            // POST /api/cats/fetch
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
