using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        public Task<Cat?> GetCatById(string id)
        {
            throw new NotImplementedException();
        }
    
        public Task FetchCatsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Tag>> GetTagsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Cat?> GetCatByCondition(Func<Cat, bool> condition)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Cat>> GetCatsPaginated(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Cat>> GetCatsPaginatedByTag(int page, int pageSize, string tag)
        {
            throw new NotImplementedException();
        }

        public async Task<int> SaveCats(List<Cat> cats)
        {
            // Get all existing tags into a case-insensitive dictionary
            var existingTags = await _dbContext.Tags
                .ToDictionaryAsync(t => t.Name.ToLower());

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
