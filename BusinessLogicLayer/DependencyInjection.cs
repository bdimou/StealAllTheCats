using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.HttpClients;
using Microsoft.Extensions.Configuration;
//using BusinessLogicLayer.Policies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;




namespace BusinessLogicLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // AutoMapper
            services.AddAutoMapper(typeof(CaasResponseToCatRequestMappingProfile).Assembly);

            // Validators
            services.AddValidatorsFromAssemblyContaining<IdValidator>();
            services.AddValidatorsFromAssemblyContaining<PaginationRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<TagRequestValidator>();

            // Add Polly policies
            //services.AddSingleton<IPollyPolicies, PollyPolicies>();

            // Services
            services.AddScoped<ICatsService, CatsService>();
            services.AddSingleton<IImageHashProvider, Sha256ImageHashProvider>();


            // HttpClient for CaasAPI
            services.AddHttpClient<ICaasClient, CaasClient>(client =>
            {
                client.BaseAddress = new Uri($"https://api.thecatapi.com/v1/");
            });

            return services;
        }
    }
}
