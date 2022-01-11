using Penguin.Code.Application.Services;
using Penguin.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Penguin.Application
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