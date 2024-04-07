using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using SPA.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SPA.ViewModels
{
    public class Drawer : ViewModel, IDrawer
    {
        public Drawer() {
            OpenFileCommand = new RelayCommand(OpenFile);
        }

        private TextDocument _document = null;
        public TextDocument Document
        {
            get { return this._document; }
            set
            {
                if (this._document != value)
                {
                    this._document = value;
                    OnPropertyChanged("Document");
                   
                }
            }
        }

        public ICommand OpenFileCommand { get; private set; }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileContent = File.ReadAllText(filePath);
                Document = new TextDocument(fileContent);
            }
        }
    }
}
