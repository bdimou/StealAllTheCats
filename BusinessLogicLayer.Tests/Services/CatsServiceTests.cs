using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using DataAccessLayer.RepositoryContracts;
using DataAccessLayer.Entities;
using AutoMapper;
using FluentValidation;

namespace BusinessLogicLayer.Tests.Services
{
    public class CatsServiceTests
    {
        [Fact]
        public async Task FetchCatsAsync_WhenCatsAreFetched_ReturnsCorrectCount()
        {
            // Arrange
            var mockCaasClient = new Mock<ICaasClient>();
            var mockCatsRepository = new Mock<ICatsRepository>();
            var mockLogger = new Mock<ILogger<CatsService>>();
            var mockMapper = new Mock<IMapper>();
            var mockHashProvider = new Mock<IImageHashProvider>();
            var mockIdValidator = new Mock<IValidator<string>>();
            var mockPaginationValidator = new Mock<IValidator<PaginationRequest>>();
            var mockTagRequestValidator = new Mock<IValidator<TagRequest>>();
            var mockCache = new Mock<IMemoryCache>();

            var service = new CatsService(
                mockCaasClient.Object,
                mockLogger.Object,
                mockCatsRepository.Object,
                mockMapper.Object,
                mockHashProvider.Object,
                mockIdValidator.Object,
                mockPaginationValidator.Object,
                mockTagRequestValidator.Object,
                mockCache.Object
            );

            var expectedCats = new List<CaasResponse>
            {
                new CaasResponse { Id = "1", Url = "http://example.com/cat1.jpg", Width = 100, Height = 100 },
                new CaasResponse { Id = "2", Url = "http://example.com/cat2.jpg", Width = 200, Height = 200 }
            };

            mockCaasClient.Setup(x => x.FetchKitties())
                .ReturnsAsync(expectedCats);

            mockCaasClient.Setup(x => x.DownloadImageAsync(It.IsAny<string>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            mockHashProvider.Setup(x => x.ComputeHash(It.IsAny<byte[]>()))
                .Returns("testHash");

            mockCatsRepository.Setup(x => x.SaveCats(It.IsAny<List<Cat>>()))
                .ReturnsAsync(2);

            // Act
            var result = await service.FetchCatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cats were successfully fetched from TheCatAPI", result.Message);
            Assert.Equal(2, result.UniqueCatsAdded);
            mockCaasClient.Verify(x => x.FetchKitties(), Times.Once);
            mockCatsRepository.Verify(x => x.SaveCats(It.IsAny<List<Cat>>()), Times.Once);
        }

        [Fact]
        public async Task FetchCatsAsync_WhenNoCatsAreFetched_ThrowsException()
        {
            // Arrange
            var mockCaasClient = new Mock<ICaasClient>();
            var mockCatsRepository = new Mock<ICatsRepository>();
            var mockLogger = new Mock<ILogger<CatsService>>();
            var mockMapper = new Mock<IMapper>();
            var mockHashProvider = new Mock<IImageHashProvider>();
            var mockIdValidator = new Mock<IValidator<string>>();
            var mockPaginationValidator = new Mock<IValidator<PaginationRequest>>();
            var mockTagRequestValidator = new Mock<IValidator<TagRequest>>();
            var mockCache = new Mock<IMemoryCache>();

            var service = new CatsService(
                mockCaasClient.Object,
                mockLogger.Object,
                mockCatsRepository.Object,
                mockMapper.Object,
                mockHashProvider.Object,
                mockIdValidator.Object,
                mockPaginationValidator.Object,
                mockTagRequestValidator.Object,
                mockCache.Object
            );

            mockCaasClient.Setup(x => x.FetchKitties())
                .ReturnsAsync((List<CaasResponse>)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.FetchCatsAsync());
            mockCaasClient.Verify(x => x.FetchKitties(), Times.Once);
            mockCatsRepository.Verify(x => x.SaveCats(It.IsAny<List<Cat>>()), Times.Never);
        }
    }
} 