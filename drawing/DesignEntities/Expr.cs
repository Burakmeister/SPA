﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public abstract class Expr : Statement
    {
        protected Expr(int lineNumber) : base(lineNumber)
        {
        }
    }
}
