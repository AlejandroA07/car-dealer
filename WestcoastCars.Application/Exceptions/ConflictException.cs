using System;

namespace WestcoastCars.Application.Exceptions;

public class ConflictException(string message) : Exception(message)
{
}
