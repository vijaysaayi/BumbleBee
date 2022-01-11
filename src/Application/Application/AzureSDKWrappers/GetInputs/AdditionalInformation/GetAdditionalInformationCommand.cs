using MediatR;
using System.Collections.Generic;

namespace Penguin.Code.Application.AzureSDKWrappers.GetInputs.AdditionalInformation
{
    public class AdditionalInformation
    {
        public List<string> Features { get; set; }

        public List<string> Dependencies { get; set; }

        public AdditionalInformation()
        {
            Features = new List<string>();
            Dependencies = new List<string>();
        }
    }

    public class GetAdditionalInformationCommand : IRequest<AdditionalInformation>
    {
    }
}