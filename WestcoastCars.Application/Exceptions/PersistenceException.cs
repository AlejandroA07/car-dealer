using System;

namespace WestcoastCars.Application.Exceptions;

public class PersistenceException(string message) : Exception(message)
{
}
