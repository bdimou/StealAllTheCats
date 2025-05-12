using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Repositories
{
    
    public class CatsRepository : ICatsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CatsRepository> _logger;

        public CatsRepository(ApplicationDbContext dbContext, ILogger<CatsRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task<Cat?> GetCatById(string id)
        {
            var query = await _dbContext.Cats
                .Include(c => c.CatTags) 
                .ThenInclude(ct => ct.Tag) 
                .FirstOrDefaultAsync(c => c.Id == int.Parse(id));

            return query;
        }


        public async Task<(List<Cat>, int)> GetCatsPaginated(int page, int pageSize)
        {
            var query = _dbContext.Cats
                .Include(c => c.CatTags) 
                .ThenInclude(ct => ct.Tag) 
                .OrderBy(c => c.Id);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }


        public async Task<(List<Cat>, int)> GetCatsPaginatedByTag(int page, int pageSize, string tag)
        {
            var query = _dbContext.Cats
                .Include(c => c.CatTags) 
                .ThenInclude(ct => ct.Tag) 
                .Where(c => c.CatTags.Any(ct => ct.Tag.Name == tag))
                .OrderBy(c => c.Id);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> SaveCats(List<Cat> cats)
        {
            // Get all existing tags into a case-insensitive dictionary
            var existingTags = await _dbContext.Tags
                .ToDictionaryAsync(t => t.Name.ToLower());

            _logger.LogInformation($"Found {existingTags.Count} existing tags in the database.");

            // Skipped cats counter
            int skipped = 0;
            foreach (var cat in cats)
            {
                var newCatTags = new List<CatTag>();

                foreach (var catTag in cat.CatTags)
                {
                    var tagName = catTag.Tag.Name.Trim();
                    var tagKey = tagName.ToLower();

                    // If tag doesn't exist, create and track it
                    if (!existingTags.TryGetValue(tagKey, out var tag))
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            Created = DateTime.UtcNow
                        };
                        _logger.LogInformation($"Adding new tag: {tag.Name}");
                        _dbContext.Tags.Add(tag);
                        existingTags[tagKey] = tag; // Add for future use
                    }

                    // Build new CatTag linking this cat to the (possibly new) tag
                    newCatTags.Add(new CatTag
                    {
                        Cat = cat,
                        Tag = tag
                    });
                }


                // Overwrite CatTags to avoid modifying during iteration
                cat.CatTags = newCatTags;

                bool exists = await _dbContext.Cats.AnyAsync(c => c.ImageHash == cat.ImageHash);
                if (!exists)
                {
                    _dbContext.Cats.Add(cat);
                }
                else
                {
                    _logger.LogInformation($"Skipped duplicate image for CatId={cat.CatId}, hash={cat.ImageHash}");
                    skipped++;
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Saved {cats.Count - skipped} new cats. Skipped {skipped} duplicates.");

            // Return the number of unique cats added to the database
            return (cats.Count - skipped);
        }

    }
}
