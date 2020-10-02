using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using TTLib;

namespace TTL.Launcher
{
    public partial class SetupInstallerWindow : Window
    {
        // instance of 'versionloadwindow'
        private readonly VersionLoadWindow versionLoadWindow = null;

        // version to install
        private readonly string version;

        // path of the version to install
        public string path;

        // text of destination textbox
        private string tBoxDestinationPath;

        public SetupInstallerWindow(VersionLoadWindow _versionLoadWindow, string _version, string _path)
        {
            InitializeComponent();

            // instantiates 'versionloadwindow' and sets it to owner
            versionLoadWindow = _versionLoadWindow;
            if (_versionLoadWindow.Visibility != Visibility.Hidden)
                this.Owner = _versionLoadWindow;

            // sets version to install
            version = _version;

            // sets path of the version to install
            path = _path;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // sets window title
            this.Title = $"Installing {version.Remove(0, 3)}";

            // sets textboxes
            tBoxDestinationPath = TBOXDestination.Text;
            TBOXFolderName.Text = version.Remove(0, 3);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult == true) { versionLoadWindow.path = path; }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void BTBrowse_Click(object sender, RoutedEventArgs e)
        {
            var cfd = new CommonOpenFileDialog
            {
                Title = "Select a destination where the game should be installed",
                IsFolderPicker = true,
                ShowHiddenItems = true,
                RestoreDirectory = true
            };

            // runs verifying routine
            MainWindow mainWindow = new MainWindow();
            mainWindow.Verify();
            
            // sets initial directory to the folder browser
            string lastPath;
            lastPath = (string)Config.Properties.Get("LastInstallPath");
            if (Directory.Exists(lastPath))
            {
                cfd.InitialDirectory = lastPath;
            }

            // opens folder browser
            if (cfd.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                // runs verifying routine
                mainWindow.Verify();

                // saves last path of the folder browser
                Config.Properties.Set("LastInstallPath", $@"{cfd.FileName}\".Replace(@"\\", @"\"));

                // checks if chosen folder is valid
                DriveInfo driveInfo = new DriveInfo(cfd.FileName);
                if (driveInfo.DriveType == DriveType.CDRom || driveInfo.DriveType == DriveType.Network)
                {
                    MessageBox.MessageBox.ShowDialog(this, "You can't install the game in this folder.\n\n" +
                        "Please choose another.", "Warning", "OK");
                    BTBrowse.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); return;
                }

                // sets textboxes
                tBoxDestinationPath = $@"{cfd.FileName}\";
                if (CBNameFolder.IsChecked == true) 
                {
                    TBOXDestination.Text = $@"{tBoxDestinationPath}\{version.Remove(0, 3)}\";
                }
                else 
                { 
                    TBOXDestination.Text = $@"{tBoxDestinationPath}\{TBOXFolderName.Text}\";
                }
            }
        }
        private void BTInstall_Click(object sender, RoutedEventArgs e)
        {
            // if destination folder doesn't exist
            if (TBOXDestination.Text[TBOXDestination.Text.Length - 2] == ' ') { TBOXDestination.Text = TBOXDestination.Text.Remove(TBOXDestination.Text.Length - 2, 1); }
            if (!Directory.Exists(TBOXDestination.Text))
            {
                // ask wether or not directory should be created
                if (MessageBox.MessageBox.ShowDialog(this, $"The directory '{TBOXDestination.Text}' doesn't exist.\n\n" +
                    "Do you want to create it?", "Warning", "YesNo") == true)
                {
                    // creates the destination folder
                    Directory.CreateDirectory(TBOXDestination.Text); 
                }
                else
                {
                    this.DialogResult = false; return;
                }
            }

            // if destination folder is not empty
            try
            {
                string[] directories = Directory.GetDirectories(TBOXDestination.Text);
                if (directories.Length > 0)
                {
                    // ask wether to install anyway or not
                    if (MessageBox.MessageBox.ShowDialog(this, $"The directory '{TBOXDestination.Text}' is not empty.\n\n" +
                        "Do you want to install anyway?", "Warning", "YesNo") == false)
                    {
                        return;
                    }
                }
            }
            catch { MessageBox.MessageBox.ShowDialog(this, $"Folder path is invalid. Please try again.", "Warning", "OK"); this.DialogResult = false; return; }

            // deactivates browse && install buttons
            BTBrowse.IsEnabled = false;
            BTInstall.IsEnabled = false;

            // starts installing
            InstallerWindow installLoadingWindow = new InstallerWindow(this, version, path, TBOXDestination.Text);
            if (installLoadingWindow.ShowDialog() == true) { this.DialogResult = true; return; }
            DialogResult = false;
        }
        private void BTCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            // makes folder name textbox writeable or not
            if (CBNameFolder.IsChecked == true)
            {
                TBOXFolderName.IsReadOnly = true;
                TBOXFolderName.Text = version.Remove(0, 3);
            }
            else if (CBNameFolder.IsChecked == false)
            {
                TBOXFolderName.IsReadOnly = false;
                TBOXFolderName.Clear();
            }
        }
        private void TBOXFolderName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TBOXFolderName.Text.Length > 0)
            {
                char[] invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
                for (int i = 0; i < invalidChars.Length; i++)
                {
                    if (TBOXFolderName.Text[0] == invalidChars[i])
                    {
                        TBOXFolderName.Clear(); break;
                    }
                }
            }

            if (CBNameFolder.IsChecked == true) 
            {
                TBOXDestination.Text = $@"{tBoxDestinationPath}\{version.Remove(0, 3)}\";
            }
            else 
            {
                if (TBOXFolderName.Text.Length > 0)
                {
                    if (TBOXFolderName.Text[0] == ' ') { TBOXFolderName.Text = TBOXFolderName.Text.Remove(0, 1); }
                }
                
                TBOXDestination.Text = $@"{tBoxDestinationPath}{TBOXFolderName.Text}\";
            }
        }
        private void TBOXDestination_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TBOXDestination.Text.Contains(@"\\")) { TBOXDestination.Text = TBOXDestination.Text.Replace(@"\\", @"\"); }
        }
    }
}
