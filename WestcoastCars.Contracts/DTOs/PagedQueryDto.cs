namespace WestcoastCars.Contracts.DTOs;

public class PagedQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
