namespace WestcoastCars.Contracts.DTOs;

public record VehicleStatsByMileageDto(string Label, int Min, int? Max, int Count);
