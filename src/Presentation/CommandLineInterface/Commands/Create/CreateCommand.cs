using CommandDotNet;

namespace BumbleBee.CommandLineInterface.Commands.Create
{
    [Command(Name = "create",
             Usage = "create [command]",
             Description = "Create new WebApp")]
    public class CreateCommand
    {
        [SubCommand]
        public FlaskApp FlaskApp { get; set; } = null!;

        [SubCommand]
        public DjangoApp DjangoApp { get; set; } = null!;
    }
}