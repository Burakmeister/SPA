﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Procedure : TNode
    {
        public string ProcName { get; private set; }
        public int LineStartNumber { get;  set; }
        public int LineEndNumber { get; set; }
        public StatementList? StatementList { get; set; }
        public Procedure? NextProcedure { get; set; } = null;

        public Procedure(string procName, int lineStartNumber)
        {
            ProcName = procName;
            LineStartNumber = lineStartNumber;
        }
    }
}
