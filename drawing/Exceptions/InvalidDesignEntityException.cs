using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.Exceptions
{
    public class InvalidDesignEntityException : Exception
    {
        public InvalidDesignEntityException()
           : base("Niepoprawne design entity")
        {
        }

        public InvalidDesignEntityException(string message)
            : base($"Niepoprawne design entity: {message}")
        {
        }

        public InvalidDesignEntityException(string message, Exception inner)
            : base($"Niepoprawne design entity: {message}", inner)
        {
        }
    }
}
