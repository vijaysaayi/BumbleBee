using BumbleBee.CommandLineInterface.Services;
using CommandDotNet;

namespace BumbleBee.CommandLineInterface.ExtensionMethods
{
    public static class InteractiveMiddleware
    {
        public static AppRunner UseInteractiveMode(this AppRunner appRunner, string appName)
        {
            return appRunner.Configure(c =>
            {
                // use the existing appRunner to reuse the configuration.
                c.UseParameterResolver(ctx => new InteractiveSession(appRunner, appName, ctx));
            });
        }
    }
}