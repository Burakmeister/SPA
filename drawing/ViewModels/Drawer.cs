using SPA.QueryProcessor;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace drawing.ViewModels
{
    public class Drawer : IDrawer, INotifyPropertyChanged
    {
        private QueryProcessor _processor;
        public ICommand executeQueryCmnd => new Command(executeQuery!);

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
            _processor = new QueryProcessor(query);

            MessageBox.Show("Query executed");
        }
    }
}
