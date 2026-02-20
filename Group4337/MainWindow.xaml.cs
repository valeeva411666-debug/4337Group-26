using System.Windows;

namespace Group4337
{
    public partial class MainWindow : Window
    {
        public MainWindow()
            => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Valeeva_4337 window = new Valeeva_4337();
            window.Show();  
        }
    }
}