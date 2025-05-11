using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.RepositoryContracts;
using DataAccessLayer.Entities;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BusinessLogicLayer.Services
{
    public class CatsService : ICatsService
    {
        private readonly CaasClient _caasClient;
        private readonly PhotoClient _photoClient;
        private readonly ICatsRepository _CatsRepository;
        private readonly ILogger<CatsService> _logger;
        private readonly IMapper _mapper;

        public CatsService(CaasClient caasClient, PhotoClient photoClient, ILogger<CatsService> logger, ICatsRepository catsRepository, IMapper mapper)
        {
            _caasClient = caasClient;
            _photoClient = photoClient;
            _logger = logger;
            _CatsRepository = catsRepository;
            _mapper = mapper;
        }

        public async Task<int> FetchCatsAsync()
        {
            // Call the CaasClient to fetch them kitties
            List<CaasResponse>? kitties = await _caasClient.FetchKitties();

            if (kitties == null || !kitties.Any())
            {
                _logger.LogWarning("No cats were returned from the CaaS API.");
                throw new InvalidOperationException("CaaS API did not find any cats");
            }

            // Log the number of kitties fetched
            _logger.LogInformation($"Fetched {kitties.Count} kitties from the API.");


            // Start deserializing the response to create our DTOs
            List<Cat> cats = new List<Cat>();
            foreach (var kitty in kitties)
            {
                // Placeholder for the list of tags of a single kitten
                List<TagRequest> tagRequests = new List<TagRequest>();

                // Extract tags from the breeds, split on delimiter(,)
                List<string> tags = kitty.Breeds
                   .SelectMany(b => b.Temperament.Split(',', StringSplitOptions.RemoveEmptyEntries))
                   .Select(t => t.Trim())
                   .ToList();

                // For each tag, create TagRequest object and add to the list
                foreach (string tag in tags)
                {
                    TagRequest tempTagRequest = new TagRequest
                    {
                        Name = tag,
                        Created = DateTime.UtcNow 
                    };
                    tagRequests.Add(tempTagRequest);
                }

                CatRequest catRequest = new CatRequest
                {
                    CatId = kitty.Id,
                    Width = kitty.Width,
                    Height = kitty.Height,
                    Image = await _photoClient.DownloadImageAsync(kitty.Url),
                    Created = DateTime.UtcNow,
                    tagRequests = tagRequests // Assign tagRequests during initialization
                };

                // Map from DTO to entity domain model
                Cat cat = _mapper.Map<Cat>(catRequest);
                cats.Add(cat);

            }
            // Save kitty to the database, care for distinct photos
            var distinctPhotos = await _CatsRepository.SaveCats(cats);

            return distinctPhotos;

        }

        public Task<CatResponse?> GetCatByCondition(Func<CatResponse, bool> condition)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CatResponse>> GetCatsPaginated(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CatResponse>> GetCatsPaginatedByTag(int page, int pageSize, string tag)
        {
            throw new NotImplementedException();
        }
    }
}
