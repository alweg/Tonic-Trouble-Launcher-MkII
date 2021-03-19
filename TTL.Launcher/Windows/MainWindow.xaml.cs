using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using TTLib;

namespace TTL.Launcher
{
    public partial class MainWindow : Window
    {
        // repair flag to fix versions if corrupted
        private string repairVersion = null;

        // timer to count the playtime
        private readonly Timer totalPlaytime = new Timer(2000);

        // timer tick to count the playtime
        private int totalPlaytimeTick;

        public MainWindow()
        {
            InitializeComponent();

            DiscordRPC.RPCApp.RPC.Server.RPCServer.DoWork += DiscordRPC.RPCApp.RPC.Server.Update;
            totalPlaytime.Elapsed += TotalPlaytime_Tick;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MainWindow_UnhandledException);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /* checks if launcher is obsolete and dgvoodoo needs an update
            if (Config.Exists())
            {
                if (Config.IsObsolete())
                {
                    string[] versions = Config.Version.GetAll();
                    for (int i = 0; i < versions.Length; i++)
                    {
                        Config.Version.Properties.Set(versions[i], "Update", 1);
                    }
                    Config.Update();
                }
            }
            else { Config.Create(); }*/

            // starts or stops discord rpc server
            if ((int)Config.Properties.Get("ShowDiscordStatus") == 1)
            {
                DiscordRPC.RPCApp.RPC.Server.Start();
            }

            // checks whether game is running or not
            bool gameRuns = false;
            foreach (Process TT in Process.GetProcessesByName("MaiDFXvd")) { if (TT.Id > 0) { gameRuns = true; } }
            foreach (Process TT in Process.GetProcessesByName("MaiD3Dvr")) { if (TT.Id > 0) { gameRuns = true; } }
            foreach (Process TT in Process.GetProcessesByName("TonicTrouble")) { if (TT.Id > 0) { gameRuns = true; } }

            // if game does not run reset last time
            if (!gameRuns)
            {
                Config.Properties.Set("LastDiscordTimeHours", 0);
                Config.Properties.Set("LastDiscordTimeMinutes", 0);
                Config.Properties.Set("LastDiscordTimeSeconds", 0);
            }

            // updates library
            UpdateLibrary();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // if a backup of the ubi.ini file exists
            if (UbiConfig.BackupExists())
            {
                // deletes current ubi.ini
                UbiConfig.Delete();

                // restores backup of the original ubi.ini
                UbiConfig.Restore();
            }

            // if no backup of the ubi.ini file exists
            else
            {
                // deletes ubisoft folder
                if (UbiConfig.Directory.Exists()) { UbiConfig.Directory.Delete(); }
            }

            // stops discord rpc server if it is running
            if (DiscordRPC.RPCApp.RPC.Server.IsRunning()) { DiscordRPC.RPCApp.RPC.Server.Stop(); }

            // checks whether game is running or not
            bool gameRuns = false;
            foreach (Process TT in Process.GetProcessesByName("MaiDFXvd")) { if (TT.Id > 0) { gameRuns = true; } }
            foreach (Process TT in Process.GetProcessesByName("MaiD3Dvr")) { if (TT.Id > 0) { gameRuns = true; } }
            foreach (Process TT in Process.GetProcessesByName("TonicTrouble")) { if (TT.Id > 0) { gameRuns = true; } }

            // if game runs save time to reset elapsed time after restart
            if (gameRuns)
            {
                TimeSpan elapsedTime = new TimeSpan((int)Config.Properties.Get("LastDiscordTimeHours"), (int)Config.Properties.Get("LastDiscordTimeMinutes"), (int)Config.Properties.Get("LastDiscordTimeSeconds"));
                DateTime lastTime = DateTime.UtcNow - elapsedTime;
                Config.Properties.Set("LastDiscordTimeHours", lastTime.Hour);
                Config.Properties.Set("LastDiscordTimeMinutes", lastTime.Minute);
                Config.Properties.Set("LastDiscordTimeSeconds", lastTime.Second);
            }
            else
            {
                Config.Properties.Set("LastDiscordTimeHours", 0);
                Config.Properties.Set("LastDiscordTimeMinutes", 0);
                Config.Properties.Set("LastDiscordTimeSeconds", 0);
            }
        }
        private void MainWindow_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            TimeSpan elapsedTime = new TimeSpan((int)Config.Properties.Get("LastDiscordTimeHours"), (int)Config.Properties.Get("LastDiscordTimeMinutes"), (int)Config.Properties.Get("LastDiscordTimeSeconds"));
            DateTime lastTime = DateTime.UtcNow - elapsedTime;
            Config.Properties.Set("LastDiscordTimeHours", lastTime.Hour);
            Config.Properties.Set("LastDiscordTimeMinutes", lastTime.Minute);
            Config.Properties.Set("LastDiscordTimeSeconds", lastTime.Second);

