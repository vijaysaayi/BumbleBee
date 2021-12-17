using Spectre.Console;

namespace BumbleBee.Code.Application.ExtensionMethods
{
    public static class AnsiConsoleExtensionMethods
    {
        public static void Display(string message, bool useSameLine = false, bool disableCheckBox = false)
        {
            if (useSameLine)
            {
                AnsiConsole.Markup(message);
                return;
            }

            if (disableCheckBox)
            {
                AnsiConsole.MarkupLine(message);
                return;
            }
            var emoji = Emoji.Known.CheckMark;
            AnsiConsole.MarkupLine($"[green]{emoji}[/] {message}");
        }
    }
}