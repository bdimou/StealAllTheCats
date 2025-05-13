using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace DataAccessLayer.Tests
{
    public class CatsRepositoryTests
    {
        private ApplicationDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private CatsRepository CreateRepository(ApplicationDbContext context)
        {
            var logger = new Mock<ILogger<CatsRepository>>();
            return new CatsRepository(context, logger.Object);
        }

        [Fact]
        public async Task GetCatById_ReturnsCatWithTags()
        {
            var db = CreateDbContext(nameof(GetCatById_ReturnsCatWithTags));
            var tag = new Tag { Name = "cute", Created = DateTime.UtcNow };
            var cat = new Cat { CatId = "cat1", Width = 100, Height = 100, Image = "url", ImageHash = "hash1", Created = DateTime.UtcNow, CatTags = new List<CatTag>() };
            var catTag = new CatTag { Cat = cat, Tag = tag };
            cat.CatTags.Add(catTag);
            db.Cats.Add(cat);
            db.Tags.Add(tag);
            db.CatTags.Add(catTag);
            db.SaveChanges();
            var repo = CreateRepository(db);

            var result = await repo.GetCatById(cat.Id.ToString());

            Assert.NotNull(result);
            Assert.Equal(cat.CatId, result.CatId);
            Assert.Single(result.CatTags);
            Assert.Equal("cute", result.CatTags.First().Tag.Name);
        }

        [Fact]
        public async Task GetCatsPaginated_ReturnsCorrectPageAndCount()
        {
            var db = CreateDbContext(nameof(GetCatsPaginated_ReturnsCorrectPageAndCount));
            for (int i = 1; i <= 15; i++)
            {
                db.Cats.Add(new Cat { CatId = $"cat{i}", Width = 100, Height = 100, Image = "url", ImageHash = $"hash{i}", Created = DateTime.UtcNow, CatTags = new List<CatTag>() });
            }
            db.SaveChanges();
            var repo = CreateRepository(db);

            var (items, totalCount) = await repo.GetCatsPaginated(2, 5);

            Assert.Equal(15, totalCount);
            Assert.Equal(5, items.Count);
            Assert.Equal("cat6", items[0].CatId);
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_ReturnsFilteredCats()
        {
            var db = CreateDbContext(nameof(GetCatsPaginatedByTag_ReturnsFilteredCats));
            var tag = new Tag { Name = "playful", Created = DateTime.UtcNow };
            db.Tags.Add(tag);
            for (int i = 1; i <= 10; i++)
            {
                var cat = new Cat { CatId = $"cat{i}", Width = 100, Height = 100, Image = "url", ImageHash = $"hash{i}", Created = DateTime.UtcNow, CatTags = new List<CatTag>() };
                if (i % 2 == 0)
                {
                    var catTag = new CatTag { Cat = cat, Tag = tag };
                    cat.CatTags.Add(catTag);
                    db.CatTags.Add(catTag);
                }
                db.Cats.Add(cat);
            }
            db.SaveChanges();
            var repo = CreateRepository(db);

            var (items, totalCount) = await repo.GetCatsPaginatedByTag(1, 3, "playful");

            Assert.Equal(5, totalCount); // Only even cats have the tag
            Assert.Equal(3, items.Count);
            Assert.All(items, c => Assert.Contains(c.CatTags, ct => ct.Tag.Name == "playful"));
        }

        [Fact]
        public async Task SaveCats_AddsNewCatsAndTags_AvoidsDuplicates()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                // Ensure schema is created
                using (var db = new ApplicationDbContext(options))
                {
                    db.Database.EnsureCreated();
                }

                // First context: add tag and first cat
                using (var db1 = new ApplicationDbContext(options))
                {
                    var tag1 = new Tag { Name = "cute", Created = DateTime.UtcNow };
                    db1.Tags.Add(tag1);
                    db1.SaveChanges();

                    var cat1 = new Cat
                    {
                        CatId = "cat1",
                        Width = 100,
                        Height = 100,
                        Image = "url1",
                        ImageHash = "hash1",
                        Created = DateTime.UtcNow,
                        CatTags = new List<CatTag> { new CatTag { Tag = tag1 } }
                    };
                    db1.Cats.Add(cat1);
                    db1.SaveChanges();
                }

                // Second context: add cat2 and cat3 (cat3 is a duplicate)
                using (var db2 = new ApplicationDbContext(options))
                {
                    var repo = CreateRepository(db2);

                    var tag1 = db2.Tags.First(t => t.Name == "cute");

                    var cat2 = new Cat
                    {
                        CatId = "cat2",
                        Width = 100,
                        Height = 100,
                        Image = "url2",
                        ImageHash = "hash2",
                        Created = DateTime.UtcNow,
                        CatTags = new List<CatTag> { new CatTag { Tag = new Tag { Name = "newtag", Created = DateTime.UtcNow } } }
                    };
                    var cat3 = new Cat
                    {
                        CatId = "cat3",
                        Width = 100,
                        Height = 100,
                        Image = "url3",
                        ImageHash = "hash1", // duplicate hash
                        Created = DateTime.UtcNow,
                        CatTags = new List<CatTag> { new CatTag { Tag = tag1 } }
                    };

                    var added = await repo.SaveCats(new List<Cat> { cat2, cat3 });

                    Assert.Equal(1, added); // only cat2 added, cat3 skipped
                    Assert.Equal(2, db2.Cats.Count());
                    Assert.Contains(db2.Tags, t => t.Name == "cute");
                    Assert.Contains(db2.Tags, t => t.Name == "newtag");
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
} 