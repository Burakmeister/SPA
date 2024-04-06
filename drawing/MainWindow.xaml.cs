using drawing.ViewModels;
using System.Windows;

namespace drawing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IIoC _ioc;
        public MainWindow()
        {
            InitializeComponent();

            _ioc = new IoC();
            _ioc.RegisterAsSingleton<IIoC, IoC>(_ioc)
                .RegisterAsTransient<IMainWindowVM, MainWindowVM>()
                .RegisterAsTransient<IDrawer, Drawer>();

            this.DataContext = _ioc.Inject<IMainWindowVM>();
        }
    }
}
