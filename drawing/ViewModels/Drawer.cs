using SPA.QueryProcessor;
using System;
using System.ComponentModel;
using SPA.DesignEntities;
using SPA.Parsing;
using SPA.PKB;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SPA.ViewModels
{
    public class Drawer : IDrawer, INotifyPropertyChanged
    {
        private QueryProcessorExec _processor;
        public ICommand executeQueryCmnd => new Command(executeQuery!);
        public ICommand DrawNextProcedureCommand => throw new NotImplementedException();
        public ICommand DrawPrevProcedureCommand => throw new NotImplementedException();
        public string Code { get; set; } = "";
        private ArrayList procedures = new ArrayList();
        private int currentIndex = 0;
        private IPkb? pkb;
        private RelationFinder relationFinder;
        private IDrawerAST DrawerAST= new DrawerAST();

        public IParser Parser { get; } = new Parser();
        public ICommand ParseCommand => new Command((param) =>
        {
            try
            {
                int numOfLines;
                numOfLines = Parser.Parse(Code);
                CompleteProceduresList();
                pkb = Pkb.GetInstance(numOfLines);
                pkb.ClearPkb();
                relationFinder = new RelationFinder(procedures, pkb);
                relationFinder.FillPKB();
                currentIndex = 0;
                DrawerAST.DrawTree(procedures[currentIndex] as Procedure);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        });
        private string _codeQuery;
        public string codeQuery
        {
            get { return _codeQuery; }
            set
            {
                if (_codeQuery != value)
                {
                    _codeQuery = value;
                    OnPropertyChanged(nameof(codeQuery));
                }
            }   
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void executeQuery(object param)
        {
            string query = _codeQuery;
            _processor = new QueryProcessorExec(query, pkb);

            MessageBox.Show("Query executed");
        }


        private void CompleteProceduresList()
        {
            Program program;
            if (Parser.GetProgram() != null)
            {
                program = Parser.GetProgram()!;
                if(program.FirstProcedure != null)
                {
                    Procedure first = program.FirstProcedure;
                    procedures.Add(first);
                    while (first!.NextProcedure != null)
                    {
                        procedures.Add(first.ProcName);
                        first = first.NextProcedure;
                    }
                }
            }

        }
    }
}
