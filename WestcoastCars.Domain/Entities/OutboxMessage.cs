using System;

namespace WestcoastCars.Domain.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; init; }
        public DateTime OccurredOnUtc { get; init; }
        public string Type { get; init; } = default!;
        public string Content { get; init; } = default!;
        public DateTime? ProcessedOnUtc { get; set; }
        public string? Error { get; set; }
    }
}
