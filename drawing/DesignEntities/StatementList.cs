using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class StatementList
    {
        private Statement FirstStatement { get; set; }
        public StatementList(Statement FirstStatement)
        {
            this.FirstStatement = FirstStatement;
        }
    }
}
