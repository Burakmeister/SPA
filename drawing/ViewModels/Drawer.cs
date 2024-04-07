using ICSharpCode.AvalonEdit.Document;
using SPA.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SPA.ViewModels
{
    public class Drawer : ViewModel, IDrawer
    {
        private string _code="";
        public TextDocument Document
        {
            get { return _code; }
            set
            {
                _code = value; 
                OnPropertyChanged("Document");
            }
        }
    }
}
