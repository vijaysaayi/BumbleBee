using BumbleBee.Application;
using BumbleBee.CommandLineInterface.ExtensionMethods;
using CommandDotNet;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BumbleBee.CommandLineInterface
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            var configuration = LoadConfiguration();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddApplication();
            services.AddSingleton<CommandBase>();

            AddLogger(args, services);

            var appConfigSettings = new NameValueCollection
            {
            };
            AppRunner appRunner = GetAppRunner(appConfigSettings, null);
            var commandClassTypes = appRunner.GetCommandClassTypes();
            foreach (var type in commandClassTypes)
            {
                services.AddTransient(type);
            }

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            appRunner.UseMicrosoftDependencyInjection(serviceProvider);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            return appRunner.Run(args);
        }

        private static void AddLogger(string[] args, IServiceCollection services)
        {
            var serilogLogConfiguration = new LoggerConfiguration()
                                            .WriteTo.File("bumblebee.log", rollingInterval: RollingInterval.Day);

            bool verboseLoggingRequired = args.Contains("[v]") || args.Contains("[verbose]");
            if (verboseLoggingRequired)
            {
                serilogLogConfiguration.MinimumLevel.Debug();
            }
            else
            {
                serilogLogConfiguration.MinimumLevel.Information();
            }

            var serilogLogger = serilogLogConfiguration.CreateLogger();
            services.AddLogging(logging =>
            {
                logging.AddSerilog(logger: serilogLogger, dispose: true);
            });
        }

        private static IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                             .AddJsonFile("config.json", optional: true, reloadOnChange: false)
                                             .Build();
        }

        public static AppRunner GetAppRunner(NameValueCollection appConfigSettings = null, string appNameForTests = "bumblebee")
        {
            appConfigSettings ??= new NameValueCollection();
            return new AppRunner<CommandBase>(appNameForTests is null ? null : new AppSettings { Help = { UsageAppName = appNameForTests } })
                .UseDefaultMiddleware(excludePrompting: true, excludeVersionMiddleware: true)
                .UseNameCasing(Case.KebabCase)
                .UseInteractiveMode("BumbleBee")
                .UseDefaultsFromAppSetting(appConfigSettings, includeNamingConventions: true)                
                .UseErrorHandler((context, ex) =>
                {
                    DisplayMessage(ex.Message, true);
                    context.PrintHelp();
                    return 1;
                });
            ;
        }

        public static void DisplayMessage(string message, bool isError = false)
        {
            string logLevel = isError ? "ERR" : "INF";
            ConsoleColor originalColor = Console.ForegroundColor;
            if (isError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Write($"[{DateTime.Now:HH:mm:ss} ");

            Console.Write($"{logLevel}] ");
            Console.ForegroundColor = originalColor;
            Console.WriteLine(message);
        }
    }
}