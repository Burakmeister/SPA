using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.Exceptions
{
    public class InvalidAttributeNameException : Exception
    {
        public InvalidAttributeNameException()
            : base("Niepoprawna nazwa atrybutu")
        {
        }

        public InvalidAttributeNameException(string message)
            : base($"Niepoprawna nazwa atrybutu: {message}")
        {
        }

        public InvalidAttributeNameException(string message, Exception inner)
            : base($"Niepoprawna nazwa atrybutu: {message}", inner)
        {
        }
    }
}
