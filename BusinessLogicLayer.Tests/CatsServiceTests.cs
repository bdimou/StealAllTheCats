using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using Xunit;

namespace BusinessLogicLayer.Tests
{
    public class CatsServiceTests
    {
        private readonly Mock<ICatsRepository> _mockCatsRepository;
        private readonly Mock<ICaasClient> _mockCaasClient;
        private readonly Mock<ILogger<CatsService>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IImageHashProvider> _mockHashProvider;
        private readonly Mock<IValidator<string>> _mockIdValidator;
        private readonly Mock<IValidator<PaginationRequest>> _mockPaginationValidator;
        private readonly Mock<IValidator<TagRequest>> _mockTagValidator;
        private readonly IMemoryCache _memoryCache;
        private readonly CatsService _catsService;

        public CatsServiceTests()
        {
            _mockCatsRepository = new Mock<ICatsRepository>();
            _mockCaasClient = new Mock<ICaasClient>();
            _mockLogger = new Mock<ILogger<CatsService>>();
            _mockMapper = new Mock<IMapper>();
            _mockHashProvider = new Mock<IImageHashProvider>();
            _mockIdValidator = new Mock<IValidator<string>>();
            _mockPaginationValidator = new Mock<IValidator<PaginationRequest>>();
            _mockTagValidator = new Mock<IValidator<TagRequest>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _catsService = new CatsService(
                _mockCaasClient.Object,
                _mockLogger.Object,
                _mockCatsRepository.Object,
                _mockMapper.Object,
                _mockHashProvider.Object,
                _mockIdValidator.Object,
                _mockPaginationValidator.Object,
                _mockTagValidator.Object,
                _memoryCache
            );
        }

