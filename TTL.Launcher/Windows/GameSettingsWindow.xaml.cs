using System.Windows;
using System.Collections.Generic;
using TTLib;
using System.Diagnostics;

namespace TTL.Launcher
{
    public partial class GameSettingsWindow : Window
    {
        // instance of 'mainwindow'
        private readonly MainWindow mainWindow = null;

        // selected version
        private readonly string version = string.Empty;

        public GameSettingsWindow(MainWindow _mainWindow, string _version)
        {
            InitializeComponent();

            // sets 'mainwindow' to owner
            mainWindow = _mainWindow;
            this.Owner = _mainWindow;

            // sets selected version
            version = _version;
            this.Title = $"Settings [{version.Remove(0, 3)}]";

            Process[] processSE = Process.GetProcessesByName("MaiDFXvd");
            Process[] processSE_Glide = Process.GetProcessesByName("MaiD3Dvr");
            Process[] processRT = Process.GetProcessesByName("TonicTrouble");

            if (processSE.Length == 0
                && processSE_Glide.Length == 0
                && processRT.Length == 0) 
            {
                CBResolution.IsEnabled = true;
                CBTextureFiltering.IsEnabled = true;
                CBAntiAliasing.IsEnabled = true;
                RBBilinearOn.IsEnabled = true;
                RBBilinearOff.IsEnabled = true;
                RBFullscreenOn.IsEnabled = true;
                RBFullscreenOff.IsEnabled = true;
                RBVoodooWMOn.IsEnabled = true;
                RBVoodooWMOff.IsEnabled = true;
                RBAPPStateOn.IsEnabled = true;
                RBAPPStateOff.IsEnabled = true;
                CBLanguage.IsEnabled = true;
                CBResetDGVoodooConfig.IsEnabled = true;
                CBInstallIndeo.IsEnabled = true;
                BTSave.IsEnabled = true;
            }
            else 
            {
                CBResolution.IsEnabled = false;
                CBTextureFiltering.IsEnabled = false;
                CBAntiAliasing.IsEnabled = false;
                RBBilinearOn.IsEnabled = false;
                RBBilinearOff.IsEnabled = false;
                RBFullscreenOn.IsEnabled = false;
                RBFullscreenOff.IsEnabled = false;
                RBVoodooWMOn.IsEnabled = false;
                RBVoodooWMOff.IsEnabled = false;
                RBAPPStateOn.IsEnabled = false;
                RBAPPStateOff.IsEnabled = false;
                CBLanguage.IsEnabled = false;
                CBResetDGVoodooConfig.IsEnabled = false;
                CBInstallIndeo.IsEnabled = false;
                BTSave.IsEnabled = false;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // gets current saved resolution
            string currentResolution = $"{Config.Version.Properties.Get(version, "ResWidth")}x{Config.Version.Properties.Get(version, "ResHeight")}";

            // gets current saved texture filtering setting
            string currentTextureFiltering = $"{(int)Config.Version.Properties.Get(version, "TextureFiltering")}x";
            if (currentTextureFiltering == "0x") { currentTextureFiltering = "Off"; }

            // gets current saved anti-aliasing setting
            string currentAntiAliasing = $"{(int)Config.Version.Properties.Get(version, "AntiAliasing")}x";
            if (currentAntiAliasing == "0x") { currentAntiAliasing = "Off"; }

            // gets current saved language
            string currentLanguage = (string)Config.Version.Properties.Get(version, "Language");
            currentLanguage = $"{currentLanguage[0].ToString().ToUpper()}{currentLanguage.Substring(1).ToLower()}";

            // sets resolutions
            var resolutions = new List<string>();
            for (int i = 0; i < TTLib.Properties._ValidResolutionWidths.Length; i++)
            {
                resolutions.Add($"{TTLib.Properties._ValidResolutionWidths[i]}x{TTLib.Properties._ValidResolutionHeights[i]}");
            }
            for (int i = 0; i < resolutions.Count; i++)
            {
                CBResolution.Items.Add(resolutions[i]);
            }
            CBResolution.SelectedValue = currentResolution;

            // sets texture filtering
            for (int i = 0; i < TTLib.Properties._ValidTextureFiltering.Length; i++)
            {
                if (i == 0) { CBTextureFiltering.Items.Add("Off"); continue; }
                CBTextureFiltering.Items.Add($"{TTLib.Properties._ValidTextureFiltering[i]}x");
            }
            if (currentTextureFiltering == "0x") { CBTextureFiltering.SelectedValue = "Off"; }
            else { CBTextureFiltering.SelectedValue = currentTextureFiltering; }

            // sets anti-aliasing
            for (int i = 0; i < TTLib.Properties._ValidAntiAliasing.Length; i++)
            {
                if (i == 0) { CBAntiAliasing.Items.Add("Off"); continue; }
                CBAntiAliasing.Items.Add($"{TTLib.Properties._ValidAntiAliasing[i]}x");
            }
            CBAntiAliasing.SelectedValue = currentAntiAliasing;

            // sets bilinear blit stretch
            if ((int)Config.Version.Properties.Get(version, "BilinearBlitStretch") == 0) { RBBilinearOff.IsChecked = true; }
            else { RBBilinearOn.IsChecked = true; }

            // sets fullscreen
            if ((int)Config.Version.Properties.Get(version, "Fullscreen") == 0) { RBFullscreenOff.IsChecked = true; }
            else { RBFullscreenOn.IsChecked = true; }

            // sets dgvoodoo watermark
            if ((int)Config.Version.Properties.Get(version, "DGVoodooWatermark") == 0) { RBVoodooWMOff.IsChecked = true; }
            else { RBVoodooWMOn.IsChecked = true; }

            // sets app controlled window state
            if ((int)Config.Version.Properties.Get(version, "AppControlledWindowState") == 0) { RBAPPStateOff.IsChecked = true; }
            else { RBAPPStateOn.IsChecked = true; }

            // adds supported languages
            bool flag = false;
            string[] getLanguages = Game.GetLanguages($@"{(string)Config.Version.Properties.Get(version, "Path")}GameData\World\Sound\");
            for (int i = 0; i < getLanguages.Length; i++)
            {
                for (int j = 0; j < TTLib.Properties._ValidLanguages.Length; j++)
                {
                    if (TTLib.Properties._ValidLanguages[j].ToLower() == getLanguages[i].ToLower())
                    {
                        CBLanguage.Items.Add(TTLib.Properties._ValidLanguages[j]);
                    }
                    if (getLanguages[i].ToLower() == currentLanguage.ToLower())
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                CBLanguage.SelectedValue = currentLanguage;
            }
            else
            {
                CBLanguage.SelectedIndex = 0;
            }

            // checks wether indeo drivers are installed or not
            if (Game.IndeoDriversInstalled()) { BTInstallIndeo.Content = "Uninstall"; TBInstallIndeo.Text = "Uninstall Indeo Drivers:"; }
            else { BTInstallIndeo.Content = "Install"; TBInstallIndeo.Text = "Install Indeo Drivers:"; }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult == null)
            {
                this.DialogResult = true;
            }
        }
        private void BTSave_Click(object sender, RoutedEventArgs e)
        {
            // runs verifying routine
            mainWindow.Verify();

            // if version exist at all
            if (Config.Version.Exists(version) && Config.Version.IsValid(version))
            {
                // saves new settings
                Config.Version.Properties.Set(version, "ResWidth", TTLib.Properties._ValidResolutionWidths[CBResolution.SelectedIndex]);
                Config.Version.Properties.Set(version, "ResHeight", TTLib.Properties._ValidResolutionHeights[CBResolution.SelectedIndex]);
                Config.Version.Properties.Set(version, "TextureFiltering", TTLib.Properties._ValidTextureFiltering[CBTextureFiltering.SelectedIndex]);
                Config.Version.Properties.Set(version, "AntiAliasing", TTLib.Properties._ValidAntiAliasing[CBAntiAliasing.SelectedIndex]);
                Config.Version.Properties.Set(version, "BilinearBlitStretch", (bool)RBBilinearOn.IsChecked);
                Config.Version.Properties.Set(version, "Fullscreen", (bool)RBFullscreenOn.IsChecked);
                Config.Version.Properties.Set(version, "DGVoodooWatermark", (bool)RBVoodooWMOn.IsChecked);
                Config.Version.Properties.Set(version, "AppControlledWindowState", (bool)RBAPPStateOn.IsChecked);
                Config.Version.Properties.Set(version, "Language", CBLanguage.SelectedValue);
                this.DialogResult = true;
            }
            else
            {
                this.DialogResult = false;
            }
        }
        private void BTCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void CBResetDGVoodooConfig_Click(object sender, RoutedEventArgs e)
        {
            // activates or deactivates 'reset' button
            if (CBResetDGVoodooConfig.IsChecked ?? false) { BTResetDGVoodooConfig.IsEnabled = true; }
            else { BTResetDGVoodooConfig.IsEnabled = false; }
        }
        private async void BTResetDGVoodooConfig_Click(object sender, RoutedEventArgs e)
        {
            // runs verifying routine
            mainWindow.Verify();

            // if version exist at all
            if (Config.Version.Exists(version) && Config.Version.IsValid(version))
            {
                // gets path of the version
                string path = (string)Config.Version.Properties.Get(version, "Path");
                if (DGVoodoo.Config.Exists(path)) { DGVoodoo.Config.Delete(path); DGVoodoo.Config.Create(path); }
                else { DGVoodoo.Config.Create(path); }

                DGVoodoo.Install(path, CntLib.Properties.Resources.D3D8, CntLib.Properties.Resources.D3DImm, CntLib.Properties.Resources.DDraw, null);

                // resets reset button
                CBResetDGVoodooConfig.IsChecked = false;
                BTResetDGVoodooConfig.IsEnabled = false;

                // gives success message
                await mainWindow.UpdateGameDetailsAsync();
                MessageBox.MessageBox.ShowDialog(this, "DDGVoodoo got sucessfully re-installed.", "Information", "OK");
            }
            else { this.DialogResult = false; }
        }
        private async void BTReinstallIndeo_Click(object sender, RoutedEventArgs e)
        {
            if ((string)BTInstallIndeo.Content == "Install")
            {
                // installs indeo drivers
                if (!Game.IndeoDriversInstalled())
                {
                    Game.InstallIndeoDrivers(CntLib.Properties.Resources.ir32_32, CntLib.Properties.Resources.ir41_32);
                    MessageBox.MessageBox.ShowDialog(this, "Indeo Drivers successfully installed.", "Information", "OK");
                }

                // resets reset button
                BTInstallIndeo.Content = "Uninstall";
                CBInstallIndeo.IsChecked = false;
                BTInstallIndeo.IsEnabled = false;
                TBInstallIndeo.Text = "Uninstall Indeo Drivers:";
                await mainWindow.UpdateGameDetailsAsync();
            }

            // uninstalls indeo drivers
            else if ((string)BTInstallIndeo.Content == "Uninstall")
            {
                if (Game.IndeoDriversInstalled()) 
                { 
                    Game.UninstallIndeoDrivers();
                    MessageBox.MessageBox.ShowDialog(this, "Indeo Drivers successfully uninstalled.", "Information", "OK");
                }

                // resets reset button
                BTInstallIndeo.Content = "Install";
                CBInstallIndeo.IsChecked = false;
                BTInstallIndeo.IsEnabled = false;
                TBInstallIndeo.Text = "Install Indeo Drivers:";
                await mainWindow.UpdateGameDetailsAsync();
            }
        }
        private void CBReinstallIndeo_Click(object sender, RoutedEventArgs e)
        {
            // activates or deactivates 'install' button
            if (CBInstallIndeo.IsChecked ?? false) { BTInstallIndeo.IsEnabled = true; }
            else { BTInstallIndeo.IsEnabled = false; }
        }
    }
}
