using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace drawing.ViewModels
{
    public class MainWindowVM : IMainWindowVM
    {

        public IDrawer Drawer { get; set; }

        public MainWindowVM(IIoC ioc)
        {
            Drawer = ioc.Inject<IDrawer>()
                ?? throw new NullReferenceException();
        }
    }
}
