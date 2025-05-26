using System.Windows;
namespace Pentagon
{

    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {          

            InitializeComponent();
            MainFrame.Navigate(new StartMenu());
        }        

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        { 
            MessageBoxResult result = MessageBox.Show(          // очікує підтвердження від користувача
                "Are you sure you want to exit the program?",
                "Exit Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();

        }
    }
}