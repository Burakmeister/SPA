using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class ExprPlus : Expr
    {
        public Expr? LeftExpr { get; set; }
        public Expr? RightExpr { get; set; }
        public ExprPlus(int lineNumber) : base(lineNumber)
        {
        }
    }
}
