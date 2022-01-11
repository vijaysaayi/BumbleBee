using MediatR;

namespace Penguin.Code.Application.HelperMethods.GetRandomName
{
    public class GetRandomNameCommand : IRequest<string>
    {
        public int Length { get; set; }

        public GetRandomNameCommand()
        {
            Length = 4;
        }
    }
}