using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SPA.ViewModels
{
    public interface IDrawer
    {
        ICommand executeQueryCmnd { get; }
        ICommand ParseCommand { get; }
        ICommand DrawNextProcedureCommand { get; }
        ICommand DrawPrevProcedureCommand { get; }
    }
}
