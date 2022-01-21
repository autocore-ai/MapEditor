using System.Windows;

namespace MapEditor.WpfShell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel m_ViewModel;

        public MainWindow()
        {
            InitializeComponent();
            m_ViewModel = new MainViewModel();
            DataContext = m_ViewModel;
            Closed += (o, e) => 
            {
                if (m_ViewModel != null) 
                {
                    m_ViewModel = null;
                }
                DataContext = null;
            };
        }
    }
}
