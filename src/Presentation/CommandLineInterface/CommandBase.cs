using BumbleBee.CommandLineInterface.Commands.Create;
using BumbleBee.CommandLineInterface.Commands.Deploy;
using BumbleBee.CommandLineInterface.Services;
using CommandDotNet;

namespace BumbleBee.CommandLineInterface
{
    [Command(
       ExtendedHelpText = "Microsoft Corporation")]
    public class CommandBase
    {
        private static bool _inSession;

        [DefaultMethod]
        public void StartSession(
            CommandContext context,
            InteractiveSession interactiveSession,
            [Option(ShortName = "i")] bool interactive)
        {
            if (interactive && !_inSession)
            {
                context.Console.WriteLine("start session");
                _inSession = true;
                interactiveSession.Start();
            }
            else
            {
                //context.Console.WriteLine($"no session {interactive} {_inSession}");
                context.ShowHelpOnExit = true;
            }
        }

        [SubCommand]
        public CreateCommand Create { get; set; } = null!;

        [SubCommand]
        public DeployCommand Deploy { get; set; } = null!;
    }
}