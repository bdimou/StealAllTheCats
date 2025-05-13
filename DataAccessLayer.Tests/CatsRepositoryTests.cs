using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DataAccessLayer.Tests
{
    public class CatsRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CatsRepository _repository;
        private readonly Mock<ILogger<CatsRepository>> _mockLogger;

        public CatsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<CatsRepository>>();
            _repository = new CatsRepository(_context, _mockLogger.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var tags = new List<Tag>
            {
                new Tag { Id = 1, Name = "friendly", Created = DateTime.UtcNow },
                new Tag { Id = 2, Name = "playful", Created = DateTime.UtcNow }
            };

            var cats = new List<Cat>
            {
                new Cat
                {
                    Id = 1,
                    CatId = "cat1",
                    Width = 100,
                    Height = 100,
                    Image = new byte[0],
                    ImageHash = "hash1",
                    Created = DateTime.UtcNow,
                    CatTags = new List<CatTag>
                    {
                        new CatTag { Tag = tags[0] }
                    }
                },
                new Cat
                {
                    Id = 2,
                    CatId = "cat2",
                    Width = 200,
                    Height = 200,
                    Image = new byte[0],
                    ImageHash = "hash2",
                    Created = DateTime.UtcNow,
                    CatTags = new List<CatTag>
                    {
                        new CatTag { Tag = tags[1] }
                    }
                }
            };

            _context.Tags.AddRange(tags);
            _context.Cats.AddRange(cats);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetCatById_ExistingId_ReturnsCat()
        {
            // Act
            var result = await _repository.GetCatById("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("cat1", result.CatId);
            Assert.Single(result.CatTags);
            Assert.Equal("friendly", result.CatTags.First().Tag.Name);
        }

        [Fact]
        public async Task GetCatById_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetCatById("999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCatsPaginated_ReturnsCorrectPage()
        {
            // Act
            var (cats, totalCount) = await _repository.GetCatsPaginated(1, 1);

            // Assert
            Assert.Single(cats);
            Assert.Equal(2, totalCount);
            Assert.Equal(1, cats[0].Id);
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_ReturnsFilteredCats()
        {
            // Act
            var (cats, totalCount) = await _repository.GetCatsPaginatedByTag(1, 10, "friendly");

            // Assert
            Assert.Single(cats);
            Assert.Equal(1, totalCount);
            Assert.Equal(1, cats[0].Id);
            Assert.Equal("friendly", cats[0].CatTags.First().Tag.Name);
        }

        [Fact]
        public async Task SaveCats_AddsNewCats()
        {
            // Arrange
            var newCats = new List<Cat>
            {
                new Cat
                {
                    CatId = "cat3",
                    Width = 300,
                    Height = 300,
                    Image = new byte[0],
                    ImageHash = "hash3",
                    Created = DateTime.UtcNow
                }
            };

            // Act
            var result = await _repository.SaveCats(newCats);

            // Assert
            Assert.Equal(1, result);
            Assert.Equal(3, _context.Cats.Count());
        }

        [Fact]
        public async Task SaveCats_DuplicateImageHash_DoesNotAddDuplicate()
        {
            // Arrange
            var duplicateCats = new List<Cat>
            {
                new Cat
                {
                    CatId = "cat3",
                    Width = 100,
                    Height = 100,
                    Image = new byte[0],
                    ImageHash = "hash1", // Same hash as existing cat
                    Created = DateTime.UtcNow
                }
            };

            // Act
            var result = await _repository.SaveCats(duplicateCats);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(2, _context.Cats.Count());
        }

        [Fact]
        public async Task GetCatsPaginated_InvalidPage_ReturnsEmptyList()
        {
            // Act
            var (cats, totalCount) = await _repository.GetCatsPaginated(999, 10);

            // Assert
            Assert.Empty(cats);
            Assert.Equal(2, totalCount);
        }

        [Fact]
        public async Task GetCatsPaginated_ZeroPageSize_ReturnsEmptyList()
        {
            // Act
            var (cats, totalCount) = await _repository.GetCatsPaginated(1, 0);

            // Assert
            Assert.Empty(cats);
            Assert.Equal(2, totalCount);
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_NonExistingTag_ReturnsEmptyList()
        {
            // Act
            var (cats, totalCount) = await _repository.GetCatsPaginatedByTag(1, 10, "nonexistent");

            // Assert
            Assert.Empty(cats);
            Assert.Equal(0, totalCount);
        }

        [Fact]
        public async Task SaveCats_EmptyList_ReturnsZero()
        {
            // Arrange
            var emptyList = new List<Cat>();

            // Act
            var result = await _repository.SaveCats(emptyList);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(2, _context.Cats.Count());
        }

        [Fact]
        public async Task SaveCats_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveCats(null));
        }

        [Fact]
        public async Task SaveCats_MultipleCatsWithSameHash_OnlySavesOne()
        {
            // Arrange
            var newCats = new List<Cat>
            {
                new Cat
                {
                    CatId = "cat3",
                    Width = 300,
                    Height = 300,
                    Image = new byte[0],
                    ImageHash = "hash1", // Same as existing cat
                    Created = DateTime.UtcNow
                },
                new Cat
                {
                    CatId = "cat4",
                    Width = 400,
                    Height = 400,
                    Image = new byte[0],
                    ImageHash = "hash1", // Same as existing cat
                    Created = DateTime.UtcNow
                }
            };

            // Act
            var result = await _repository.SaveCats(newCats);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(2, _context.Cats.Count());
        }

        [Fact]
        public async Task GetCatById_InvalidFormat_ThrowsFormatException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _repository.GetCatById("invalid"));
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_CaseInsensitive_ReturnsMatchingCats()
        {
            // Act
            var (cats, totalCount) = await _repository.GetCatsPaginatedByTag(1, 10, "FRIENDLY");

            // Assert
            Assert.Single(cats);
            Assert.Equal(1, totalCount);
            Assert.Equal("friendly", cats[0].CatTags.First().Tag.Name);
        }

        [Fact]
        public async Task SaveCats_WithTags_CreatesTagsAndRelationships()
        {
            // Arrange
            var newCats = new List<Cat>
            {
                new Cat
                {
                    CatId = "cat3",
                    Width = 300,
                    Height = 300,
                    Image = new byte[0],
                    ImageHash = "hash3",
                    Created = DateTime.UtcNow,
                    CatTags = new List<CatTag>
                    {
                        new CatTag
                        {
                            Tag = new Tag
                            {
                                Name = "newtag",
                                Created = DateTime.UtcNow
                            }
                        }
                    }
                }
            };

            // Act
            var result = await _repository.SaveCats(newCats);

            // Assert
            Assert.Equal(1, result);
            Assert.Equal(3, _context.Cats.Count());
            Assert.Equal(3, _context.Tags.Count());
            Assert.Equal(3, _context.CatTags.Count());
        }

        [Fact]
        public async Task GetCatsPaginated_IncludesAllRelatedData()
        {
            // Act
            var (cats, _) = await _repository.GetCatsPaginated(1, 10);

            // Assert
            Assert.All(cats, cat => Assert.NotNull(cat.CatTags));
            Assert.All(cats, cat => Assert.All(cat.CatTags, ct => Assert.NotNull(ct.Tag)));
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_IncludesAllRelatedData()
        {
            // Act
            var (cats, _) = await _repository.GetCatsPaginatedByTag(1, 10, "friendly");

            // Assert
            Assert.All(cats, cat => Assert.NotNull(cat.CatTags));
            Assert.All(cats, cat => Assert.All(cat.CatTags, ct => Assert.NotNull(ct.Tag)));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 