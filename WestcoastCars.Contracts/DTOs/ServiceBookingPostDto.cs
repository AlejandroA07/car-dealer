using System;
using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.DTOs;

public class ServiceBookingPostDto
{
    [Required(ErrorMessage = "Registreringsnummer måste anges")]
    [MaxLength(10)]
    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Typ av service måste anges")]
    [MaxLength(50)]
    public string ServiceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Datum för bokning måste anges")]
    public DateTime BookingDate { get; set; }

    [Range(0, 2, ErrorMessage = "Ogiltigt tidsfönster")]
    public int TimeSlot { get; set; }

    [Required(ErrorMessage = "Namn måste anges")]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-post måste anges")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
    [MaxLength(256)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefonnummer måste anges")]
    [MaxLength(50)]
    public string CustomerPhone { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(36)]
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Proof that <see cref="CustomerEmail"/> was verified via the OTP flow
    /// (POST .../verification/request-code + confirm-code). Required unless the
    /// caller is authenticated (their account email is already confirmed).
    /// </summary>
    [MaxLength(2000)]
    public string? VerifiedEmailToken { get; set; }
}
