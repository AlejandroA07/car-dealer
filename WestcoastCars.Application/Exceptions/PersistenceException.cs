using System;

namespace WestcoastCars.Application.Exceptions
{
    public class PersistenceException : Exception
    {
        public PersistenceException(string message) : base(message)
        {
        }
    }
}
