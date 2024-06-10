using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class ExprTimes : Expr
    {
        public Expr? LeftExpr { get; set; }
        public Expr? RightExpr { get; set; }
        public ExprTimes(int lineNumber, Expr leftExpr, Expr rightExpr) : base(lineNumber)
        {
            LeftExpr = leftExpr;
            RightExpr = rightExpr;
        }
        public ExprTimes(int lineNumber, Expr leftExpr) : base(lineNumber)
        {
            LeftExpr = leftExpr;
        }
    }
}
