using System.Windows;
using TTLib;

namespace TTL.Launcher
{
    public partial class SettingsWindow : Window
    {
        // instance of 'mainwindow'
        private readonly MainWindow mainWindow = null;

        public SettingsWindow(MainWindow _mainWindow)
        {
            InitializeComponent();

            // instantiate 'mainwindow' and set it to owner
            mainWindow = _mainWindow;
            this.Owner = _mainWindow;

            TBVersion.Text = $"Launcher Version: {Properties._LauncherVersion}";
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // read settings and sets radio buttons
            if ((int)Config.Properties.Get("MinimizeLauncher") == 1) { RBMinimizeYes.IsChecked = true; }
            else { RBMinimizeNo.IsChecked = true; }
            if ((int)Config.Properties.Get("ShowDiscordStatus") == 1) { RBDiscordYes.IsChecked = true; }
            else { RBDiscordNo.IsChecked = true; }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void CBReset_Click(object sender, RoutedEventArgs e)
        {
            // activates or deactivates 'reset' button
            if (CBReset.IsChecked ?? false) { BTReset.IsEnabled = true; }
            else { BTReset.IsEnabled = false; }
        }
        private void BTReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.MessageBox.ShowDialog(this, "This action will reset your library and removes all settings and can not be undone!" +
                "\n\nAre you sure to continue?", "Warning", "YesNo") == true)
            {
                // removes everything
                if (Config.Exists()) { Config.Remove(); }

                // uninstalls indeo drivers
                if (Game.IndeoDriversInstalled()) { Game.UninstallIndeoDrivers(); }

                // closes application
                mainWindow.Close();
            }
        }
        private void BTSave_Click(object sender, RoutedEventArgs e)
        {
            // runs verifying routine
            mainWindow.Verify();

            // saves new settings
            Config.Properties.Set("MinimizeLauncher", (bool)RBMinimizeYes.IsChecked);
            Config.Properties.Set("ShowDiscordStatus", (bool)RBDiscordYes.IsChecked);

            // closes settings window
            this.Close();
        }
        private void BTCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}