            Exception ee = (Exception)e.ExceptionObject;
            MessageBox.MessageBox.ShowDialog(this, $"Wouups, something went badly wrong. \n\nError: {ee.Message} \n\nLauncher closes.", "Error", "OK");
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            await CntLib.CntLib.InitializeAsync();
        }

        private void BTPlay_Click(object sender, RoutedEventArgs e)
        {
            // verifying routine
            Verify();

            // gets selected version
            string version = GetSelectedVersion();

            if ((string)BTPlay.Content == "Play")
            {
                if (Config.Version.Exists(version) && Config.Version.IsValid(version))
                {
                    // sets running version
                    Config.Properties.Set("RunningVersion", GetSelectedVersion());

                    // sets last time played
                    DateTime currentDate = DateTime.Now;
                    Config.Version.Properties.Set(version, "LastDay", currentDate.Day);
                    Config.Version.Properties.Set(version, "LastMonth", currentDate.Month);
                    Config.Version.Properties.Set(version, "LastYear", currentDate.Year);

                    // gets path of the version
                    string path = (string)Config.Version.Properties.Get(version, "Path");

                    // updates dgvoodoo config
                    if (!DGVoodoo.Config.Exists(path)) { DGVoodoo.Config.Create(path); }
                    UpdateDGVoodooConfig(version);

                    // special edition
                    if (version.ToLower().Contains("se-"))
                    {
                        // if is glide version
                        if (version.ToLower() == "se-v8.1.0")
                        {
                            // kills process if game is already running
                            foreach (Process TT in Process.GetProcessesByName("MaiDFXvd")) { try { TT.Kill(); } catch { } }

                            // starts the game
                            using (Process TT = new Process())
                            {
                                TT.StartInfo.WorkingDirectory = path;
                                TT.StartInfo.FileName = ("MaiDFXvd.exe");
                                TT.Start();
                            }
                            totalPlaytime.Start();
                        }
                        else
                        {
                            // kills process if game is already running
                            foreach (Process TT in Process.GetProcessesByName("MaiD3Dvr")) { try { TT.Kill(); } catch { } }
                            StartGame(0, version);
                        }
                    }

                    // retail version
                    else if (version.ToLower().Contains("rt-"))
                    {
                        // kills process if game is already running
                        foreach (Process TT in Process.GetProcessesByName("TonicTrouble")) { try { TT.Kill(); } catch { } }
                        StartGame(1, version);
                    }

                    // whether or not launcher should minimize after launching
                    if ((int)Config.Properties.Get("MinimizeLauncher") == 1) { this.WindowState = WindowState.Minimized; }
                    UpdateGameDetails();
                    BTPlay.Content = "Stop";
                }
                else { UpdateLibrary(); return; }
            }
            else if ((string)BTPlay.Content == "Stop")
            {
                foreach (Process TT in Process.GetProcessesByName("MaiDFXvd")) { try { TT.Kill(); } catch { } }
                foreach (Process TT in Process.GetProcessesByName("MaiD3Dvr")) { try { TT.Kill(); } catch { } }
                foreach (Process TT in Process.GetProcessesByName("TonicTrouble")) { try { TT.Kill(); } catch { } }

                BTPlay.Content = "Play";

                Config.Properties.Set("LastDiscordTimeHours", 0);
                Config.Properties.Set("LastDiscordTimeMinutes", 0);
                Config.Properties.Set("LastDiscordTimeSeconds", 0);
            }
            else if ((string)BTPlay.Content == "Fix (!)")
            {
                if (Config.Version.Exists(version))
                {
                    if (MessageBox.MessageBox.ShowDialog(this, $"{version.Remove(0, 3)} seems to be corrupted. Are you missing some files? \n\n" +
                        "Please select the game folder of this version to try to repair this version.", "Warning", "OK") == true)
                    {
                        repairVersion = version;
                        BTAddVersion.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                }

                Config.Properties.Set("LastDiscordTimeHours", 0);
                Config.Properties.Set("LastDiscordTimeMinutes", 0);
                Config.Properties.Set("LastDiscordTimeSeconds", 0);
            }
            else { UpdateLibrary(); return; }
        }
        private void BTQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BTRefreshLibrary_Click(object sender, RoutedEventArgs e)
        {
            UpdateLibrary();
        }
        private void BTRemove_Click(object sender, RoutedEventArgs e)
        {
            // runs verifying routine
            Verify(); 
            
            // if no versions exist
            if (!Config.Version.ExistsAny())
            { 
                UpdateLibrary(); return; 
            }

            // special edition
            string version = string.Empty;
            if (LBSE.SelectedIndex != -1)
            {
                // gets version of clicked item 
                var lbi = (ListBoxItem)LBSE.ItemContainerGenerator.ContainerFromIndex(LBSE.SelectedIndex);
                version = $"SE-{(string)lbi.Content}";
                if (version.Contains("(Warning)"))
                {
                    version = version.Replace("(Warning)", "");
                    version = version.Remove(version.Length - 1, 1);
                }
            }

            // retail version
            if (LBRT.SelectedIndex != -1)
            {
                // gets version of clicked item 
                var lbi = (ListBoxItem)LBRT.ItemContainerGenerator.ContainerFromIndex(LBRT.SelectedIndex);
                version = $"RT-{(string)lbi.Content}";
                if (version.Contains("(Warning)"))
                {
                    version = version.Replace("(Warning)", "");
                    version = version.Remove(version.Length - 1, 1);
                }
            }

            if (MessageBox.MessageBox.ShowDialog(this, "Are you sure to remove the selected version from your library?", "Warning", "YesNo") == true)
            {
                // if version exist then remove it
                if (Config.Version.Exists(version))
                {
                    Config.Version.Remove(version);
                }

                // updates library
                UpdateLibrary();
            }
        }
        private void BTWiki_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://1drv.ms/u/s!AsgcQQ-C4Q3CkYEl9u9Gc7WPh35Rfg");
        }
        private void BTDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://discord.gg/drfGJtA");
        }
        private void BTSRC_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.speedrun.com/tonic_trouble_pc_n64_gbc");
        }
        private void BTSettings_Click(object sender, RoutedEventArgs e)
        {
            // runs verifying routine
            Verify();

            // opens 'settingswindow'
            SettingsWindow sw = new SettingsWindow(this);
            sw.ShowDialog();

            // starts or stops discord rpc server
            if ((int)Config.Properties.Get("ShowDiscordStatus") == 1)
            {
                if (!DiscordRPC.RPCApp.RPC.Server.IsRunning()) { DiscordRPC.RPCApp.RPC.Server.Start(); }
            }
            else if((int)Config.Properties.Get("ShowDiscordStatus") == 0)
            {
                if (DiscordRPC.RPCApp.RPC.Server.IsRunning()) { DiscordRPC.RPCApp.RPC.Server.Stop(); }
            }
        }   
        private void BTAddVersion_Click(object sender, RoutedEventArgs e)
        {
            if (repairVersion == null)
            {
                if (MessageBox.MessageBox.ShowDialog(this, "Please select a folder where the game is located." +
                "\n\nNote: You can select a folder or a disc drive / mounted image.", "Information", "OKCancel") == false) { return; }
            }

            var cfd = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                ShowHiddenItems = true,
                RestoreDirectory = true
            };

            if (repairVersion != null) { cfd.Title = "Select the folder where the game is located"; }
            else if (repairVersion == null) { cfd.Title = "Select a folder or a disc / mounted image"; }

            // runs verifying routine
            Verify();

            // sets initial directory to the folder browser
            string lastPath;
            lastPath = (string)Config.Properties.Get("LastPath");
            if (Directory.Exists(lastPath))
            {
                cfd.InitialDirectory = lastPath;
            }

            // opens folder browser
            if (cfd.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                // runs verifying routine
                Verify();

                // saves last path of folder browser
                Config.Properties.Set("LastPath", $@"{cfd.FileName}\".Replace(@"\\", @"\"));

                // shows 'versionloadwindow'
                VersionLoadWindow versionLoadWindow;
                if (repairVersion == null) { versionLoadWindow = new VersionLoadWindow(this, $@"{cfd.FileName}\", null); }
                else { versionLoadWindow = new VersionLoadWindow(this, $@"{cfd.FileName}\", repairVersion); }

                // loads version
                versionLoadWindow.ShowDialog();
                UpdateLibrary();
            }
        }
        private void TotalPlaytime_Tick(object sender, ElapsedEventArgs e)
        {
            // updates playtime
            UpdateTotalPlaytime();
        }

        private async void BTGameSettings_Click(object sender, RoutedEventArgs e)
        {
            // runs verifying routine
            Verify();

            string version = string.Empty;
            this.Dispatcher.Invoke(() => 
            {
                // gets selected version
                version = GetSelectedVersion();
            });

            if (version == null
                || !Config.Version.Exists(version)
                || !Config.Version.IsValid(version))
            {
                UpdateLibrary(); return;
            }

            // opens 'gamesettingswindow'
            GameSettingsWindow gameSettingsWindow = new GameSettingsWindow(this, version);
            if (gameSettingsWindow.ShowDialog() == false) { UpdateLibrary(); }
            else { await UpdateGameDetailsAsync(); }
        }
        private async void LBSE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBSE.SelectedIndex != -1)
            {
                // makes only 1 item selectable at a time
                if (LBRT.SelectedIndex != -1) { LBRT.SelectedIndex = -1; }

                // updates game details
                await UpdateGameDetailsAsync();
            }
        }
        private async void LBRT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBRT.SelectedIndex != -1)
            {
                // makes only 1 item selectable at a time
                if (LBSE.SelectedIndex != -1) { LBSE.SelectedIndex = -1; }

                // updates game details
                await UpdateGameDetailsAsync();
            }
        }

        /// <summary> Runs a routine that verifies whether or not the launcher configuration exists 
        /// and / or is valid. Also deletes invalid versions and repairs existing versions if needed. </summary>
        public void Verify()
        {
            // checks if configuration exists
            if (!Config.Exists()) { Config.Create(); }
            if (!Config.IsValid()) { Config.Properties.ApplyDefaults(); }

            // deletes all not existing versions
            Config.Version.RemoveInvalidVersions();

            // repairs all versions if needed
            string[] versions = Config.Version.GetAll();
            for (int i = 0; i < versions.Length; i++)
            {
                Config.Version.Properties.Repair(versions[i]);
            }
        }

        private async void UpdateGameDetails()
        {
            // runs verifying routine
            Verify();

            await this.Dispatcher.Invoke( async () => 
            {
                // gets selected version
                string version = GetSelectedVersion();

                // checks if a game is running
                if ((string)Config.Properties.Get("RunningVersion") != version) { BTPlay.Content = "Play"; }
                else
                {
                    Process[] processSE = Process.GetProcessesByName("MaiDFXvd");
                    Process[] processSE_Glide = Process.GetProcessesByName("MaiD3Dvr");
                    Process[] processRT = Process.GetProcessesByName("TonicTrouble");

                    if (processSE.Length == 0
                        && processSE_Glide.Length == 0
                        && processRT.Length == 0) { BTPlay.Content = "Start"; }
                    else { BTPlay.Content = "Stop"; }
                }

                if (version != null && version.ToLower().Contains("se-"))
                {
                    // if version doesn't exist
                    if (!Config.Version.Exists(version))
                    {
                        // removes version and updates library
                        LBSE.Items.RemoveAt(LBSE.SelectedIndex);
                        LBSE.SelectedIndex = -1;
                        UpdateLibrary(); return;
                    }
                }
                else if (version != null && version.ToLower().Contains("rt-"))
                {
                    // if version doesn't exist
                    if (!Config.Version.Exists(version))
                    {
                        // removes version and updates library
                        LBRT.Items.RemoveAt(LBRT.SelectedIndex);
                        LBRT.SelectedIndex = -1;
                        UpdateLibrary(); return;
                    }
                }
                else if (version == null) { UpdateLibrary(); return; }

                // if version is invalid action is required
                if (!Config.Version.IsValid(version))
                {
                    var lbi = GetSelectedVersionItem();
                    if (lbi != null)
                    {
                        lbi.Content = $"{version.Remove(0, 3)} (Warning)";
                        lbi.Foreground = Brushes.Red;
                        ToggleGameDetails(true, true, true, false, false, true, true, true);
                    }
                }
                else
                {
                    ToggleGameDetails(true, true, false, false, true, true, true, true);
                }

                // sets location
                string path = (string)Config.Version.Properties.Get(version, "Path");
                if (path.ToLower() == "unset")
                {
                    path = $"{path[0].ToString().ToUpper()}{path.Substring(1).ToLower()}";
                }

                // gets total playtime
                int totalHour = (int)Config.Version.Properties.Get(version, "TotalHour");
                int totalMin = (int)Config.Version.Properties.Get(version, "TotalMin");
                TBGameDetailsPlayTime.Text = $"Total playtime: {totalHour} Hours {totalMin} Minutes";

                // gets last time played
                int lastDay = (int)Config.Version.Properties.Get(version, "LastDay");
                int lastMonth = (int)Config.Version.Properties.Get(version, "LastMonth");
                int lastYear = (int)Config.Version.Properties.Get(version, "LastYear");

                // calculates last time played
                int yearsAgo = 0;
                int monthsAgo = 0;
                int daysAgo = 0;
                if (lastDay > 0
                    || lastMonth > 0
                    || lastYear > 0)
                {
                    DateTime zeroTime = new DateTime(1, 1, 1);
                    DateTime lastDate = new DateTime(lastYear, lastMonth, lastDay);
                    TimeSpan timeSpan = DateTime.Now - lastDate;
                    yearsAgo = (zeroTime + timeSpan).Year - 1;
                    monthsAgo = (zeroTime + timeSpan).Month - 1;
                    daysAgo = (zeroTime + timeSpan).Day - 1;
                }

                // converts last month to string
                string lastMonthSTR = string.Empty;
                switch (lastMonth)
                {
                    case 1: { lastMonthSTR = "January"; break; }
                    case 2: { lastMonthSTR = "February"; break; }
                    case 3: { lastMonthSTR = "March"; break; }
                    case 4: { lastMonthSTR = "April"; break; }
                    case 5: { lastMonthSTR = "May"; break; }
                    case 6: { lastMonthSTR = "June"; break; }
                    case 7: { lastMonthSTR = "July"; break; }
                    case 8: { lastMonthSTR = "August"; break; }
                    case 9: { lastMonthSTR = "Sebtember"; break; }
                    case 10: { lastMonthSTR = "October"; break; }
                    case 11: { lastMonthSTR = "November"; break; }
                    case 12: { lastMonthSTR = "December"; break; }
                }

                // sets calculated time
                if (yearsAgo > 1) { TBGameDetailsLastTimePlayed.Text = $"Last time played: {lastDay} {lastMonthSTR} {lastYear} ({yearsAgo} years ago)"; }
                else if (yearsAgo == 1) { TBGameDetailsLastTimePlayed.Text = $"Last time played: {lastDay} {lastMonthSTR} {lastYear} ({yearsAgo} year ago)"; }
                else if (monthsAgo > 1) { TBGameDetailsLastTimePlayed.Text = $"Last time played: {lastDay} {lastMonthSTR} {lastYear} ({monthsAgo} months ago)"; }
                else if (monthsAgo == 1) { TBGameDetailsLastTimePlayed.Text = $"Last time played: {lastDay} {lastMonthSTR} {lastYear} ({monthsAgo} month ago)"; }
                else if (daysAgo > 1) { TBGameDetailsLastTimePlayed.Text = $"Last time played: {lastDay} {lastMonthSTR} {lastYear} ({daysAgo} days ago)"; }
                else if (daysAgo == 1) { TBGameDetailsLastTimePlayed.Text = $"Last time played: {lastDay} {lastMonthSTR} {lastYear} (Yesterday)"; }
                else if (daysAgo <= 0) { TBGameDetailsLastTimePlayed.Text = $"Last time played: {lastDay} {lastMonthSTR} {lastYear} (Today)"; }
                if (lastDay <= 0
                    || lastMonth <= 0
                    || lastYear <= 0) { TBGameDetailsPlayTime.Text = string.Empty; TBGameDetailsLastTimePlayed.Text = string.Empty; }

                // sets settings panel
                TBSettingsResolution.Text = $"Resolution: {(int)Config.Version.Properties.Get(version, "ResWidth")}x{(int)Config.Version.Properties.Get(version, "ResHeight")}";

                int textureFiltering = (int)Config.Version.Properties.Get(version, "TextureFiltering");
                if (textureFiltering == 0) { TBSettingsTextureFiltering.Text = "Texture Filtering: Off"; }
                else { TBSettingsTextureFiltering.Text = $"Texture Filtering: {textureFiltering}x"; }

                int antiAliasing = (int)Config.Version.Properties.Get(version, "AntiAliasing");
                if (antiAliasing == 0) { TBSettingsAntiAliasing.Text = "Anti-Aliasing: Off"; }
                else { TBSettingsAntiAliasing.Text = $"Anti-Aliasing: {antiAliasing}x"; }

                int bilinearBlitStretch = (int)Config.Version.Properties.Get(version, "BilinearBlitStretch");
                if (bilinearBlitStretch == 0) { TBSettingsBilinearBlitStretch.Text = "Bilinear Blit Stretch: Off"; }
                else { TBSettingsBilinearBlitStretch.Text = "Bilinear Blit Stretch: On"; }

                int DGVoodoo = (int)Config.Version.Properties.Get(version, "Fullscreen");
                if (DGVoodoo == 0) { TBSettingsFullscreen.Text = "Fullscreen: Off"; }
                else { TBSettingsFullscreen.Text = "Fullscreen: On"; }

                int dgVoodooWatermark = (int)Config.Version.Properties.Get(version, "DGVoodooWatermark");
                if (dgVoodooWatermark == 0) { TBSettingsDGVoodooWatermark.Text = "DGVoodoo Watermark: Off"; }
                else { TBSettingsDGVoodooWatermark.Text = "DGVoodoo Watermark: On"; }

                int appControlledWindowState = (int)Config.Version.Properties.Get(version, "AppControlledWindowState");
                if (appControlledWindowState == 0) { TBSettingsAppControlledWindowState.Text = "App-Controlled Window State: Off"; }
                else { TBSettingsAppControlledWindowState.Text = "App-Controlled Window State: On"; }

                string language = Config.Version.Properties.Get(version, "Language").ToString().ToLower();
                language = $"{language[0].ToString().ToUpper()}{language.Substring(1)}";
                TBSettingsLanguage.Text = $"Language: {language}";

                // sets size and calculates it if needed
                long size = (int)Config.Version.Properties.Get(version, "Size");
                if (size == 0 && Config.Version.Properties.Get(version, "Path").ToString().ToLower() != "unset")
                {
                    TBDetailsSize.Text = "Size: Calculating...";
                    DirectoryInfo di = new DirectoryInfo((string)Config.Version.Properties.Get(version, "Path"));
                    size = await Game.GetSizeAsync(di) / 1024 / 1024;
                    Config.Version.Properties.Set(version, "Size", (int)size);
                }
                TBDetailsSize.Text = $"Size: {size} MB";

                // sets release date
                for (int i = 0; i < TTLib.Properties._ValidVersions.Length; i++)
                {
                    if (version.ToLower() == TTLib.Properties._ValidVersions[i].ToLower())
                    {
                        TBDetailsReleaseDate.Text = $"Release Date: {TTLib.Properties._ValidReleaseDates[i]}";
                    }
                }

                // sets controller support
                if (File.Exists($"{(string)Config.Version.Properties.Get(version, "Path")}dinput.dll")) { TBDetailsControllerSupport.Text = "Controller Support: Yes"; }
                else { TBDetailsControllerSupport.Text = "Controller Support: No"; }

                // sets indeo drivers
                if (Game.IndeoDriversInstalled()) { TBIndeoDrivers.Text = "Indeo Drivers: Installed"; }
                else { TBIndeoDrivers.Text = "Indeo Drivers: Not installed"; }

                // sets dgvoodoo
                if (version != "SE-V8.1.0")
                {
                    if (File.Exists($"{path}D3D8.dll")
                        && File.Exists($"{path}D3DImm.dll")
                        && File.Exists($"{path}DDraw.dll")
                        && File.Exists($"{path}dgvoodoo.conf")) { TBDGVoodoo.Text = "DGVoodoo: Installed"; }
                    else { TBDGVoodoo.Text = "DGVoodoo: Not installed"; }
                }
                else
                {
                    if (File.Exists($"{path}Glide2x.dll") && File.Exists($"{path}dgvoodoo.conf")) { TBDGVoodoo.Text = "DGVoodoo: Installed"; }
                    else { TBDGVoodoo.Text = "DGVoodoo: Not installed"; }
                }
            });
        }
        public async Task UpdateGameDetailsAsync()
        {
            await Task.Run(() => UpdateGameDetails());
        }

        private ListBoxItem GetSelectedVersionItem()
        {
            ListBoxItem lbi = null;

            // special edition
            if (LBSE.SelectedIndex != -1) { lbi = (ListBoxItem)LBSE.ItemContainerGenerator.ContainerFromIndex(LBSE.SelectedIndex); }

            // retail version
            if (LBRT.SelectedIndex != -1) { lbi = (ListBoxItem)LBRT.ItemContainerGenerator.ContainerFromIndex(LBRT.SelectedIndex); }
            return lbi;
        }
        private void UpdateLibrary()
        {
            // clears library
            LBSE.Items.Clear();
            LBRT.Items.Clear();

            // resets repair flag
            if (repairVersion != null) { repairVersion = null; }

            // sets game details
            ToggleGameDetails(false, false, false, false, false, false, false, false);

            // runs verifying routine
            Verify();

            // if any version exist
            if (Config.Version.ExistsAny())
            {
                // gets all existing versions
                string[] version = Config.Version.GetAll();
                ListBox lb = null;
                for (int i = 0; i < version.Length; i++)
                {
                    // if received version is special edition or retail version
                    if (version[i].ToLower().Contains("SE-".ToLower())) { lb = LBSE; }
                    else if (version[i].ToLower().Contains("RT-".ToLower())) { lb = LBRT; }

                    // if path or language is invalid then add version with a warning
                    if (!Config.Version.IsValid(version[i]))
                    {
                        var lbt = new ListBoxItem
                        {
                            Content = $"{version[i].Remove(0, 3)} (Warning)",
                            Foreground = Brushes.Red
                        };
                        lb.Items.Add(lbt); continue;
                    }

                    // if everything is O.K then add version normally
                    lb.Items.Add(version[i].Remove(0, 3));
                }
            }

            // enables or disables refresh button
            if (LBSE.Items.Count > 0 || LBRT.Items.Count > 0) 
            {
                BTRefreshLibrary.IsEnabled = true;
            }
            else
            {
                BTRefreshLibrary.IsEnabled = false;
            }

            // if library includes items then expand
            if (LBSE.Items.Count == 0)
            {
                EXPSE.IsExpanded = false;
                EXPSE.Header = "Special Edition";
            }
            else
            {
                EXPSE.IsExpanded = true;
                EXPSE.Header = $"Special Edition ({LBSE.Items.Count})";
            }

            // if library includes no items then hide library
            if (LBRT.Items.Count == 0)
            {
                EXPRT.IsExpanded = false;
                EXPRT.Header = "Retail Version";
            }
            else
            {
                EXPRT.IsExpanded = true;
                EXPRT.Header = $"Retail Version ({LBRT.Items.Count})";
            }
        }
        private void ToggleGameDetails(bool navigationBar, bool playButton, bool fixButton, bool updateButton, bool settingsButton, bool removeButton, bool detailsBar, bool settingsPanel)
        {
            if (navigationBar)
            {
                TBGameDetailsLastTimePlayed.Visibility = Visibility.Visible;
                TBGameDetailsPlayTime.Visibility = Visibility.Visible;
            }
            else
            {
                TBGameDetailsLastTimePlayed.Visibility = Visibility.Hidden;
                TBGameDetailsPlayTime.Visibility = Visibility.Hidden;
            }

            if (playButton) { BTPlay.IsEnabled = true; }
            else { BTPlay.IsEnabled = false; }

            if (fixButton) { BTPlay.Content = "Fix (!)"; }
            else if (!fixButton && (string)BTPlay.Content != "Stop") { BTPlay.Content = "Play"; }

            if (settingsButton) { BTGameSettings.IsEnabled = true; }
            else { BTGameSettings.IsEnabled = false; }

            if (removeButton) { BTRemove.Visibility = Visibility.Visible; }
            else { BTRemove.Visibility = Visibility.Hidden; }

            if (detailsBar)
            {
                TBDetailsSize.Visibility = Visibility.Visible;
                TBDetailsReleaseDate.Visibility = Visibility.Visible;
                TBDetailsControllerSupport.Visibility = Visibility.Visible;
                RECTDetailsSeparator.Visibility = Visibility.Visible;
                RECTDetailsSeparator2.Visibility = Visibility.Visible;
                TBIndeoDrivers.Visibility = Visibility.Visible;
                TBDGVoodoo.Visibility = Visibility.Visible;
            }
            else
            {
                TBDetailsSize.Visibility = Visibility.Hidden;
                TBDetailsReleaseDate.Visibility = Visibility.Hidden;
                TBDetailsControllerSupport.Visibility = Visibility.Hidden;
                RECTDetailsSeparator.Visibility = Visibility.Hidden;
                RECTDetailsSeparator2.Visibility = Visibility.Hidden;
                TBIndeoDrivers.Visibility = Visibility.Hidden;
                TBDGVoodoo.Visibility = Visibility.Hidden;
            }
            if (settingsPanel)
            {
                BOSettingsVideo.Visibility = Visibility.Visible;
                TBSettingsResolution.Visibility = Visibility.Visible;
                TBSettingsTextureFiltering.Visibility = Visibility.Visible;
                TBSettingsAntiAliasing.Visibility = Visibility.Visible;
                TBSettingsBilinearBlitStretch.Visibility = Visibility.Visible;
                BOSettingsOther.Visibility = Visibility.Visible;
                TBSettingsFullscreen.Visibility = Visibility.Visible;
                TBSettingsDGVoodooWatermark.Visibility = Visibility.Visible;
                TBSettingsAppControlledWindowState.Visibility = Visibility.Visible;
                BOSettingsGame.Visibility = Visibility.Visible;
                TBSettingsLanguage.Visibility = Visibility.Visible;
            }
            else
            {
                BOSettingsVideo.Visibility = Visibility.Hidden;
                TBSettingsResolution.Visibility = Visibility.Hidden;
                TBSettingsTextureFiltering.Visibility = Visibility.Hidden;
                TBSettingsAntiAliasing.Visibility = Visibility.Hidden;
                TBSettingsBilinearBlitStretch.Visibility = Visibility.Hidden;
                BOSettingsOther.Visibility = Visibility.Hidden;
                TBSettingsFullscreen.Visibility = Visibility.Hidden;
                TBSettingsDGVoodooWatermark.Visibility = Visibility.Hidden;
                TBSettingsAppControlledWindowState.Visibility = Visibility.Hidden;
                BOSettingsGame.Visibility = Visibility.Hidden;
                TBSettingsLanguage.Visibility = Visibility.Hidden;
            }
        }
        private string GetSelectedVersion()
        {
            string version = null;
            ListBoxItem lbi;

            // special edition
            if (LBSE.SelectedIndex != -1)
            {
                // gets verison of clicked listboxitem
                lbi = (ListBoxItem)LBSE.ItemContainerGenerator.ContainerFromIndex(LBSE.SelectedIndex);
                version = $"SE-{(string)lbi.Content}";
                if (version.Contains("(Warning)"))
                {
                    version = version.Replace("(Warning)", "");
                    version = version.Remove(version.Length - 1, 1);
                }
            }

            // retail version
            if (LBRT.SelectedIndex != -1)
            {
                // gets verison of clicked listboxitem
                lbi = (ListBoxItem)LBRT.ItemContainerGenerator.ContainerFromIndex(LBRT.SelectedIndex);
                version = $"RT-{(string)lbi.Content}";
                if (version.Contains("(Warning)"))
                {
                    version = version.Replace("(Warning)", "");
                    version = version.Remove(version.Length - 1, 1);
                }
            }
            return version;
        }
        private void StartGame(int versionId, string version)
        {
            // if ubisoft folder doesn't exist
            if (!UbiConfig.Directory.Exists())
            {
                // creates ubisoft folder
                UbiConfig.Directory.Create();

                // special edition
                if (versionId == 0)
                {
                    // creates ubi.ini 
                    UbiConfig.Create(0, $"{(string)Config.Version.Properties.Get(version, "Path")}", $"{(string)Config.Version.Properties.Get(version, "Language")}");

                    // starts the game
                    using (Process TT = new Process())
                    {
                        TT.StartInfo.WorkingDirectory = (string)Config.Version.Properties.Get(version, "Path");
                        TT.StartInfo.FileName = ("UBISETUP.EXE");
                        TT.StartInfo.Arguments = ("-play TONICT");
                        TT.Start();
                    }
                }

                // retail version
                else if (versionId == 1)
                {
                    // creates ubi.ini
                    UbiConfig.Create(1, $"{(string)Config.Version.Properties.Get(version, "Path")}", $"{(string)Config.Version.Properties.Get(version, "Language")}");

                    // starts the game
                    using (Process TT = new Process())
                    {
                        TT.StartInfo.WorkingDirectory = (string)Config.Version.Properties.Get(version, "Path");
                        TT.StartInfo.FileName = ("INSTALTT.EXE");
                        TT.StartInfo.Arguments = ("-play TONICT");
                        TT.Start();
                    }
                }
            }

            // if ubisoft folder exists
            else
            {
                // if ubi.ini already exists
                if (UbiConfig.Exists())
                {
                    // backups ubi.ini
                    if (!UbiConfig.BackupExists()) { UbiConfig.Backup(); }

                    // special edition
                    if (versionId == 0)
                    {
                        // creates ubi.ini 
                        UbiConfig.Create(0, $"{(string)Config.Version.Properties.Get(version, "Path")}", $"{(string)Config.Version.Properties.Get(version, "Language")}");

                        // starts the game
                        using (Process TT = new Process())
                        {
                            TT.StartInfo.WorkingDirectory = (string)Config.Version.Properties.Get(version, "Path");
                            TT.StartInfo.FileName = ("UBISETUP.EXE");
                            TT.StartInfo.Arguments = ("-play TONICT");
                            TT.Start();
                        }
                    }

                    // retail version
                    else if (versionId == 1)
                    {
                        // creates ubi.ini
                        UbiConfig.Create(1, $"{(string)Config.Version.Properties.Get(version, "Path")}", $"{(string)Config.Version.Properties.Get(version, "Language")}");

                        // starts the game
                        using (Process TT = new Process())
                        {
                            TT.StartInfo.WorkingDirectory = (string)Config.Version.Properties.Get(version, "Path");
                            TT.StartInfo.FileName = ("INSTALTT.EXE");
                            TT.StartInfo.Arguments = ("-play TONICT");
                            TT.Start();
                        }
                    }
                }

                // if ubi.ini doesn't exist
                else
                {
                    // special edition
                    if (versionId == 0)
                    {
                        // creates ubi.ini 
                        UbiConfig.Create(0, $"{(string)Config.Version.Properties.Get(version, "Path")}", $"{(string)Config.Version.Properties.Get(version, "Language")}");

                        // starts the game
                        using (Process TT = new Process())
                        {
                            TT.StartInfo.WorkingDirectory = (string)Config.Version.Properties.Get(version, "Path");
                            TT.StartInfo.FileName = ("UBISETUP.EXE");
                            TT.StartInfo.Arguments = ("-play TONICT");
                            TT.Start();
                        }
                    }

                    // retail version
                    else if (versionId == 1)
                    {
                        // creates ubi.ini
                        UbiConfig.Create(1, $"{(string)Config.Version.Properties.Get(version, "Path")}", $"{(string)Config.Version.Properties.Get(version, "Language")}");

                        // starts the game
                        using (Process TT = new Process())
                        {
                            TT.StartInfo.WorkingDirectory = (string)Config.Version.Properties.Get(version, "Path");
                            TT.StartInfo.FileName = ("INSTALTT.EXE");
                            TT.StartInfo.Arguments = ("-play TONICT");
                            TT.Start();
                        }
                    }
                }
            }

            totalPlaytime.Start();
        }
        private void UpdateDGVoodooConfig(string version)
        {
            // gets path of the version
            string path = (string)Config.Version.Properties.Get(version, "Path");

            // creates config if needed
            if (!DGVoodoo.Config.Exists(path)) { DGVoodoo.Config.Create(path); }

            // anti-aliasing
            if ((int)Config.Version.Properties.Get(version, "AntiAliasing") == 0) { DGVoodoo.Config.Properties.Set(path, "AntiAliasing", "appdriven"); }
            else { DGVoodoo.Config.Properties.Set(path, "AntiAliasing", $"{Config.Version.Properties.Get(version, "AntiAliasing")}x"); }

            // fullscreen mode
            if ((int)Config.Version.Properties.Get(version, "AppControlledWindowState") == 0) { DGVoodoo.Config.Properties.Set(path, "AppControlledScreenMode", "false"); }
            else { DGVoodoo.Config.Properties.Set(path, "AppControlledScreenMode", "true"); }

            // bilinear blit stretch
            if ((int)Config.Version.Properties.Get(version, "BilinearBlitStretch") == 0) { DGVoodoo.Config.Properties.Set(path, "BilinearBlitStretch", "false"); }
            else { DGVoodoo.Config.Properties.Set(path, "BilinearBlitStretch", "true"); }

            // watermark
            if ((int)Config.Version.Properties.Get(version, "DGVoodooWatermark") == 0) { DGVoodoo.Config.Properties.Set(path, "DGVoodooWatermark", "false"); }
            else { DGVoodoo.Config.Properties.Set(path, "DGVoodooWatermark", "true"); }

            // watermark
            if ((int)Config.Version.Properties.Get(version, "DGVoodooWatermark") == 0) { DGVoodoo.Config.Properties.Set(path, "DGVoodooWatermark", "false"); }
            else { DGVoodoo.Config.Properties.Set(path, "DGVoodooWatermark", "true"); }

            // fullscreen
            if ((int)Config.Version.Properties.Get(version, "Fullscreen") == 0) { DGVoodoo.Config.Properties.Set(path, "FullScreenMode", "false"); }
            else { DGVoodoo.Config.Properties.Set(path, "FullScreenMode", "true"); }

            // resolution
            string resolution = $"{(int)Config.Version.Properties.Get(version, "ResWidth")}x{(int)Config.Version.Properties.Get(version, "ResHeight")}";
            DGVoodoo.Config.Properties.Set(path, "Resolution", resolution);

            // texture filtering
            if ((int)Config.Version.Properties.Get(version, "TextureFiltering") == 0) { DGVoodoo.Config.Properties.Set(path, "TextureFiltering", "appdriven"); }
            else { DGVoodoo.Config.Properties.Set(path, "TextureFiltering", $"{Config.Version.Properties.Get(version, "TextureFiltering")}"); }
        }
        private async void UpdateTotalPlaytime()
        {
            Process[] processSE = Process.GetProcessesByName("MaiDFXvd");
            Process[] processSE_Glide = Process.GetProcessesByName("MaiD3Dvr");
            Process[] processRT = Process.GetProcessesByName("TonicTrouble");

            string runningVersion = (string)Config.Properties.Get("RunningVersion");

            this.Dispatcher.Invoke(() => { if (processSE_Glide.Length > 0 && runningVersion == GetSelectedVersion()) { BTPlay.Content = "Stop"; }});
            this.Dispatcher.Invoke(() => { if (processSE.Length > 0 && runningVersion == GetSelectedVersion()) { BTPlay.Content = "Stop"; }});
            this.Dispatcher.Invoke(() => { if (processRT.Length > 0 && runningVersion == GetSelectedVersion()) { BTPlay.Content = "Stop"; }});

            this.Dispatcher.Invoke(() => 
            {
                if (processSE.Length == 0
                && processSE_Glide.Length == 0
                && processRT.Length == 0) { BTPlay.Content = "Play"; totalPlaytimeTick = 0; totalPlaytime.Stop(); }
            });

            if (totalPlaytime.Enabled) 
            {
                int totalMin = (int)Config.Version.Properties.Get(runningVersion, "TotalMin");
                int totalHour = (int)Config.Version.Properties.Get(runningVersion, "TotalHour");

                totalPlaytimeTick++;

                if (totalPlaytimeTick >= 30)
                {
                    totalPlaytimeTick = 0;
                    totalMin++;
                    if (totalMin >= 60)
                    {
                        totalMin = 0;
                        totalHour++;
                    }

                    Config.Version.Properties.Set(runningVersion, "TotalMin", totalMin);
                    Config.Version.Properties.Set(runningVersion, "TotalHour", totalHour);
                }

                await UpdateGameDetailsAsync();
            }
        }

        // DEBUG BUTTON
        private void DEBUGBUTTON_Click(object sender, RoutedEventArgs e)
        {
            // causes crash
            //File.Move("","");
        }
    }
}
