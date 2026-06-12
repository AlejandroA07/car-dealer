using System;

namespace WestcoastCars.Application.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
}
