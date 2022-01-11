using MediatR;
using Sharprompt;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Code.Application.AzureSDKWrappers.GetInputs.AdditionalInformation
{
    public class GetAdditionalInformationCommandHandler : IRequestHandler<GetAdditionalInformationCommand, AdditionalInformation>
    {
        private string[] _acceptedCommands = new string[] { "WITH", "USES", "Done - Start Deploying to Azure" };

        private string[] _availableFeatures = new string[] { "source_code", "always_on" };

        private string[] _acceptedDependencies = new string[] { "sql", "storage", "postgresql" };

        private List<string> SelectedFeatures = new List<string>();

        private List<string> SelectedDependencies = new List<string>();

        private string[] featuresThatCanBeSelected
        {
            get
            {
                return _availableFeatures.Except(SelectedFeatures).ToArray();
            }
        }

        private string[] dependenciesThatCanBeSelected
        {
            get
            {
                return _acceptedDependencies.Except(SelectedDependencies).ToArray();
            }
        }

        public async Task<AdditionalInformation> Handle(GetAdditionalInformationCommand request, CancellationToken cancellationToken)
        {
            Prompt.Symbols.Prompt = new Symbol("", "");
            Prompt.Symbols.Done = new Symbol("", "");
            Prompt.Symbols.Error = new Symbol("", "");
            string command = "Done - Start Deploying to Azure";

            do
            {
                command = Prompt.Select("", _acceptedCommands); ;
                int length = command.Length + 4;
                
                string message = "";
                switch (command)
                {
                    case "WITH":
                        string feature = Prompt.Select("", featuresThatCanBeSelected); ;

                        //string feature = AnsiConsole.Prompt(
                        //        new SelectionPrompt<string>()
                        //                            .Title("")
                        //                            .PageSize(10)
                        //                            .AddChoices(featuresThatCanBeSelected)

                        //);

                        SelectedFeatures.Add(feature);
                        message = feature;
                        break;

                    case "USES":
                        //var dependenciesSelected = Prompt.MultiSelect("", dependenciesThatCanBeSelected);

                        var dependenciesSelected = AnsiConsole.Prompt(
                                new MultiSelectionPrompt<string>()
                                            .Title("")
                                            .NotRequired()
                                            .PageSize(10)
                                            .InstructionsText(
                                                "[grey](Press [blue]<space>[/] to toggle a dependency, " +
                                                "[green]<enter>[/] to accept)[/]")
                                            .AddChoices(dependenciesThatCanBeSelected)
                            );

                        foreach (var item in dependenciesSelected)
                        {
                            SelectedDependencies.Add(item);
                        }

                        message = string.Join(", ", dependenciesSelected);
                        break;
                }

                Console.SetCursorPosition(length, Console.CursorTop - 1);
                var color = Console.ForegroundColor;
                var bgColor = Console.BackgroundColor; ;
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                AnsiConsole.Markup(message);
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = color;
                Console.WriteLine("");
            } while (command != "Done - Start Deploying to Azure");

            var a = new AdditionalInformation()
            {
                Features = SelectedFeatures,
                Dependencies = SelectedDependencies
            };
            Prompt.Symbols.Prompt = new Symbol("🤔", "?");
            Prompt.Symbols.Done = new Symbol("😎", "V");
            Prompt.Symbols.Error = new Symbol("😱", ">>");
            return await Task.FromResult(a);
        }
    }
}