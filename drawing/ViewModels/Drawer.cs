using SPA.QueryProcessor;
using System;
using System.ComponentModel;
using SPA.DesignEntities;
using SPA.Parsing;
using SPA.PKB;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

namespace SPA.ViewModels
{
    public class Drawer : IDrawer, INotifyPropertyChanged
    {
        private QueryProcessorExec _processor;
        public ICommand executeQueryCmnd => new Command(executeQuery!);
        public ICommand DrawNextProcedureCommand => throw new NotImplementedException();
        public ICommand DrawPrevProcedureCommand => throw new NotImplementedException();
        public string Code { get; set; } = "";
        public string ResultQuery { get; set; } = "";
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
                relationFinder = new RelationFinder(procedures, pkb);
                relationFinder.FillPKB();
                MessageBox.Show("Parsing execute");
                currentIndex = 0;
                //DrawerAST.DrawTree(procedures[currentIndex] as Procedure);
                //MessageBox.Show("Tree Drawed");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            try
            {
                string query = _codeQuery;
                _processor = new QueryProcessorExec(query, pkb);
                List<string> synonyms = _processor.Query.Synonyms;
                Dictionary<string, List<string>> results = _processor.Query.Result;

                //Dictionary<string, List<string>>.ValueCollection values = results.Values;
                ResultQuery = "";
                foreach (string synonym in synonyms)
                {
                    ResultQuery += synonym + " ";
                    foreach (string result in results[synonym])
                    {
                        ResultQuery += result + ' ';
                    }
                    ResultQuery += "\n";
                }
                OnPropertyChanged(nameof(ResultQuery));
                MessageBox.Show("Query executed");
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void CompleteProceduresList()
        {
            procedures.Clear();
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
                        procedures.Add(first.NextProcedure);
                        first = first.NextProcedure;
                    }

                }
            }

        }
    }
}
