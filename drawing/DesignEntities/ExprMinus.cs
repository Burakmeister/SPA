using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class ExprMinus : Expr
    {
        public Expr? LeftExpr { get; set; }
        public Expr? RightExpr { get; set; }
        public ExprMinus(int lineNumber, Expr leftExpr, Expr rightExpr) : base(lineNumber)
        {
            this.LeftExpr = leftExpr;
            this.RightExpr = rightExpr;
        }
        public ExprMinus(int lineNumber, Expr leftExpr) : base(lineNumber)
        {
            this.LeftExpr = leftExpr;
        }
    }
}
