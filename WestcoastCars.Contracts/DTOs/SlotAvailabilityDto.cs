namespace WestcoastCars.Contracts.DTOs;

public class SlotAvailabilityDto
{
    public DateOnly Date { get; set; }
    /// <summary>0 = Morning (08–10), 1 = MidMorning (10–12), 2 = Afternoon (13–15)</summary>
    public int TimeSlot { get; set; }
    public bool IsBooked { get; set; }
}
