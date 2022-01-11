using Penguin.CommandLineInterface.Commands.Create;
using Penguin.CommandLineInterface.Commands.Deploy;
using Penguin.CommandLineInterface.Commands.Use;
using Penguin.CommandLineInterface.Services;
using CommandDotNet;

namespace Penguin.CommandLineInterface
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

        [SubCommand]
        public UseCommand Use { get; set; } = null!;
    }
}