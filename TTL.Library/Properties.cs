namespace TTLib
{
    /// <summary> A collection of valid properties </summary>
    public static class Properties
    {
        /// <summary> The launcher's version. </summary>
        public static readonly string _LauncherVersion = "2.02";

        /// <summary> DGVoodoo's version. </summary>
        public static readonly string _DGVoodooVersion = "2.7";

        /// <summary> Path to the launcher's root registry key. </summary>
        public static readonly string _LauncherRootKeyPath = @"Software\TTL\";

        /// <summary> Path to the launcher's registry key. </summary>
        public static readonly string _LauncherKeyPath = @"Software\TTL\Launcher\";

        /// <summary> Path to Indeo Drivers. </summary>
        public static readonly string _IndeoPath = @"Software\Classes\System\CurrentControlSet\control\MediaResources\icm\";

        /// <summary> Path to Indeo Drivers. </summary>
        public static readonly string _IndeoPathAlt = @"Software\Microsoft\Windows NT\CurrentVersion\Drivers32\";

        /// <summary> Path to Ubi configuration file. </summary>
        public static readonly string _UbiConfigPath = @"C:\Windows\UbiSoft\ubi.ini";

        /// <summary> Path to Ubi configuration folder. </summary>
        public static readonly string _UbiConfigDirectoryPath = @"C:\Windows\UbiSoft\";

        /// <summary> Collection of valid versions. </summary>
        public static readonly string[] _ValidVersions =
        {
            "SE-V8.1.0", "SE-V8.5.1", "SE-V8.5.2", "SE-V8.6.1", "SE-V8.6.2",
            "SE-V8.6.6", "SE-V8.6.8", "SE-V8.7.0", "SE-V8.7.4", "RT-Retail Master V5",
            "RT-Retail Master German V3", "RT-Review English (ITA)", "RT-Review English (ESP)",
            "RT-Retail Master English V3"
        };

        /// <summary> Collection of valid release dates. </summary>
        public static readonly string[] _ValidReleaseDates =
        {
            "Jan 30 1998", "Feb 13 1998", "Feb 17 1998", "Mar 31 1998", "Mar 30 1998",
            "Apr 30 1998", "Jul 27 1998", "Sep 01 1998", "Apr 14 1999", "Oct 18 1999",
            "Oct 22 1999", "Oct 22 1999", "Oct 22 1999", "Oct 13 1999"
        };

        /// <summary> Collection of valid languages. </summary>
        public static readonly string[] _ValidLanguages =
        {
            "English", "French", "German", "Italian", "Spanish"
        };

        /// <summary> Collection of valid version properties (Type: string) </summary>
        public static readonly string[] _ValidVersionPropertiesString =
        {
            "Path", "Language"
        };

        /// <summary> Collection of valid version properties (Type: dword) </summary>
        public static readonly string[] _ValidVersionPropertiesDWord =
        {
            "DGVoodooWatermark", "Fullscreen",
            "ResWidth", "ResHeight", "AntiAliasing", "TextureFiltering",
            "BilinearBlitStretch", "AppControlledWindowState", "TotalHour", "TotalMin",
            "LastDay", "LastMonth", "LastYear", "Size", "Update"
        };

        /// <summary> Collection of default version properties (string) </summary>
        public static readonly string[] _DefaultVersionPropertiesString =
        {
            "unset", "unset"
        };

        /// <summary> Collection of default version properties (dword) </summary>
        public static readonly int[] _DefaultVersionPropertiesDWord =
        {
            0, 0, 1024, 768, 0, 4, 1, 0, 0, 0, 0, 0, 0, 0, 0
        };

        /// <summary> Collection of valid launcher properties (string) </summary>
        public static readonly string[] _ValidLauncherPropertiesString =
        {
            "Version", "LastPath", "LastInstallPath", "RunningVersion"
        };

        /// <summary> Collection of valid launcher properties (dword) </summary>
        public static readonly string[] _ValidLauncherPropertiesDWord =
        {
            "MinimizeLauncher", "ShowDiscordStatus", "LastDiscordTimeHours", "LastDiscordTimeMinutes", "LastDiscordTimeSeconds"
        };

        /// <summary> Collection of default launcher properties (string) </summary>
        public static readonly string[] _DefaultLauncherPropertiesString =
        {
            "", "unset", "unset", "unset"
        };

        /// <summary> Collection of default launcher properties (dword) </summary>
        public static readonly int[] _DefaultLauncherPropertiesDWord =
        {
            0, 1, 0, 0, 0
        };


        /// <summary> Collection of valid resolution widths </summary>
        public static readonly int[] _ValidResolutionWidths =
        {
            640, 720, 720, 800, 1024, 1152, 1280, 1280,
            1280, 1280, 1280, 1360, 1366, 1600, 1600, 1600,
            1680, 1920, 1920, 1920, 2560, 3840
        };

        /// <summary> Collection of valid resolution heights </summary>
        public static readonly int[] _ValidResolutionHeights = 
        { 
            480, 480, 576, 600, 768, 864,720, 768, 800,
            960, 1024, 768, 768, 900, 1024,1200, 1050, 1080,
            1200, 1440, 1440, 2160
        };

        /// <summary> Collection of valid texture filtering values </summary>
        public static readonly int[] _ValidTextureFiltering =
        {
            0, 2, 4, 8, 16
        };

        /// <summary> Collection of valid anti-aliasing values </summary>
        public static readonly int[] _ValidAntiAliasing =
        {
            0, 2, 4, 8
        };
    }
}
