using MediatR;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Update
{
    public class UpdateTransmissionCommand : IRequest
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