        [Fact]
        public async Task GetCatById_ValidId_ReturnsCat()
        {
            // Arrange
            var catId = "1";
            var cat = new Cat { Id = 1, CatId = "cat1" };
            var catResponse = new CatResponse(1, "cat1", 100, 100, "http://example.com/cat1.jpg", new List<TagResponse>(), DateTime.UtcNow);

            _mockIdValidator.Setup(x => x.ValidateAsync(catId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCatsRepository.Setup(x => x.GetCatById(catId))
                .ReturnsAsync(cat);

            _mockMapper.Setup(x => x.Map<CatResponse>(cat))
                .Returns(catResponse);

            // Act
            var result = await _catsService.GetCatById(catId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(catResponse, result);
            _mockCatsRepository.Verify(x => x.GetCatById(catId), Times.Once);
        }

        [Fact]
        public async Task GetCatById_InvalidId_ThrowsValidationException()
        {
            // Arrange
            var catId = "invalid";
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Id", "Invalid ID format")
            };

            _mockIdValidator.Setup(x => x.ValidateAsync(catId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _catsService.GetCatById(catId));
        }

        [Fact]
        public async Task GetCatsPaginated_ValidRequest_ReturnsPaginatedList()
        {
            // Arrange
            var page = "1";
            var pageSize = "10";
            var cats = new List<Cat>
            {
                new Cat { Id = 1, CatId = "cat1" },
                new Cat { Id = 2, CatId = "cat2" }
            };
            var catResponses = cats.Select(c => new CatResponse(c.Id, c.CatId, 100, 100, "http://example.com/cat.jpg", new List<TagResponse>(), DateTime.UtcNow)).ToList();

            _mockPaginationValidator.Setup(x => x.ValidateAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCatsRepository.Setup(x => x.GetCatsPaginated(1, 10))
                .ReturnsAsync((cats, 2));

            _mockMapper.Setup(x => x.Map<CatResponse>(It.IsAny<Cat>()))
                .Returns((Cat c) => catResponses.First(r => r.Id == c.Id));

            // Act
            var result = await _catsService.GetCatsPaginated(page, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_ValidRequest_ReturnsFilteredList()
        {
            // Arrange
            var page = "1";
            var pageSize = "10";
            var tag = "friendly";
            var cats = new List<Cat>
            {
                new Cat { Id = 1, CatId = "cat1" }
            };
            var catResponses = cats.Select(c => new CatResponse(c.Id, c.CatId, 100, 100, "http://example.com/cat.jpg", new List<TagResponse>(), DateTime.UtcNow)).ToList();

            _mockPaginationValidator.Setup(x => x.ValidateAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockTagValidator.Setup(x => x.Validate(It.IsAny<TagRequest>()))
                .Returns(new ValidationResult());

            _mockCatsRepository.Setup(x => x.GetCatsPaginatedByTag(1, 10, tag))
                .ReturnsAsync((cats, 1));

            _mockMapper.Setup(x => x.Map<CatResponse>(It.IsAny<Cat>()))
                .Returns((Cat c) => catResponses.First(r => r.Id == c.Id));

            // Act
            var result = await _catsService.GetCatsPaginatedByTag(page, pageSize, tag);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public async Task FetchCatsAsync_SuccessfulFetch_ReturnsCount()
        {
            // Arrange
            var caasResponses = new List<CaasResponse>
            {
                new CaasResponse
                {
                    Id = "cat1",
                    Width = 100,
                    Height = 100,
                    Url = "http://example.com/cat1.jpg",
                    Breeds = new List<Breed>
                    {
                        new Breed { Temperament = "friendly,playful" }
                    }
                }
            };

            var cat = new Cat
            {
                Id = 1,
                CatId = "cat1",
                Width = 100,
                Height = 100,
                Image = "http://example.com/cat1.jpg",
                ImageHash = "hash1",
                Created = DateTime.UtcNow
            };

            _mockCaasClient.Setup(x => x.FetchKitties())
                .ReturnsAsync(caasResponses);

            _mockCaasClient.Setup(x => x.DownloadImageAsync(It.IsAny<string>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            _mockHashProvider.Setup(x => x.ComputeHash(new byte[] { 1, 2, 3 }))
                .Returns("hash1");

            _mockMapper.Setup(x => x.Map<Cat>(It.IsAny<CatRequest>()))
                .Returns(cat);

            _mockCatsRepository.Setup(x => x.SaveCats(It.IsAny<List<Cat>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _catsService.FetchCatsAsync();

            // Assert
            Assert.Equal(1, result.UniqueCatsAdded);
            _mockCatsRepository.Verify(x => x.SaveCats(It.IsAny<List<Cat>>()), Times.Once);
        }

        [Fact]
        public async Task GetCatById_CachedResult_ReturnsFromCache()
        {
            // Arrange
            var catId = "1";
            var cat = new Cat { Id = 1, CatId = "cat1" };
            var catResponse = new CatResponse(1, "cat1", 100, 100, "http://example.com/cat1.jpg", new List<TagResponse>(), DateTime.UtcNow);

            _mockIdValidator.Setup(x => x.ValidateAsync(catId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCatsRepository.Setup(x => x.GetCatById(catId))
                .ReturnsAsync(cat);

            _mockMapper.Setup(x => x.Map<CatResponse>(cat))
                .Returns(catResponse);

            // Act - First call
            var result1 = await _catsService.GetCatById(catId);
            // Act - Second call (should use cache)
            var result2 = await _catsService.GetCatById(catId);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(result1, result2);
            _mockCatsRepository.Verify(x => x.GetCatById(catId), Times.Once); // Should only call repository once
        }

        [Fact]
        public async Task GetCatsPaginated_InvalidPage_ThrowsValidationException()
        {
            // Arrange
            var page = "0";
            var pageSize = "10";
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("PageIndex", "Page index must be greater than 0")
            };

            _mockPaginationValidator.Setup(x => x.ValidateAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _catsService.GetCatsPaginated(page, pageSize));
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_InvalidTag_ThrowsValidationException()
        {
            // Arrange
            var page = "1";
            var pageSize = "10";
            var tag = "";
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Tag name cannot be empty")
            };

            _mockPaginationValidator.Setup(x => x.ValidateAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockTagValidator.Setup(x => x.Validate(It.IsAny<TagRequest>()))
                .Returns(new ValidationResult(validationFailures));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _catsService.GetCatsPaginatedByTag(page, pageSize, tag));
        }

        [Fact]
        public async Task FetchCatsAsync_EmptyResponse_ThrowsException()
        {
            // Arrange
            _mockCaasClient.Setup(x => x.FetchKitties())
                .ReturnsAsync(new List<CaasResponse>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _catsService.FetchCatsAsync());
        }

        [Fact]
        public async Task FetchCatsAsync_ApiError_ThrowsException()
        {
            // Arrange
            _mockCaasClient.Setup(x => x.FetchKitties())
                .ThrowsAsync(new HttpRequestException("API Error"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _catsService.FetchCatsAsync());
        }

        [Fact]
        public async Task GetCatsPaginated_CacheExpiration_RefetchesFromRepository()
        {
            // Arrange
            var page = "1";
            var pageSize = "10";
            var cats = new List<Cat>
            {
                new Cat { Id = 1, CatId = "cat1" }
            };
            var catResponses = cats.Select(c => new CatResponse(c.Id, c.CatId, 100, 100, "http://example.com/cat.jpg", new List<TagResponse>(), DateTime.UtcNow)).ToList();

            _mockPaginationValidator.Setup(x => x.ValidateAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCatsRepository.Setup(x => x.GetCatsPaginated(1, 10))
                .ReturnsAsync((cats, 1));

            _mockMapper.Setup(x => x.Map<CatResponse>(It.IsAny<Cat>()))
                .Returns((Cat c) => catResponses.First(r => r.Id == c.Id));

            // Act - First call
            var result1 = await _catsService.GetCatsPaginated(page, pageSize);
            
            // Simulate cache expiration
            _memoryCache.Remove($"CatsPage{page}_Size{pageSize}");
            
            // Act - Second call after cache expiration
            var result2 = await _catsService.GetCatsPaginated(page, pageSize);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            _mockCatsRepository.Verify(x => x.GetCatsPaginated(1, 10), Times.Exactly(2));
        }

        [Fact]
        public async Task GetCatsPaginatedByTag_CacheKeyIncludesTag()
        {
            // Arrange
            var page = "1";
            var pageSize = "10";
            var tag = "friendly";
            var cats = new List<Cat>
            {
                new Cat { Id = 1, CatId = "cat1" }
            };
            var catResponses = cats.Select(c => new CatResponse(c.Id, c.CatId, 100, 100, "http://example.com/cat.jpg", new List<TagResponse>(), DateTime.UtcNow)).ToList();

            _mockPaginationValidator.Setup(x => x.ValidateAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockTagValidator.Setup(x => x.Validate(It.IsAny<TagRequest>()))
                .Returns(new ValidationResult());

            _mockCatsRepository.Setup(x => x.GetCatsPaginatedByTag(1, 10, tag))
                .ReturnsAsync((cats, 1));

            _mockMapper.Setup(x => x.Map<CatResponse>(It.IsAny<Cat>()))
                .Returns((Cat c) => catResponses.First(r => r.Id == c.Id));

            // Act
            var result = await _catsService.GetCatsPaginatedByTag(page, pageSize, tag);

            // Assert
            Assert.NotNull(result);
            Assert.True(_memoryCache.TryGetValue($"CatsPage{page}_Size{pageSize}_Tag{tag}", out _));
        }

        [Fact]
        public async Task FetchCatsAsync_ImageHashCollision_HandlesGracefully()
        {
            // Arrange
            var caasResponses = new List<CaasResponse>
            {
                new CaasResponse
                {
                    Id = "cat1",
                    Width = 100,
                    Height = 100,
                    Url = "http://example.com/cat1.jpg",
                    Breeds = new List<Breed>
                    {
                        new Breed { Temperament = "friendly,playful" }
                    }
                }
            };

            var cat = new Cat
            {
                Id = 1,
                CatId = "cat1",
                Width = 100,
                Height = 100,
                Image = "http://example.com/cat1.jpg",
                ImageHash = "existing_hash",
                Created = DateTime.UtcNow
            };

            _mockCaasClient.Setup(x => x.FetchKitties())
                .ReturnsAsync(caasResponses);

            _mockCaasClient.Setup(x => x.DownloadImageAsync(It.IsAny<string>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            _mockHashProvider.Setup(x => x.ComputeHash(new byte[] { 1, 2, 3 }))
                .Returns("existing_hash");

            _mockMapper.Setup(x => x.Map<Cat>(It.IsAny<CatRequest>()))
                .Returns(cat);

            _mockCatsRepository.Setup(x => x.SaveCats(It.IsAny<List<Cat>>()))
                .ReturnsAsync(0); // Simulate duplicate hash

            // Act
            var result = await _catsService.FetchCatsAsync();

            // Assert
            Assert.Equal(0, result.UniqueCatsAdded);
            _mockCatsRepository.Verify(x => x.SaveCats(It.IsAny<List<Cat>>()), Times.Once);
        }
    }
} 