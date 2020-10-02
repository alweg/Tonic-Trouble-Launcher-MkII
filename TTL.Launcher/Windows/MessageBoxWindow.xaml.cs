using System.Windows;

namespace TTL.Launcher.Windows
{
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow(Window mbo, string mbt, string mbc, string mbtn)
        {
            InitializeComponent();

            this.Owner = mbo;
            this.Title = mbc;
            TBText.Text = mbt;

            switch (mbtn)
            {
                case "YesNo": { BTLeft.Content = "Yes!"; BTRight.Content = "No"; break; }
                case "OK": { BTLeft.Content = "Okay"; BTLeft.Margin = new Thickness(0,0,0,5); BTRight.Visibility = Visibility.Hidden; break; }
                case "OKCancel": { BTLeft.Content = "Okay"; BTRight.Content = "Cancel"; break; }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void BTLeft_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void BTRight_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
