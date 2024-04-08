using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.Exceptions
{
    public class OutOfBoundsIndexException : Exception
    {
        public OutOfBoundsIndexException() : base() { }
        public OutOfBoundsIndexException(string message) : base(message) { }
        public OutOfBoundsIndexException(string message, Exception e) : base(message, e) { }

        private int index;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }
    }
}
