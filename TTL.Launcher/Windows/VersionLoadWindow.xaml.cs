using System.IO;
using System.Windows;
using TTLib;
using TTL.CntLib;

namespace TTL.Launcher
{
    public partial class VersionLoadWindow : Window
    {
        // instance of 'mainwindow'
        private readonly MainWindow mainWindow;

        // path of selected folder
        public string path = string.Empty;

        // executable name of 'V8.5.1' or above
        private readonly string DXExecutable = "MaiD3Dvr.exe";

        // executable name of 'V8.1.0' or above
        private readonly string GLExecutable = "MaiDFXvd.exe";

        // executable name of retail version
        private readonly string RTExecutable = "TonicTrouble.exe";

        // repair flag to fix versions if corrupted
        private readonly string repairVersion = null;

        // window closing behaviour flag
        private bool closeFlag = false;

        public VersionLoadWindow(MainWindow _mainWindow, string _path, string _repairVersion)
        {
            InitializeComponent();

            // instantiates 'mainwindow' and sets it to owner
            mainWindow = _mainWindow;
            this.Owner = _mainWindow;

            // sets path of selected folder
            path = _path;

            // sets repair flag
            repairVersion = _repairVersion;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // runs verifying routine
            mainWindow.Verify();

            // special edition
            if (File.Exists($"{path}{DXExecutable}")
                || File.Exists($"{path}{GLExecutable}")
                || File.Exists($"{path}{RTExecutable}"))
            {
                // checks if languages folder exists
                string languagePath = $@"{path}GAMEDATA\WORLD\SOUND\";
                if (!Directory.Exists(languagePath))
                {
                    // gives error message
                    MessageBox.MessageBox.ShowDialog(this, $@"Missing folder: '{path}GAMEDATA\WORLD\SOUND\' could not be found.", "Warning", "OK");
                    closeFlag = true; this.Close(); return;
                }

                // checks whether languages inside languages folder are found or not
                string[] language = Game.GetLanguages(languagePath);
                if (language.Length == 0)
                {
                    // gives error message
                    MessageBox.MessageBox.ShowDialog(this, "Could not recognize any languages. \n\nAre you missing some files?", "Warning", "OK");
                    closeFlag = true; this.Close(); return;
                }

                string version = string.Empty;

                // if is V8.1.0
                if (File.Exists($"{path}{GLExecutable}")) { version = "SE-V8.1.0"; }

                // detects special edition
                else if (File.Exists($"{path}{DXExecutable}")) { version = $"SE-{await Game.Executable.GetVersionAsync($"{path}{DXExecutable}", 0)}"; }

                // detects retail version
                else if (File.Exists($"{path}{RTExecutable}")) { version = $"RT-{await Game.Executable.GetVersionAsync($"{path}{RTExecutable}", 1)}"; }

                if (version.ToLower() == "rt-")
                {
                    switch (language[0].ToLower())
                    {
                        default:
                            {
                                // gives error message
                                MessageBox.MessageBox.ShowDialog(this, "Could not recognize any languages. \n\nAre you missing some files?", "Warning", "OK");
                                closeFlag = true; this.Close(); return;
                            }
                        case "english": { version = "RT-Retail Master English V3"; break; }
                        case "french": { version = "RT-Retail Master V5"; break; }
                        case "german": { version = "RT-Retail Master German V3"; break; }
                        case "italian": { version = "RT-Review English (ITA)"; break; }
                        case "spanish": { version = "RT-Review English (ESP)"; break; }
                    }
                }
                else if (version.ToLower() == "rt-review english")
                {
                    switch (language[0].ToLower())
                    {
                        default:
                            {
                                // gives error message
                                MessageBox.MessageBox.ShowDialog(this, "Could not recognize any supported languages. \n\nAre you missing some files?", "Warning", "OK");
                                closeFlag = true; this.Close(); return;
                            }
                        case "italian": { version = "RT-Review English (ITA)"; break; }
                        case "spanish": { version = "RT-Review English (ESP)"; break; }
                    }
                }

                DriveInfo driveInfo = new DriveInfo(path);

                // if version already exists
                if (Config.Version.Exists(version) || (repairVersion != null && Config.Version.Exists(repairVersion)))
                {
                    // if repair flag is set
                    if (repairVersion != null)
                    {
                        // if is correct version
                        if (version.ToLower() == repairVersion.ToLower())
                        {
                            // if game is on a disc
                            if (driveInfo.DriveType == DriveType.CDRom)
                            {
                                // gives error message
                                MessageBox.MessageBox.ShowDialog(this, "The selected folder is not accessible. \n\n" +
                                    "Please choose another.", "Warning", "OK");
                                closeFlag = true; this.Close(); return;
                            }

                            // if everything is O.K then repair version
                            Config.Version.Properties.Set(version, "Path", path);
                            Config.Version.Properties.Set(version, "Language", language[0]);

                            MessageBox.MessageBox.ShowDialog(this, $"Successfully repaired {version.Remove(0, 3)}.", "Success", "OK");
                            closeFlag = true; this.Close(); return;
                        }
                        else
                        {
                            // gives error message
                            MessageBox.MessageBox.ShowDialog(this, "You have selected the wrong version. \n\n" +
                                $"Version to repair: {repairVersion.Remove(0, 3)}\n" +
                                $"Version in selected folder: {version.Remove(0, 3)}\n\n" +
                                "Please select the correct version.", "Warning", "OK");
                            closeFlag = true; this.Close(); return;
                        }
                    }
                    else
                    {
                        // gives error message
                        MessageBox.MessageBox.ShowDialog(this, $"Version {version.Remove(0, 3)} is already in your library.", "Information", "OK");
                        closeFlag = true; this.Close(); return;
                    }
                }

                // if version to repair got deleted
                else if (repairVersion != null && !Config.Version.Exists(repairVersion))
                {
                    // gives error message
                    MessageBox.MessageBox.ShowDialog(this, $"Could not repair {version.Remove(0, 3)} since it doesn't exist anymore.", "Warning", "OK");
                    closeFlag = true; this.Close(); return;
                }

                // if game is on a disc
                if (driveInfo.DriveType == DriveType.CDRom)
                {
                    SetupInstallerWindow setupInstallerWindow = new SetupInstallerWindow(this, version, path);
                    if (setupInstallerWindow.ShowDialog() == false) 
                    {
                        // sends message for cancelation
                        MessageBox.MessageBox.ShowDialog(this, "Installation has been canceled.", "Information", "OK");
                        closeFlag = true; this.Close(); return;
                    }
                }

                // checks indeo drivers
                if (!Game.IndeoDriversInstalled()) { Game.InstallIndeoDrivers(CntLib.Properties.Resources.ir32_32, CntLib.Properties.Resources.ir41_32); }

                // patches executable (retail version)
                if (version.ToLower().Contains("rt-"))
                {
                    if (File.Exists($"{path}{RTExecutable}") && !File.Exists($"{path}{RTExecutable}_ttl.bak"))
                    {
                        File.Move($"{path}{RTExecutable}", $"{path}{RTExecutable}_ttl.bak");
                    }
                    else if(!File.Exists($"{path}{RTExecutable}"))
                    {
                        // gives error message
                        MessageBox.MessageBox.ShowDialog(this, "Game could not be found. \n\nAre you missing some files?", "Warning", "OK");
                        closeFlag = true; this.Close(); return;
                    }
                    switch (version.ToLower())
                    {
                        case "rt-retail master english v3": { File.WriteAllBytes($"{path}{RTExecutable}", CntLib.Properties.Resources.TonicTroubleEN); break; }
                        case "rt-retail master v5": { File.WriteAllBytes($"{path}{RTExecutable}", CntLib.Properties.Resources.TonicTroubleFR); break; }
                        case "rt-retail master german v3": { File.WriteAllBytes($"{path}{RTExecutable}", CntLib.Properties.Resources.TonicTroubleGER); break; }
                        case "rt-review english (ita)": { File.WriteAllBytes($"{path}{RTExecutable}", CntLib.Properties.Resources.TonicTroubleITA); break; }
                        case "rt-review english (esp)": { File.WriteAllBytes($"{path}{RTExecutable}", CntLib.Properties.Resources.TonicTroubleESP); break; }
                    }
                }

                // installs dgvoodoo files
                if (version != "SE-V8.1.0") { DGVoodoo.Install(path, CntLib.Properties.Resources.D3D8, CntLib.Properties.Resources.D3DImm, CntLib.Properties.Resources.DDraw, null); }
                else { DGVoodoo.Install(path, null, null, null, CntLib.Properties.Resources.Glide2x); }

                // checks & installs controller support
                if (!File.Exists($"{path}dinput.dll")) { File.WriteAllBytes($"{path}dinput.dll", CntLib.Properties.Resources.dinput); }

                // if everything is O.K then add version
                Config.Version.Add(version, path, language[0]);

                MessageBox.MessageBox.ShowDialog(this, $"Version {version.Remove(0, 3)} got successfully imported to your library.", "Success", "OK");
                closeFlag = true; this.DialogResult = true;
            }

            // if game is not found
            else
            {
                // gives error message
                MessageBox.MessageBox.ShowDialog(this, "The game could not be found. \n\nAre you missing some files?", "Warning", "OK");
                closeFlag = true; this.Close();
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!closeFlag) { e.Cancel = true; }
            this.DialogResult = true;
        }
    }
}
