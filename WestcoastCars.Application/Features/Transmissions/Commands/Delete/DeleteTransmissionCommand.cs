using MediatR;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Delete
{
    public class DeleteTransmissionCommand : IRequest
    {
        public int Id { get; set; }
    }
}
