using SPA.parser;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SPA.ViewModels
{
    public class Drawer : IDrawer
    {
        public string Code { get; set; } = "";
        public Parser Parser { get; } = new();
        public ICommand ParseCommand => new Command((param) =>
        {
            Parser.Parse(Code);
        });
    }
}
