using System.IO;
using System.Windows;
using System.Threading.Tasks;

namespace TTL.Launcher
{
    public partial class InstallerWindow : Window
    {
        // instance of 'installwindow'
        private readonly SetupInstallerWindow installWindow;

        // version to install
        private readonly string version;

        // game location
        private readonly string path;

        // destination folder 
        private readonly string destinationPath;

        public InstallerWindow(SetupInstallerWindow _installWindow, string _version, string _path, string _destinationPath)
        {
            InitializeComponent();

            // isntantiates 'installwindw' and sets it to owner
            installWindow = _installWindow;
            this.Owner = _installWindow;

            // sets version to install
            version = _version;

            // sets game location path
            path = _path;

            // sets destination path
            destinationPath = _destinationPath;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"Installing {version.Remove(0, 3)}";
            await InstallGameAsync(path, destinationPath);
            if (this.DialogResult != false) { this.DialogResult = true; }
        }

        private async Task InstallGameAsync(string gamePath, string destinationPath)
        {
            await Task.Run(() => InstallGame(gamePath, destinationPath));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult == true) { installWindow.path = destinationPath; }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void InstallGame(string gamePath, string destinationPath)
        {
            string[] files = Directory.GetFiles(gamePath, "*.*", SearchOption.AllDirectories);
            string[] directories = Directory.GetDirectories(gamePath, "*.*", SearchOption.AllDirectories);
            this.Dispatcher.Invoke(() => { PBLoading.Maximum = files.Length + directories.Length; });

            try
            {
                // creates directories
                foreach (string dir in directories)
                {
                    this.Dispatcher.Invoke(() => { PBLoading.Value++; });
                    if (Directory.Exists($"{destinationPath}{dir.Remove(0, 4)}")) { continue; }
                    Directory.CreateDirectory($"{destinationPath}{dir.Remove(0, 4)}");
                }

                // copies files
                foreach (string file in files)
                {
                    this.Dispatcher.Invoke(() => 
                    { 
                        PBLoading.Value++;
                        double percentage = PBLoading.Value / PBLoading.Maximum * 100;
                        TBPercentage.Text = $"{ percentage:0.00}%"; 
                    });

                    // skips fake files
                    if (file.ToLower().Contains("tt1.cnt")
                        || file.ToLower().Contains("tt2.cnt")
                        || file.ToLower().Contains("tt3.cnt")
                        || file.ToLower().Contains("tt4.cnt")) { continue; }

                    // copies files
                    if (File.Exists($"{destinationPath}{file.Remove(0, 4)}")) { continue; }
                    File.Copy(file, $"{destinationPath}{file.Remove(0, 4)}");
                }
            }
            catch 
            {
                this.Dispatcher.Invoke(() => 
                {
                    // gives error message
                    MessageBox.MessageBox.ShowDialog(this, "Oups, an error occured. Installation could not finish.\n\n" +
                        "Please try again.", "Warning", "OK");

                    // deletes already installed files
                    // TODO deleting files after canceling installation

                    // cancels installation
                    this.DialogResult = false;
                });
            }

            // TODO CHECK INDEO DRIVERS 
        }
    }
}
