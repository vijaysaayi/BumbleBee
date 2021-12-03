using BumbleBee.Code.Application.Services;
using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BumbleBee.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<IAzureService, AzureService>();

            return services;
        }
    }
}