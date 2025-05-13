using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Validators;
using DataAccessLayer.RepositoryContracts;
using DataAccessLayer.Entities;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Net.Mime.MediaTypeNames;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace BusinessLogicLayer.Services
{
    public class CatsService : ICatsService
    {
        private readonly ICaasClient _caasClient;
        private readonly ICatsRepository _CatsRepository;
        private readonly ILogger<CatsService> _logger;
        private readonly IMapper _mapper;
        private readonly IImageHashProvider _hashProvider;
        private readonly IValidator<string> _idValidator;
        private readonly IValidator<PaginationRequest> _paginationValidator;
        private readonly IValidator<TagRequest> _tagRequestValidator;
        private readonly IMemoryCache _cache;

        public CatsService(ICaasClient caasClient,
            ILogger<CatsService> logger,
            ICatsRepository catsRepository,
            IMapper mapper,
            IImageHashProvider hashProvider,
            IValidator<string> idValidator,
            IValidator<PaginationRequest> paginationValidator,
            IValidator<TagRequest> tagRequestValidator,
            IMemoryCache cache)
        {
            _caasClient = caasClient;
            _logger = logger;
            _CatsRepository = catsRepository;
            _mapper = mapper;
            _hashProvider = hashProvider;
            _idValidator = idValidator;
            _paginationValidator = paginationValidator;
            _tagRequestValidator = tagRequestValidator;
            _cache = cache;
        }

        public async Task<int> FetchCatsAsync()
        {
            _logger.LogInformation("Fetching cats from CaaS API...");
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
                List<string> tags = new List<string>();
                if (kitty.Breeds != null)
                {
                    foreach (var breed in kitty.Breeds)
                    {
                        if (!string.IsNullOrEmpty(breed?.Temperament))
                        {
                            tags.AddRange(breed.Temperament.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim()));
                        }
                    }
                }

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

                // get the kitty image in bytes[]
                var kittyImage = await _caasClient.DownloadImageAsync(kitty.Url);
                if (kittyImage == null)
                {
                    _logger.LogWarning($"Failed to download image for kitty with ID: {kitty.Id}");
                    continue; // Skip this kitty if the image download fails
                }
                // hash it for faster image comparison
                var imageHash = _hashProvider.ComputeHash(kittyImage);

                CatRequest catRequest = new CatRequest
                {
                    CatId = kitty.Id,
                    Width = kitty.Width,
                    Height = kitty.Height,
                    Image = kittyImage,
                    ImageHash = imageHash,
                    Created = DateTime.UtcNow,
                    tagRequests = tagRequests // Assign tagRequests during initialization
                };

                // Map from DTO to entity domain model
                Cat cat = _mapper.Map<Cat>(catRequest);
                cats.Add(cat);

            }

            _logger.LogInformation($"Mapped {cats.Count} kitties to Cat entities.");

            // Save kitty to the database, care for distinct photos
            var distinctPhotos = await _CatsRepository.SaveCats(cats);

            return distinctPhotos;

        }

        public async Task<PaginatedList<CatResponse>> GetCatsPaginated(string page, string pageSize)
        {
            _logger.LogInformation("Fetching paginated cats -no tag- from the repository...");

            // Create Pagination object and validate
            var paginationRequest = new PaginationRequest() { PageIndex = page, PageSize = pageSize };
            var validationResult = await _paginationValidator.ValidateAsync(paginationRequest);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid pagination request: {Errors}", validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }

            //Try to get the paginated list from the cache
            if (_cache.TryGetValue($"CatsPage{page}_Size{pageSize}", out PaginatedList<CatResponse> cachedCats))
            {
                _logger.LogInformation($"Returning cached cats for page: {page} and page size: {pageSize}");
                return cachedCats;
            }

            // Change page and pageSize to int
            int pageInt = int.TryParse(page, out int pageNumber) ? pageNumber : 1;
            int pageSizeInt = int.TryParse(pageSize, out int pageSizeNumber) ? pageSizeNumber : 10;

            // Fetch paginated cats and total count from the repository  
            var (cats, totalCount) = await _CatsRepository.GetCatsPaginated(pageInt, pageSizeInt);

            _logger.LogInformation($"Fetched {totalCount} cats from the repository for page: {page} and page size: {pageSize}");

            // Map the list of Cat entities to CatResponse DTOs  
            var catResponses = cats.Select(cat => _mapper.Map<CatResponse>(cat)).ToList();

            // Craete a new PaginatedList of CatResponse  
            PaginatedList<CatResponse> paginatedListResponse = new PaginatedList<CatResponse>(catResponses, pageInt, (int)Math.Ceiling((double)totalCount / pageSizeInt));

            // Cache the paginated list
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set($"CatsPage{page}_Size{pageSize}", paginatedListResponse, cacheEntryOptions);

            _logger.LogInformation($"Caching paginated cats for page: {page} and page size: {pageSize}");

            // Return the paginated list
            return paginatedListResponse;

        }

        public async Task<PaginatedList<CatResponse>> GetCatsPaginatedByTag(string page, string pageSize, string tag)
        {
            _logger.LogInformation($"Fetching paginated cats with tag: {tag} from the repository...");

            // Create Pagination object and validate
            var paginationRequest = new PaginationRequest() { PageIndex = page, PageSize = pageSize };
            var validationResult = await _paginationValidator.ValidateAsync(paginationRequest);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid pagination request: {Errors}", validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }
            // Validate the tag format
            var tagValidationResult = _tagRequestValidator.Validate(new TagRequest { Name = tag });
            if (!tagValidationResult.IsValid)
            {
                _logger.LogWarning("Invalid tag request: {Errors}", tagValidationResult.Errors);
                throw new ValidationException(tagValidationResult.Errors);
            }

            //Try to get the paginated list from the cache minding the tag
            if (_cache.TryGetValue($"CatsPage{page}_Size{pageSize}_Tag{tag}", out PaginatedList<CatResponse> cachedCats))
            {
                _logger.LogInformation($"Returning cached cats for page: {page} and page size: {pageSize} and tag: {tag}");
                return cachedCats;
            }

            // Change page and pageSize to int
            int pageInt = int.TryParse(page, out int pageNumber) ? pageNumber : 1;
            int pageSizeInt = int.TryParse(pageSize, out int pageSizeNumber) ? pageSizeNumber : 10;

            // Fetch paginated cats and total count from the repository  
            var (cats, totalCount) = await _CatsRepository.GetCatsPaginatedByTag(pageInt, pageSizeInt, tag);

            _logger.LogInformation($"Fetched {totalCount} cats from the repository for page: {page} and page size: {pageSize} and tag: {tag}");

            // Map the list of Cat entities to CatResponse DTOs  
            var catResponses = cats.Select(cat => _mapper.Map<CatResponse>(cat)).ToList();

            // Create a new PaginatedList of CatResponse  
            PaginatedList<CatResponse> paginatedListResponse = new PaginatedList<CatResponse>(catResponses, pageInt, (int)Math.Ceiling((double)totalCount / pageSizeInt));

            // Cache the paginated list
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            _cache.Set($"CatsPage{page}_Size{pageSize}_Tag{tag}", paginatedListResponse, cacheEntryOptions);

            _logger.LogInformation($"Caching paginated cats for page: {page} and page size: {pageSize} and tag: {tag}");

            // Return the paginated list
            return paginatedListResponse;
        }

        public async Task<CatResponse?> GetCatById(string id)
        {
            _logger.LogInformation($"Fetching cat with ID: {id} from the repository...");

            // Validate the ID format
            var idValidationResult = await _idValidator.ValidateAsync(id);
            if (!idValidationResult.IsValid)
            {
                _logger.LogWarning("Invalid ID format: {Errors}", idValidationResult.Errors);
                throw new ValidationException(idValidationResult.Errors);
            }

            // Try to get the cat from the cache
            if (_cache.TryGetValue($"CatId{id}", out CatResponse cachedCat))
            {
                _logger.LogInformation($"Returning cached cat with ID: {id}");
                return cachedCat;
            }

            // Get cat by ID from the repository
            Cat? cat = await _CatsRepository.GetCatById(id);
            
            // Convert to response DTO
            CatResponse? catResponse = cat != null ? _mapper.Map<CatResponse>(cat) : null;

            // Cache the cat response
            if (catResponse != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set($"CatId{id}", catResponse, cacheEntryOptions);
                _logger.LogInformation($"Caching cat with ID: {id}");

                return catResponse;
            }
            else
            {
                _logger.LogWarning($"Cat with ID: {id} not found in the repository.");
                return null;
            }
        }
    }
}
