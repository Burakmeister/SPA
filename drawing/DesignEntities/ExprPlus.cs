using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    class ExprPlus : Expr
    {
        private Expr LeftExpr { get; }
        private Expr RightExpr { get; }
        public ExprPlus(int lineNumber, Expr leftExpr, Expr rightExpr) : base(lineNumber)
        {
            LeftExpr = leftExpr;
            RightExpr = rightExpr;
        }
    }
}
