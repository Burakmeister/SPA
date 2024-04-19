using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.Exceptions
{
    public class InvalidRelationshipTypeException : Exception
    {
        public InvalidRelationshipTypeException()
           : base("Niepoprawny typ relacji")
        {
        }

        public InvalidRelationshipTypeException(string message)
            : base($"Niepoprawny typ relacji: {message}")
        {
        }

        public InvalidRelationshipTypeException(string message, Exception inner)
            : base($"Niepoprawny typ relacji: {message}", inner)
        {
        }
    }
}
