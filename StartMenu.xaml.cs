using System.Windows;
using System.Windows.Controls;

namespace Pentagon
{
    public partial class StartMenu : Page
    {
        public StartMenu()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            bool enableBarriers = EnBariersCheckBox.IsChecked == true;  // статус чек-боксу (містить галочку чи ні)
            if (Application.Current.MainWindow is MainWindow mw)
                mw.MainFrame.Navigate(new GamePage(enableBarriers));
        }
    }
}
