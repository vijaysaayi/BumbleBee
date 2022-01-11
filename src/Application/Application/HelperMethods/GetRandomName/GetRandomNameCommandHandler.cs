using MediatR;
using Penguin.Code.Application.ExtensionMethods;
using RandomNameGeneratorLibrary;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Code.Application.HelperMethods.GetRandomName
{
    public class GetRandomNameCommandHandler : IRequestHandler<GetRandomNameCommand, string>
    {
        public async Task<string> Handle(GetRandomNameCommand request, CancellationToken cancellationToken)
        {
            var placeGenerator = new PlaceNameGenerator();
            var name = placeGenerator.GenerateRandomPlaceName();
            name = name.ToLowerInvariant()
                       .Replace(" ", "")
                       .Replace(".", "")
                       .Replace("'", "");

            string randomName = $"{name}-{StringExtensionMethods.RandomString(request.Length)}";
            return await Task.FromResult(randomName);
        }
    }
}