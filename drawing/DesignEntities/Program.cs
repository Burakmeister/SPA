using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Program
    {
        private string Name { get; set;}
        private Procedure FirstProcedure { get; set; }
        public Program(string name, Procedure procedure) {
            this.Name = name;
            this.FirstProcedure = procedure;
        }
    }
}
