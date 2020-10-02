using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;

namespace TTLib
{
    /// <summary> Represents the configuration of the launcher. </summary> 
    public class Config
    {
        /// <summary> Manages game versions within the configuration. </summary> 
        public class Version
        {
            /// <summary> Adds a version. </summary> 
            /// <param name="_gameVersion"> The version to add. </param>
            /// <param name="_gamePath"> The folder path where the game is located. </param>
            /// <param name="_gameLanguage"> The language of the version. </param>
            public static void Add(string _gameVersion , string _gamePath, string _gameLanguage)
            {
                Config.Version.Properties.ApplyDefaults(_gameVersion);
                var _rk = Registry.LocalMachine.CreateSubKey($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", true);
                _rk.SetValue("Path", _gamePath, RegistryValueKind.String);
                _rk.SetValue("Language", _gameLanguage, RegistryValueKind.String);
                _rk.Close();
            }

            /// <summary> Removes a version. </summary> 
            /// <param name="_gameVersion"> The version to remove. </param>
            public static void Remove(string _gameVersion)
            {
                Registry.LocalMachine.DeleteSubKeyTree($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", false);
            }

            /// <summary> Checks if version exists. </summary> 
            /// <param name="_gameVersion"> The game version to check. </param>
            public static bool Exists(string _gameVersion)
            {
                var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", false);
                string[] _kn = _rk.GetSubKeyNames();
                for (int i = 0; i < _kn.Length; i++)
                {
                    if (_kn[i].ToLower() == _gameVersion.ToLower())
                    {
                        _rk.Close(); return true;
                    }
                }
                _rk.Close(); return false;
            }

            /// <summary> Checks if any version exists. </summary> 
            public static bool ExistsAny()
            {
                var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}");
                if (_rk.SubKeyCount > 0) 
                {
                    var _kn = _rk.GetSubKeyNames();
                    for (int i = 0; i < TTLib.Properties._ValidVersions.Length; i++)
                    {
                        for (int j = 0; j < _kn.Length; j++)
                        {
                            if (TTLib.Properties._ValidVersions[i].ToLower() == _kn[j].ToLower())
                            {
                                _rk.Close(); return true;
                            }
                        }
                    }
                }
                _rk.Close(); return false;
            }

            /// <summary> Checks if version configuration is valid. </summary> 
            /// <param name="_gameVersion"> The game version to check. </param>
            public static bool IsValid(string _gameVersion)
            {
                if (Config.Version.Properties.Get(_gameVersion, "Path").ToString().ToLower() == "unset"
                    || Config.Version.Properties.Get(_gameVersion, "Language").ToString().ToLower() == "unset")
                {
                    return false;
                }
                return true;
            }

            /// <summary> Removes versions that doesn't actually exist. </summary>
            public static void RemoveInvalidVersions()
            {
                var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", true);
                if (_rk.SubKeyCount <= 0) 
                { 
                    _rk.Close(); return; 
                }
                string[] _kn = _rk.GetSubKeyNames();
                bool _f = false;
                for (int i = 0; i < _kn.Length; i++)
                {
                    for (int j = 0; j < TTLib.Properties._ValidVersions.Length; j++)
                    {
                        if (_kn[i].ToLower() == TTLib.Properties._ValidVersions[j].ToLower())
                        {
                            _f = true;
                            break;
                        }
                    }
                    if (!_f) 
                    {
                        _rk.DeleteSubKeyTree(_kn[i], false);
                    }
                    _f = false;
                }
                _rk.Close();
            }

            /// <summary> Returns an Array of all existing versions. </summary>
            public static string[] GetAll()
            {
                var _rk = Registry.LocalMachine.OpenSubKey(TTLib.Properties._LauncherKeyPath);
                var _v = new List<string>();
                var _kn = _rk.GetSubKeyNames();
                for (int i = 0; i < _kn.Length; i++)
                {
                    _v.Add(_kn[i]);
                }
                return _v.ToArray();
            }

            /// <summary> Checks if version needs an update. </summary> 
            /// <param name="_gameVersion"> The game version to check. </param>
            public static bool NeedsUpdate(string _gameVersion)
            {
                if ((int)Config.Version.Properties.Get(_gameVersion, "Update") == 1)
                {
                    return true;
                }
                return false;
            }

            /// <summary> Manages properties of a version. </summary> 
            public class Properties
            {
                /// <summary> Sets the value of a property. </summary> 
                /// <param name="_gameVersion"> The game version the property. </param>
                /// <param name="_gameProperty"> The property to change. </param>
                /// <param name="_propertyValue"> The value of the property. </param>
                public static void Set(string _gameVersion, string _gameProperty, object _propertyValue)
                {
                    var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", true);
                    if (_propertyValue.GetType().Equals(typeof(string)))
                    {
                        _rk.SetValue(_gameProperty, _propertyValue, RegistryValueKind.String);
                    }
                    else if (_propertyValue.GetType().Equals(typeof(int)) || _propertyValue.GetType().Equals(typeof(bool)))
                    {
                        _rk.SetValue(_gameProperty, _propertyValue, RegistryValueKind.DWord);
                    }
                    _rk.Close();
                }

                /// <summary> Gets the value of a property. </summary> 
                /// <param name="_gameVersion"> The game version of the property. </param>
                /// <param name="_gameProperty"> The property to get the value from. </param>
                public static object Get(string _gameVersion, string _gameProperty)
                {
                    var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", false);
                    var _value = _rk.GetValue(_gameProperty);
                    _rk.Close(); return _value;
                }

                /// <summary> Applies default properties to a version. </summary> 
                /// <param name="_gameVersion"> The version default properties should be applied to. </param>
                public static void ApplyDefaults(string _gameVersion)
                {
                    var _rk = Registry.LocalMachine.CreateSubKey($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", true);
                    var _vn = _rk.GetValueNames();

                    for (int i = 0; i < _vn.Length; i++)
                    {
                        _rk.DeleteValue(_vn[i], false);
                    }   
                    
                    for (int i = 0; i < TTLib.Properties._ValidVersionPropertiesString.Length; i++)
                    {
                        _rk.SetValue(TTLib.Properties._ValidVersionPropertiesString[i], TTLib.Properties._DefaultVersionPropertiesString[i], RegistryValueKind.String);
                    }

                    for (int i = 0; i < TTLib.Properties._ValidVersionPropertiesDWord.Length; i++)
                    {
                        _rk.SetValue(TTLib.Properties._ValidVersionPropertiesDWord[i], TTLib.Properties._DefaultVersionPropertiesDWord[i], RegistryValueKind.DWord);
                    }

                    _rk.Close();
                }

                /// <summary> Checks if a property exists. </summary>
                /// <param name="_gameVersion"> The property's version to check. </param>
                /// <param name="_property"> The property to check. </param>
                public static bool Exists(string _gameVersion, string _property)
                {
                    var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", false);
                    if (_rk.GetValue(_property, null) == null) 
                    { 
                        _rk.Close(); return false; 
                    }
                    _rk.Close(); return true;
                }

                /// <summary> Checks if all properties exist. </summary>
                /// <param name="_gameVersion"> The version to check. </param>
                public static bool ExistsAll(string _gameVersion)
                {
                    var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", false);

                    for (int i = 0; i < TTLib.Properties._ValidVersionPropertiesString.Length; i++)
                    {
                        if (!Config.Version.Properties.Exists(_gameVersion, TTLib.Properties._ValidVersionPropertiesString[i])) 
                        { 
                            _rk.Close(); return false; 
                        }
                    }

                    for (int i = 0; i < TTLib.Properties._ValidVersionPropertiesDWord.Length; i++)
                    {
                        if (!Config.Version.Properties.Exists(_gameVersion, TTLib.Properties._ValidVersionPropertiesDWord[i]))
                        {
                            _rk.Close(); return false;
                        }
                    }

                    _rk.Close(); return true;
                }

                /// <summary> Repairs version if properties in configuration are invalid. </summary>
                /// <param name="_gameVersion"> The version to repair. </param>
                public static void Repair(string _gameVersion)
                {
                    var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}{_gameVersion}", true);
                    string _ph;
                    bool _f;

                    if (_rk.GetValue("Path", null) == null || _rk.GetValueKind("Path") != RegistryValueKind.String)
                    {
                        _rk.SetValue("Path", "unset", RegistryValueKind.String);
                    }

                    _ph = (string)_rk.GetValue("Path");
                    if (!Directory.Exists(_ph))
                    {
                        _rk.SetValue("Path", "unset", RegistryValueKind.String);
                    }

                    if (_rk.GetValue("Language", null) == null || _rk.GetValueKind("Language") != RegistryValueKind.String)
                    {
                        _rk.SetValue("Language", "unset", RegistryValueKind.String);
                    }

                    _f = false;
                    _ph = _rk.GetValue("Language").ToString().ToLower();
                    string _p = (string)Config.Version.Properties.Get(_gameVersion, "Path");

                    if (_p != null && _p.ToLower() != "unset")
                    {
                        if (Directory.Exists($@"{_p}GAMEDATA\WORLD\SOUND\"))
                        {
                            string[] _vl = Game.GetLanguages($@"{_p}GAMEDATA\WORLD\SOUND\");
                            for (int i = 0; i < _vl.Length; i++)
                            {
                                if (_ph == _vl[i].ToLower())
                                {
                                    _f = true; break;
                                }
                            }
                        }
                    }
                    if (!_f) 
                    { 
                        _rk.SetValue("Language", "unset", RegistryValueKind.String); 
                    }

                    if (_rk.GetValue("DGVoodooWatermark", null) == null || _rk.GetValueKind("DGVoodooWatermark") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("DGVoodooWatermark", false, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("Fullscreen", null) == null || _rk.GetValueKind("Fullscreen") != RegistryValueKind.DWord 
                        || (int)_rk.GetValue("Fullscreen") > 1)
                    {
                        _rk.SetValue("Fullscreen", false, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("ResWidth", null) == null 
                        || _rk.GetValue("ResHeight", null) == null 
                        || _rk.GetValueKind("ResWidth") != RegistryValueKind.DWord 
                        || _rk.GetValueKind("ResHeight") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("ResHeight", 768, RegistryValueKind.DWord);
                        _rk.SetValue("ResWidth", 1024, RegistryValueKind.DWord);
                        
                    }

                    int _rw = (int)_rk.GetValue("ResWidth");
                    int _rh = (int)_rk.GetValue("ResHeight");
                    _f = false;

                    for (int i = 0; i < TTLib.Properties._ValidResolutionWidths.Length; i++)
                    {
                        if (_rw == TTLib.Properties._ValidResolutionWidths[i] && _rh == TTLib.Properties._ValidResolutionHeights[i])
                        {
                            _f = true; break;
                        }
                    }

                    if (!_f)
                    {
                        _rk.SetValue("ResHeight", 768, RegistryValueKind.DWord);
                        _rk.SetValue("ResWidth", 1024, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("AntiAliasing", null) == null || _rk.GetValueKind("AntiAliasing") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("AntiAliasing", 0, RegistryValueKind.DWord);
                    }
                    else if ((int)_rk.GetValue("AntiAliasing") != 0
                            && (int)_rk.GetValue("AntiAliasing") != 2
                            && (int)_rk.GetValue("AntiAliasing") != 4
                            && (int)_rk.GetValue("AntiAliasing") != 8)
                    {
                        _rk.SetValue("AntiAliasing", 0, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("TextureFiltering", null) == null || _rk.GetValueKind("TextureFiltering") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("TextureFiltering", 4, RegistryValueKind.DWord);
                    }
                    else if ((int)_rk.GetValue("TextureFiltering") != 0
                            && (int)_rk.GetValue("TextureFiltering") != 2
                            && (int)_rk.GetValue("TextureFiltering") != 4
                            && (int)_rk.GetValue("TextureFiltering") != 8
                            && (int)_rk.GetValue("TextureFiltering") != 16)
                    {
                        _rk.SetValue("TextureFiltering", 4, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("BilinearBlitStretch", null) == null 
                            || _rk.GetValueKind("BilinearBlitStretch") != RegistryValueKind.DWord 
                            || (int)_rk.GetValue("BilinearBlitStretch") > 1)
                    {
                        _rk.SetValue("BilinearBlitStretch", true, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("AppControlledWindowState", null) == null || _rk.GetValueKind("AppControlledWindowState") != RegistryValueKind.DWord 
                        || (int)_rk.GetValue("AppControlledWindowState") > 1)
                    {
                        _rk.SetValue("AppControlledWindowState", false, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("TotalHour", null) == null || _rk.GetValueKind("TotalHour") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("TotalHour", 0, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("TotalMin", null) == null || _rk.GetValueKind("TotalMin") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("TotalMin", 0, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("LastDay", null) == null 
                        || _rk.GetValue("LastMonth", null) == null 
                        || _rk.GetValue("LastYear", null) == null 
                        || _rk.GetValueKind("LastDay") != RegistryValueKind.DWord 
                        || _rk.GetValueKind("LastMonth") != RegistryValueKind.DWord 
                        || _rk.GetValueKind("LastYear") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("LastDay", 0, RegistryValueKind.DWord);
                        _rk.SetValue("LastMonth", 0, RegistryValueKind.DWord);
                        _rk.SetValue("LastYear", 0, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("Size", null) == null || _rk.GetValueKind("Size") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("Size", 0, RegistryValueKind.DWord);
                    }

                    if (_rk.GetValue("Update", null) == null || _rk.GetValueKind("Update") != RegistryValueKind.DWord)
                    {
                        _rk.SetValue("Update", 0, RegistryValueKind.DWord);
                    }

                    _rk.Close();
                }
            }
        }

        /// <summary> Creates new configuration with default properties. </summary> 
        public static void Create()
        {
            Config.Properties.ApplyDefaults();
            var _rk = Registry.LocalMachine.OpenSubKey(TTLib.Properties._LauncherRootKeyPath, true);
            var _kn =_rk.GetSubKeyNames();
            for (int i = 0; i < _kn.Length; i++)
            {
                if (_kn[i].ToLower() != "Launcher".ToLower())
                {
                    _rk.DeleteSubKeyTree(_kn[i], false);
                }
            }
            _rk.Close();
        }

        /// <summary> Completely removes the configuration. </summary> 
        public static void Remove()
        {
            Registry.LocalMachine.DeleteSubKeyTree($"{TTLib.Properties._LauncherRootKeyPath}", false);
        }

        /// <summary> Checks if version of the configuration is obsolete. </summary> 
        public static bool IsObsolete()
        {
            var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", true);
            if ((string)_rk.GetValue("Version") != TTLib.Properties._LauncherVersion)
            {
                _rk.Close(); return true;
            }
            _rk.Close(); return false;
        }

        /// <summary> Updates version of the configuration. </summary> 
        public static void Update()
        {
            var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", true);
            _rk.SetValue("Version", TTLib.Properties._LauncherVersion);
            _rk.Close();
        }

        /// <summary> Checks if configuration exists. </summary> 
        public static bool Exists()
        {
            var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", false);
            if (_rk != null) 
            {
                _rk.Close(); return true; 
            }
            return false;
        }

        /// <summary> Checks if configuration is valid. </summary> 
        public static bool IsValid()
        {
            var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", false);

            if (!Config.Properties.ExistsAll())
            {
                _rk.Close(); return false;
            }

            for (int i = 0; i < TTLib.Properties._ValidLauncherPropertiesString.Length; i++)
            {
                if (_rk.GetValueKind(TTLib.Properties._ValidLauncherPropertiesString[i]) != RegistryValueKind.String) { return false; }
            }

            for (int i = 0; i < TTLib.Properties._ValidLauncherPropertiesDWord.Length; i++)
            {
                if (_rk.GetValueKind(TTLib.Properties._ValidLauncherPropertiesDWord[i]) != RegistryValueKind.DWord) { return false; }
            }

            var _p = (string)_rk.GetValue("LastPath");
            if (_p.ToLower() != "unset" && !Directory.Exists(_p))
            {
                _rk.Close(); return false;
            }

            if (_rk.GetValueKind("RunningVersion") != RegistryValueKind.String)
            {
                _rk.Close(); return false;
            }

            if (_rk.GetValueKind("LastDiscordTimeHours") != RegistryValueKind.DWord)
            {
                _rk.Close(); return false;
            }

            if (_rk.GetValueKind("LastDiscordTimeMinutes") != RegistryValueKind.DWord)
            {
                _rk.Close(); return false;
            }

            if (_rk.GetValueKind("LastDiscordTimeSeconds") != RegistryValueKind.DWord)
            {
                _rk.Close(); return false;
            }

            _rk.Close(); return true;
        }

        /// <summary> Manages properties of the configuration. </summary> 
        public class Properties
        {
            /// <summary> Sets the value of a property. </summary> 
            /// <param name="_configProperty"> The property to change. </param>
            /// <param name="_propertyValue"> The value of the property. </param>
            public static void Set(string _configProperty, object _propertyValue)
            {
                var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", true);
                if (_propertyValue.GetType().Equals(typeof(string))) 
                { 
                    _rk.SetValue(_configProperty, _propertyValue, RegistryValueKind.String); 
                }
                else if (_propertyValue.GetType().Equals(typeof(bool))
                    || _propertyValue.GetType().Equals(typeof(int))) 
                { 
                    _rk.SetValue(_configProperty, _propertyValue, RegistryValueKind.DWord); 
                }
                _rk.Close();
            }

            /// <summary> Gets a value of the property. </summary> 
            /// <param name="_configProperty"> The property to get the value from. </param>
            public static object Get(string _configProperty)
            {
                var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", false);
                var _v = _rk.GetValue(_configProperty);
                _rk.Close(); return _v;
            }

            /// <summary> Applies default properties to the configuration. </summary> 
            public static void ApplyDefaults()
            {
                var _rk = Registry.LocalMachine.CreateSubKey($"{TTLib.Properties._LauncherKeyPath}", true);
                var _vn = _rk.GetValueNames();

                for (int i = 0; i < _vn.Length; i++)
                {
                    _rk.DeleteValue(_vn[i], false);
                }

                for (int i = 1; i < TTLib.Properties._ValidLauncherPropertiesString.Length; i++)
                {
                    _rk.SetValue(TTLib.Properties._ValidLauncherPropertiesString[i], TTLib.Properties._DefaultLauncherPropertiesString[i]);
                }

                for (int i = 0; i < TTLib.Properties._ValidLauncherPropertiesDWord.Length; i++)
                {
                    _rk.SetValue(TTLib.Properties._ValidLauncherPropertiesDWord[i], TTLib.Properties._DefaultLauncherPropertiesDWord[i]);
                }

                _rk.SetValue("Version", TTLib.Properties._LauncherVersion, RegistryValueKind.String);
                _rk.Close();
            }

            /// <summary> Checks if a property exists. </summary>
            /// <param name="_property"> The property to check. </param>
            public static bool Exists(string _property)
            {
                var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", false);
                if (_rk.GetValue(_property, null) == null)
                {
                    _rk.Close(); return false;
                }
                _rk.Close(); return true;
            }

            /// <summary> Checks if all properties exist. </summary>
            public static bool ExistsAll()
            {
                var _rk = Registry.LocalMachine.OpenSubKey($"{TTLib.Properties._LauncherKeyPath}", false);
                for (int i = 0; i < TTLib.Properties._ValidLauncherPropertiesString.Length; i++)
                {
                    if (!Config.Properties.Exists(TTLib.Properties._ValidLauncherPropertiesString[i]))
                    {
                        _rk.Close(); return false;
                    }
                }

                for (int i = 0; i < TTLib.Properties._ValidLauncherPropertiesDWord.Length; i++)
                {
                    if (!Config.Properties.Exists(TTLib.Properties._ValidLauncherPropertiesDWord[i]))
                    {
                        _rk.Close(); return false;
                    }
                }

                _rk.Close(); return true;
            }
        }
    }

    /// <summary> Represents the game. </summary> 
    public class Game
    {
        /// <summary> Returns an array of languages that the game supports. </summary>
        /// <param name="_path"> The '..\GameData\World\Sound\' folder location. </param>
        public static string[] GetLanguages(string _path)
        {
            var _l = new List<string>();
            foreach (var _d in Directory.GetDirectories(_path))
            {
                var _dir = new DirectoryInfo(_d);
                _l.Add(_dir.Name);
            }
            return _l.ToArray();
        }

        private static long GetSize(DirectoryInfo _directoryInfo)
        {
            long _size = 0;
            FileInfo[] _fi = _directoryInfo.GetFiles();
            foreach (FileInfo _fis in _fi)
            {
                try { _size += _fis.Length; }
                catch { continue; }
            }
            DirectoryInfo[] _di = _directoryInfo.GetDirectories();
            foreach (DirectoryInfo _dir in _di)
            {
                try { _size += GetSize(_dir); }
                catch { continue; }
            }
            return _size;
        }

        /// <summary> Gets the size of the game in bytes. </summary>
        /// <param name="_directoryInfo"> The game directory to calculate. </param>
        public static async Task<long> GetSizeAsync(DirectoryInfo _directoryInfo)
        {
            return await Task.Run(() => GetSize(_directoryInfo));
        }

        /// <summary> Checks wether Indeo Drivers are installed or not. </summary>
        public static bool IndeoDriversInstalled()
        {
            RegistryKey _rk;
            if (Environment.Is64BitOperatingSystem) { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); }
            else { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); }

            _rk = _rk.OpenSubKey(Properties._IndeoPath, false);
            if (_rk == null) { return false; }

            var _kn = _rk.GetSubKeyNames();
            bool _f = false;
            for (int i = 0; i < _kn.Length; i++)
            {
                if (_kn[i].ToLower() == "vidc.iv32" || _kn[i].ToLower() == "vidc.iv41") { _f = true; break; }
            }
            if (!_f) { return false; }

            _rk.Close();

            if (Environment.Is64BitOperatingSystem) { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); }
            else { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); }

            _rk = _rk.OpenSubKey($@"{Properties._IndeoPath}vidc.iv32\", false);
            var _vn = _rk.GetValueNames();

            _f = false;
            for (int i = 0; i < _vn.Length; i++)
            {
                if (_vn[i].ToLower() == "driver") { _f = true; break; }
                else { continue; }
            }
            if (!_f) { return false; }
            if (!_rk.GetValue("Driver").GetType().Equals(typeof(string)) || (string)_rk.GetValue("Driver").ToString().ToLower() != "ir32_32.dll") { _rk.Close(); return false; }

            _rk.Close();

            if (Environment.Is64BitOperatingSystem) { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); }
            else { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); }

            _rk = _rk.OpenSubKey($@"{Properties._IndeoPath}vidc.iv41\", false);
            _vn = _rk.GetValueNames();

            _f = false;
            for (int i = 0; i < _vn.Length; i++)
            {
                if (_vn[i].ToLower() == "driver") { _f = true; break; }
                else { continue; }
            }
            if (!_f) { return false; }
            if (!_rk.GetValue("Driver").GetType().Equals(typeof(string)) || (string)_rk.GetValue("Driver").ToString().ToLower() != "ir41_32.dll") { _rk.Close(); return false; }

            _rk.Close();

            _rk = Registry.LocalMachine.OpenSubKey($"{Properties._IndeoPathAlt}");
            if (_rk == null) { return false; }

            _vn = _rk.GetValueNames();

            _f = false;
            bool _f2 = false;
            for (int i = 0; i < _vn.Length; i++)
            {
                if (_f && !_f2)
                {
                    if (_vn[i].ToLower() == "vidc.iv41") { _f2 = true; break; }
                }
                else
                {
                    if (_vn[i].ToLower() == "vidc.iv32") { _f = true; i = 0; continue; }
                }
            }
            if (!_f || !_f2) { return false; }

            if (!_rk.GetValue("vidc.iv32").GetType().Equals(typeof(string)) || (string)_rk.GetValue("vidc.iv32").ToString().ToLower() != "ir32_32.dll") { _rk.Close(); return false; }
            if (!_rk.GetValue("vidc.iv41").GetType().Equals(typeof(string)) || (string)_rk.GetValue("vidc.iv41").ToString().ToLower() != "ir41_32.dll") { _rk.Close(); return false; }

            _rk.Close();

            if (Environment.Is64BitOperatingSystem) 
            {
                if (!File.Exists(@"C:\Windows\SysWOW64\ir32_32.dll") || !File.Exists(@"C:\Windows\SysWOW64\ir41_32.dll")) { return false; }
            }
            else 
            {
                if (!File.Exists(@"C:\Windows\System32\ir32_32.dll") || !File.Exists(@"C:\Windows\System32\ir41_32.dll")) { return false; }
            }

             return true;
        }

        /// <summary> Installs Indeo Drivers. </summary>
        public static void InstallIndeoDrivers(byte[] _ir32, byte[] _ir41)
        {
            RegistryKey _rk;
            if (Environment.Is64BitOperatingSystem) { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); }
            else { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); }

            _rk.CreateSubKey($"{Properties._IndeoPath}vidc.iv32", true);
            _rk.CreateSubKey($"{Properties._IndeoPath}vidc.iv41", true);

            _rk = _rk.OpenSubKey($"{Properties._IndeoPath}vidc.iv32", true);
            _rk.SetValue("Description", "Indeo® video R3.2 by Intel", RegistryValueKind.String);
            _rk.SetValue("Driver", "ir32_32.dll", RegistryValueKind.String);
            _rk.SetValue("FriendlyName", "Indeo® video R3.2 by Intel", RegistryValueKind.String);

            _rk.Close();

            if (Environment.Is64BitOperatingSystem) { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); }
            else { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); }

            _rk = _rk.OpenSubKey($"{Properties._IndeoPath}vidc.iv41", true);
            _rk.SetValue("Description", "Indeo® video interactive R4.1 by Intel", RegistryValueKind.String);
            _rk.SetValue("Driver", "ir41_32.dll", RegistryValueKind.String);
            _rk.SetValue("FriendlyName", "Indeo® video interactive R4.1 by Intel", RegistryValueKind.String);

            _rk.Close();

            _rk = Registry.LocalMachine.OpenSubKey($"{Properties._IndeoPathAlt}", true);
            _rk.SetValue("vidc.iv32", "ir32_32.dll");
            _rk.SetValue("vidc.iv41", "ir41_32.dll");

            _rk.Close();

            string _ir32x64_86 = $@"{Environment.SystemDirectory}\ir32_32.dll";
            string _ir41x64_86 = $@"{Environment.SystemDirectory}\ir41_32.dll";

            if (File.Exists(_ir32x64_86)) 
            {
                if (File.Exists($"{_ir32x64_86}_ttl.bak"))
                {
                    try { File.Delete($"{_ir32x64_86}_ttl.bak"); }
                    catch (UnauthorizedAccessException)
                    {
                        FileAccess.GetAccessOfFile($"{_ir32x64_86}_ttl.bak");
                        File.Delete($"{_ir32x64_86}_ttl.bak");
                    }
                }

                try { File.Move(_ir32x64_86, $"{_ir32x64_86}_ttl.bak"); }
                catch (UnauthorizedAccessException)
                {
                    FileAccess.GetAccessOfFile(_ir32x64_86);
                    File.Move(_ir32x64_86, $"{_ir32x64_86}_ttl.bak");
                }
            }

            if (File.Exists(_ir41x64_86)) 
            {
                if (File.Exists($"{_ir41x64_86}_ttl.bak"))
                {
                    try { File.Delete($"{_ir41x64_86}_ttl.bak"); }
                    catch (UnauthorizedAccessException)
                    {
                        FileAccess.GetAccessOfFile($"{_ir41x64_86}_ttl.bak");
                        File.Delete($"{_ir41x64_86}_ttl.bak");
                    }
                }

                try { File.Move(_ir41x64_86, $"{_ir41x64_86}_ttl.bak"); }
                catch (UnauthorizedAccessException)
                {
                    FileAccess.GetAccessOfFile(_ir41x64_86);
                    File.Move(_ir41x64_86, $"{_ir41x64_86}_ttl.bak");
                }
            }

            File.WriteAllBytes(_ir32x64_86, _ir32);
            File.WriteAllBytes(_ir41x64_86, _ir41);
        }

        /// <summary> Uninstalls Indeo Drivers. </summary>
        public static void UninstallIndeoDrivers()
        {
            RegistryKey _rk;
            if (Environment.Is64BitOperatingSystem) { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); }
            else { _rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32); }

            _rk.DeleteSubKey($"{Properties._IndeoPath}vidc.iv32", false);
            _rk.DeleteSubKey($"{Properties._IndeoPath}vidc.iv41", false);

            _rk.Close();

            _rk = Registry.LocalMachine.OpenSubKey($"{Properties._IndeoPathAlt}", true);
            _rk.DeleteValue($"vidc.iv32", false);
            _rk.DeleteValue($"vidc.iv41", false);

            string _ir32x64_86 = $@"{Environment.SystemDirectory}\ir32_32.dll";
            string _ir41x64_86 = $@"{Environment.SystemDirectory}\ir41_32.dll";
            
            if (File.Exists(_ir32x64_86) && File.Exists($"{_ir32x64_86}_ttl.bak"))
            {
                try { File.Delete(_ir32x64_86); }
                catch (UnauthorizedAccessException) 
                {
                    FileAccess.GetAccessOfFile(_ir32x64_86); 
                    File.Delete(_ir32x64_86);
                }

                try { File.Move($"{_ir32x64_86}_ttl.bak", _ir32x64_86); }
                catch (UnauthorizedAccessException) 
                {
                    FileAccess.GetAccessOfFile($"{_ir32x64_86}_ttl.bak");
                    File.Move($"{_ir32x64_86}_ttl.bak", _ir32x64_86);
                }
            }

            if (File.Exists(_ir41x64_86))
            {
                try { File.Delete(_ir41x64_86); }
                catch (UnauthorizedAccessException)
                {
                    FileAccess.GetAccessOfFile(_ir41x64_86);
                    File.Delete(_ir41x64_86);
                }
                if (File.Exists($"{_ir41x64_86}_ttl.bak"))
                {
                    try { File.Move($"{_ir41x64_86}_ttl.bak", _ir41x64_86); }
                    catch (UnauthorizedAccessException)
                    {
                        FileAccess.GetAccessOfFile(_ir41x64_86);
                        File.Move($"{_ir41x64_86}_ttl.bak", _ir41x64_86);
                    }
                }
            }

            if (File.Exists(_ir32x64_86)) { FileAccess.RevertAccessOfFile(_ir32x64_86); }
            if (File.Exists(_ir41x64_86)) { FileAccess.RevertAccessOfFile(_ir41x64_86); }
        }

        /// <summary> Manages the executable of the game. </summary>
        public class Executable
        {
            private static string GetVersion(string _path, int _versionID)
            {
                string _rb = string.Empty;
                string _rbn = string.Empty;
                string _vr = string.Empty;
                string _a;

                var _br = new BinaryReader(File.OpenRead(_path));
                var _bl = new List<byte>();

                // Gets the binary in bytes and writes it into a string
                for (int i = 0; i < _br.BaseStream.Length; i++)
                {
                    _rb += _br.ReadByte().ToString("X2");
                    if (_rb.Length <= 8)
                    {
                        _rbn += Convert.ToString(Convert.ToInt32(_rb, 16), 2);
                        _bl.Add(Convert.ToByte(_rbn, 2));
                        _rb = string.Empty;
                        _rbn = string.Empty;
                    }
                }

                // Converts the string with the bytes in ascii
                _a = Encoding.ASCII.GetString(_bl.ToArray());

                // if Special Edition
                if (_versionID == 0)
                {
                    // Reads the version
                    for (int _posi = 0; _posi < _a.Length; _posi++)
                    {
                        // Orienting in 'TT V' which is unique
                        if (_a[_posi] == ('T') && _a[_posi + 1] == ('T') && _a[_posi + 2] == (' ') && _a[_posi + 3] == ('V'))
                        {
                            for (int _posj = _posi - 28; _posj < _a.Length; _posj++)
                            {
                                _vr += _a[_posj].ToString();
                                if (_vr.Length == 37) { break; }
                            }
                            _vr = _vr.Remove(11, 19); _br.Close();
                            return _vr.Remove(0, 12);
                        }
                    }
                }
                // if Retail Version
                else if (_versionID == 1)
                {
                    for (int _pos = 0; _pos < _a.Length; _pos++)
                    {
                        // Orienting in 'MyError.log' which is unique
                        if (_a[_pos] == ('M') && _a[_pos + 1] == ('y') && _a[_pos + 2] == ('E')
                            && _a[_pos + 3] == ('r') && _a[_pos + 4] == ('r') && _a[_pos + 5] == ('o')
                            && _a[_pos + 6] == ('r') && _a[_pos + 7] == ('.') && _a[_pos + 8] == ('l')
                            && _a[_pos + 9] == ('o') && _a[_pos + 10] == ('g'))
                        {
                            if (_a.ToString().Contains("TONIC TROUBLE"))
                            {
                                for (int rtPos = _pos + 26; rtPos < _a.Length; rtPos++)
                                {
                                    _vr += _a[rtPos].ToString();
                                    if (_vr.Contains(")")) { break; }
                                }
                                if (!_vr.Contains("-PC")) { _vr = _vr.Remove(_vr.IndexOf(':'), 12); }
                                else { _vr = _vr.Remove(_vr.IndexOf(':'), 24); }
                                 _br.Close();
                                if (_vr[_vr.Length - 1] == ' ') { _vr = _vr.Remove(_vr.Length - 1, 1); }
                                switch (_vr.ToLower())
                                {
                                    case "retail master v3": { _vr = "Retail Master English V3"; break; }
                                    case "retail master english": { _vr = "Retail Master English V3"; break; }
                                    case "retail master english v3": { _vr = "Retail Master English V3"; break; }
                                    case "retail master v5": { _vr = "Retail Master V5"; break; }
                                    case "retail master german v3": { _vr = "Retail Master German V3"; break; }
                                    case "review english": { _vr = "Review English"; break; }
                                }
                                return _vr;
                            }
                            /*else
                            {
                                for (int rtPos = pos + 12; rtPos < _ascii.Length; rtPos++)
                                {
                                    _vr += _a[rtPos].ToString();
                                    if (_vr.Contains(")")) { break; }
                                }
                                _br.Close();
                                return _vr.Remove(_vr.IndexOf(':'), 24);
                            }*/
                        }
                    }
                }
                _br.Close(); return null;
            }

            /// <summary> Checks the version of the game executable. </summary> 
            /// <param name="_path"> Executable path. </param>
            /// <param name="_versionID"> Special Edition: 0 | Retail Version: 1 (Retail only works with a patched executable) </param>
            public static async Task<string> GetVersionAsync(string _path, int _versionID)
            {
                return await Task.Run(() => GetVersion(_path, _versionID));
            }
        }
    }

    /// <summary> Manages files that need to have certain permissions to be able to modify. </summary>
    public class FileAccess
    {
        /// <summary> Gets full control of a file. </summary>
        /// <param name="_fileName"> Name of the file to get access of. </param>
        public static void GetAccessOfFile(string _fileName)
        {
            var _pi = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/k takeown /f {_fileName} && icacls {_fileName} /grant %userdomain%\\Administrators:F"
            };
            _pi.UseShellExecute = true;
            _pi.Verb = "runas";

            var _p = new Process { StartInfo = _pi };
            _p.Start();

            var _usid = WindowsIdentity.GetCurrent().User;
            var _fs = File.GetAccessControl(_fileName);
            var _fsid = _fs.GetOwner(typeof(SecurityIdentifier));

            while (_usid != _fsid)
            {
                _fs = File.GetAccessControl(_fileName);
                _fsid = _fs.GetOwner(typeof(SecurityIdentifier));
            }
            _p.Kill();

            _fs.AddAccessRule(new FileSystemAccessRule(_usid, FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(_fileName, _fs);
        }

        /// <summary> Gets full control of a file. </summary>
        /// <param name="_fileName"> Name of the file to get access of. </param>
        public static void RevertAccessOfFile(string _fileName)
        {
            var _pi = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/k icacls {_fileName} /setowner \"NT SERVICE\\TrustedInstaller\""
            };
            _pi.UseShellExecute = true;
            _pi.Verb = "runas";

            var _p = new Process { StartInfo = _pi };
            _p.Start();

            var _usid = WindowsIdentity.GetCurrent().User;
            var _fs = File.GetAccessControl(_fileName);
            var _fsid = _fs.GetOwner(typeof(SecurityIdentifier));

            while (_usid == _fsid)
            {
                _fs = File.GetAccessControl(_fileName);
                _fsid = _fs.GetOwner(typeof(SecurityIdentifier));
            }
            _p.Kill();
        }
    }

    /// <summary> Represents the ubi.ini file. </summary>
    public class UbiConfig
    {
        /// <summary> Creates the ubi.ini file. </summary>
        /// <param name="_versionID"> Special Edition: 0 | Retail Version: 1 </param>
        /// <param name="_path"> Path to save into the file. </param>
        /// <param name="_language"> Language to save into the file. </param>
        public static void Create(int _versionID, string _path, string _language)
        {
            File.Create(Properties._UbiConfigPath).Dispose();
            if (_versionID == 0)
            {
                using (var _sw = new StreamWriter(Properties._UbiConfigPath))
                {
                    _sw.WriteLine("[TONICT]");
                    _sw.WriteLine($"Directory={_path}");
                    _sw.WriteLine($"Language={_language}");
                    _sw.WriteLine("Configuration=1");
                    _sw.WriteLine("Complete=0");
                    _sw.Close();
                }
            }
            else if (_versionID == 1)
            {
                using (var _sw = File.CreateText(Properties._UbiConfigPath))
                {
                    _sw.WriteLine("[TONICT]");
                    _sw.WriteLine($"Directory={_path}");
                    _sw.WriteLine($"Language={_language}");
                    _sw.WriteLine("Configuration=0");

                    if (File.Exists($@"{_path}\DLL\GLIDX6VR.DLL"))
                    {
                        var files = System.IO.Directory.GetFiles($@"{_path}\DLL\");
                        for (int i = 0; i < files.Length; i++)
                        {
                            if (files[i].ToLower().Contains("glidx6vr.dll"))
                            {
                                _sw.WriteLine($"GliDll={files[i].Remove(0, files[i].Length - 12)}");
                                break;
                            }
                        }
                    }

                    _sw.WriteLine("GliDriver=dgVoodoo DirectX Wrapper");
                    _sw.WriteLine("GliTexturesMemory=2048");
                    _sw.WriteLine("GliTexturesMode=Full");
                    _sw.WriteLine("GliBuffering=1");
                    _sw.WriteLine("GliAlphaTest=1");
                    _sw.Close();
                }
            }
        }

        /// <summary> Deletes the ubi.ini file. </summary>
        public static void Delete()
        {
            if (File.Exists(Properties._UbiConfigPath)) { File.Delete(Properties._UbiConfigPath); }
        }

        /// <summary> Creates a backup of the ubi.ini file. </summary>
        public static void Backup()
        {
            File.Move(Properties._UbiConfigPath, $"{Properties._UbiConfigPath}_ttl.bak");
        }

        /// <summary> Checks wether a backup of the ubi.ini file exists or not. </summary>
        public static bool BackupExists()
        {
            if (File.Exists($"{Properties._UbiConfigPath}_ttl.bak")) { return true; }
            return false;
        }

        /// <summary> Restores a backup of the ubi.ini file. </summary>
        public static void Restore()
        {
            File.Move($"{Properties._UbiConfigPath}_ttl.bak", Properties._UbiConfigPath);
        }

        /// <summary> Checks wether the ubi.ini file exists or not. </summary>
        public static bool Exists()
        {
            if (File.Exists(Properties._UbiConfigPath)) { return true; }
            return false;
        }

        /// <summary> Represents the UbiSoft folder located in C:\Windows. </summary>
        public class Directory
        {
            /// <summary> Creates the UbiSoft folder. </summary>
            public static void Create()
            {
                System.IO.Directory.CreateDirectory(Properties._UbiConfigDirectoryPath);
            }

            /// <summary> Deletes the UbiSoft folder located in C:\Windows. </summary>
            public static void Delete()
            {
                System.IO.Directory.Delete(Properties._UbiConfigDirectoryPath, true);
            }

            /// <summary> Checks wether the ubi.ini file exists or not. </summary>
            public static bool Exists()
            {
                if (System.IO.Directory.Exists(Properties._UbiConfigDirectoryPath)) { return true; }
                return false;
            }
        }
    }

    /// <summary> Represents the DGVoodoo installation. </summary>
    public class DGVoodoo
    {
        /// <summary> Installs DGVoodoo. </summary>
        public static void Install(string _path, byte[] _D3D8, byte[] _D3DImm, byte[] _DDraw, byte[] _Glide2x)
        {
            if (_D3D8 != null)
            {
                if (!File.Exists($"{_path}D3D8.dll")) { File.Delete($"{_path}D3D8.dll"); }
                File.WriteAllBytes($"{_path}D3D8.dll", _D3D8);
            }
            if (_D3DImm != null)
            {
                if (!File.Exists($"{_path}D3DImm.dll")) { File.Delete($"{_path}D3DImm.dll"); }
                File.WriteAllBytes($"{_path}D3DImm.dll", _D3DImm);
            }
            if (_DDraw != null)
            {
                if (!File.Exists($"{_path}DDraw.dll")) { File.Delete($"{_path}DDraw.dll"); }
                File.WriteAllBytes($"{_path}DDraw.dll", _DDraw);
            }
            if (_Glide2x != null)
            {
                if (!File.Exists($"{_path}Glide2x.dll")) { File.Delete($"{_path}Glide2x.dll"); }
                File.WriteAllBytes($"{_path}Glide2x.dll", _Glide2x);
            }
        }

        /// <summary> Manages the dgVoodoo.conf file. </summary>
        public class Config
        {
            /// <summary> Creates a dgVoodoo.conf file. </summary>
            /// <param name="_path"> Path of the config file. </param>
            public static void Create(string _path)
            {
                File.Create($"{_path}dgvoodoo.conf").Dispose();
                using (var _sw = new StreamWriter($"{_path}dgvoodoo.conf"))
                {
                    _sw.WriteLine(";==========================================================================");
                    _sw.WriteLine("; === Optimized for Tonic Trouble Special Edition & Retail Version");
                    _sw.WriteLine(";==========================================================================");
                    _sw.WriteLine("");
                    _sw.WriteLine("Version = 0x270");
                    _sw.WriteLine("");
                    _sw.WriteLine(";--------------------------------------------------------------------------");
                    _sw.WriteLine("");
                    _sw.WriteLine("[General]");
                    _sw.WriteLine("");
                    _sw.WriteLine("OutputAPI = bestavailable");
                    _sw.WriteLine("Adapters = all");
                    _sw.WriteLine("FullScreenOutput = default");
                    _sw.WriteLine("FullScreenMode = false");
                    _sw.WriteLine("ScalingMode = stretched_4_3");
                    _sw.WriteLine("ProgressiveScanlineOrder = false");
                    _sw.WriteLine("EnumerateRefreshRates = false");
                    _sw.WriteLine("");
                    _sw.WriteLine("Brightness = 100");
                    _sw.WriteLine("Color = 100");
                    _sw.WriteLine("Contrast = 100");
                    _sw.WriteLine("InheritColorProfileInFullScreenMode = false");
                    _sw.WriteLine("");
                    _sw.WriteLine("KeepWindowAspectRatio = true");
                    _sw.WriteLine("CaptureMouse = true");
                    _sw.WriteLine("CenterAppWindow = true");
                    _sw.WriteLine("");
                    _sw.WriteLine(";--------------------------------------------------------------------------");
                    _sw.WriteLine("");
                    _sw.WriteLine("[GeneralExt]");
                    _sw.WriteLine("");
                    _sw.WriteLine("DesktopResolution = ");
                    _sw.WriteLine("DesktopBitDepth = ");
                    _sw.WriteLine("DeframerSize = 1");
                    _sw.WriteLine("ImageScaleFactor = 1");
                    _sw.WriteLine("DisplayROI = ");
                    _sw.WriteLine("Resampling = bilinear");
                    _sw.WriteLine("FreeMouse = false");
                    _sw.WriteLine("WindowedAttributes = ");
                    _sw.WriteLine("Environment = ");
                    _sw.WriteLine("EnableGDIHooking = false");
                    _sw.WriteLine("");
                    _sw.WriteLine(";--------------------------------------------------------------------------");
                    _sw.WriteLine("");
                    _sw.WriteLine("[Glide]");
                    _sw.WriteLine("");
                    _sw.WriteLine("VideoCard = voodoo_2");
                    _sw.WriteLine("OnboardRAM = 8");
                    _sw.WriteLine("MemorySizeOfTMU = 4096");
                    _sw.WriteLine("NumberOfTMUs = 2");
                    _sw.WriteLine("TMUFiltering = appdriven");
                    _sw.WriteLine("DisableMipmapping = false");
                    _sw.WriteLine("Resolution = unforced");
                    _sw.WriteLine("Antialiasing = appdriven");
                    _sw.WriteLine("");
                    _sw.WriteLine("EnableGlideGammaRamp = true");
                    _sw.WriteLine("ForceVerticalSync = true");
                    _sw.WriteLine("ForceEmulatingTruePCIAccess = false");
                    _sw.WriteLine("16BitDepthBuffer = false");
                    _sw.WriteLine("3DfxWatermark = false");
                    _sw.WriteLine("3DfxSplashScreen = false");
                    _sw.WriteLine("PointcastPalette = false");
                    _sw.WriteLine("EnableInactiveAppState = false");
                    _sw.WriteLine("");
                    _sw.WriteLine(";--------------------------------------------------------------------------");
                    _sw.WriteLine("");
                    _sw.WriteLine("[GlideExt]");
                    _sw.WriteLine("");
                    _sw.WriteLine("DitheringEffect = pure32bit");
                    _sw.WriteLine("Dithering = forcealways");
                    _sw.WriteLine("DitherOrderedMatrixSizeScale = 0");
                    _sw.WriteLine("");
                    _sw.WriteLine(";--------------------------------------------------------------------------");
                    _sw.WriteLine("");
                    _sw.WriteLine("[DirectX]");
                    _sw.WriteLine("");
                    _sw.WriteLine("DisableAndPassThru = false");
                    _sw.WriteLine("");
                    _sw.WriteLine("VideoCard = internal3D");
                    _sw.WriteLine("VRAM = 256");
                    _sw.WriteLine("Filtering = appdriven");
                    _sw.WriteLine("DisableMipmapping = false");
                    _sw.WriteLine("Resolution = unforced");
                    _sw.WriteLine("Antialiasing = appdriven");
                    _sw.WriteLine("");
                    _sw.WriteLine("AppControlledScreenMode = false");
                    _sw.WriteLine("DisableAltEnterToToggleScreenMode = false");
                    _sw.WriteLine("");
                    _sw.WriteLine("BilinearBlitStretch = false");
                    _sw.WriteLine("PhongShadingWhenPossible = false");
                    _sw.WriteLine("ForceVerticalSync = false");
                    _sw.WriteLine("dgVoodooWatermark = false");
                    _sw.WriteLine("FastVideoMemoryAccess = false");
                    _sw.WriteLine("");
                    _sw.WriteLine(";--------------------------------------------------------------------------");
                    _sw.WriteLine("");
                    _sw.WriteLine("[DirectXExt]");
                    _sw.WriteLine("");
                    _sw.WriteLine("AdapterIDType = ");
                    _sw.WriteLine("VendorID = ");
                    _sw.WriteLine("DeviceID = ");
                    _sw.WriteLine("SubsystemID = ");
                    _sw.WriteLine("RevisionID = ");
                    _sw.WriteLine("");
                    _sw.WriteLine("DefaultEnumeratedResolutions = all");
                    _sw.WriteLine("ExtraEnumeratedResolutions = ");
                    _sw.WriteLine("EnumeratedResolutionBitdepths = all");
                    _sw.WriteLine("");
                    _sw.WriteLine("DitheringEffect = pure32bit");
                    _sw.WriteLine("Dithering = forcealways");
                    _sw.WriteLine("DitherOrderedMatrixSizeScale = 0");
                    _sw.WriteLine("DepthBuffersBitDepth = appdriven");
                    _sw.WriteLine("");
                    _sw.WriteLine("MaxVSConstRegisters = 256");
                    _sw.WriteLine("");
                    _sw.WriteLine("MSD3DDeviceNames = false");
                    _sw.WriteLine("RTTexturesForceScaleAndMSAA = true");
                    _sw.WriteLine("SmoothedDepthSampling = true");
                    _sw.WriteLine("DeferredScreenModeSwitch = false");
                    _sw.WriteLine("PrimarySurfaceBatchedUpdate = false");
                    _sw.WriteLine("");
                    _sw.WriteLine(";--------------------------------------------------------------------------");
                    _sw.WriteLine("");
                    _sw.WriteLine("[Debug]");
                    _sw.WriteLine("");
                    _sw.WriteLine("Info = enable");
                    _sw.WriteLine("Warning = enable");
                    _sw.WriteLine("Error = enable");
                    _sw.WriteLine("MaxTraceLevel = 0");
                    _sw.WriteLine("");
                    _sw.WriteLine("LogToFile = false");
                    _sw.WriteLine("");
                    _sw.Close();
                }
            }

            /// <summary> Deletes the dgVoodoo.conf file. </summary>
            /// <param name="_path"> Path of the config file. </param>
            public static void Delete(string _path)
            {
                File.Delete($"{_path}dgvoodoo.conf");
            }

            /// <summary> Checks wether dgVoodoo.conf file exists or not. </summary>
            /// <param name="_path"> Path of the config file. </param>
            public static bool Exists(string _path)
            {
                if (File.Exists($"{_path}dgvoodoo.conf")) { return true; }
                return false;
            }

            /// <summary> Manages the properties of the configuration. </summary>
            public class Properties
            {
                /// <summary> Sets the value of a property. </summary> 
                /// <param name="_path"> Path of the config file. </param>
                /// <param name="_configProperty"> The property to change. </param>
                /// <param name="_propertyValue"> The value of the property. </param>
                public static void Set(string _path, string _configProperty, object _propertyValue)
                {
                    int _li = 0;
                    string[] _l = File.ReadAllLines($"{_path}dgvoodoo.conf");

                    if (_configProperty.ToLower() == "antialiasing")
                    {
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("glide")) { _li++; }

                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains(_configProperty.ToLower())) { _li++; }
                        _l[_li] = $"Antialiasing = {_propertyValue}";

                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("directx")) { _li++; }

                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains(_configProperty.ToLower())) { _li++; }
                        _l[_li] = $"Antialiasing = {_propertyValue}";

                        File.Delete($"{_path}dgvoodoo.conf");
                        File.WriteAllLines($"{_path}dgvoodoo.conf", _l);

                        return;
                    }

                    if (_configProperty.ToLower() == "dgvoodoowatermark")
                    {
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("glide")) { _li++; }
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("3dfxwatermark")) { _li++; }

                        _l[_li] = $"3DfxWatermark = {_propertyValue}";

                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("directx")) { _li++; }
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains(_configProperty.ToLower())) { _li++; }

                        _l[_li] = $"dgVoodooWatermark = {_propertyValue}";

                        File.Delete($"{_path}dgvoodoo.conf");
                        File.WriteAllLines($"{_path}dgvoodoo.conf", _l);

                        return;
                    }

                    if (_configProperty.ToLower() == "resolution")
                    {
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("glide")) { _li++; }
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains(_configProperty.ToLower())) { _li++; }

                        _l[_li] = $"{_configProperty} = {_propertyValue}";

                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("directx")) { _li++; }
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains(_configProperty.ToLower())) { _li++; }
                        _l[_li] = $"{_configProperty} = {_propertyValue}";

                        File.Delete($"{_path}dgvoodoo.conf");
                        File.WriteAllLines($"{_path}dgvoodoo.conf", _l);

                        return;
                    }

                    if (_configProperty.ToLower() == "texturefiltering")
                    {
                        while (!_l.ElementAtOrDefault(_li).ToLower().Contains("filtering") || _l[_li].ToLower().Contains("tmufiltering")) { _li++; }

                        _l[_li] = $"Filtering = {_propertyValue}";

                        File.Delete($"{_path}dgvoodoo.conf");
                        File.WriteAllLines($"{_path}dgvoodoo.conf", _l);

                        return;
                    }

                    while (!_l.ElementAtOrDefault(_li).ToLower().Contains(_configProperty.ToLower())) { _li++; }
                    _l[_li] = $"{_configProperty} = {_propertyValue}";

                    File.Delete($"{_path}dgvoodoo.conf");
                    File.WriteAllLines($"{_path}dgvoodoo.conf", _l);
                }

                /// <summary> Checks if the specified property exists. </summary>
                /// <param name="_path"> Path of the config file. </param>
                /// <param name="_property"> The property to check. </param>
                public static bool Exists(string _path, string _property)
                {
                    string _l = File.ReadAllText($"{_path}dgvoodoo.conf");
                    if (_l.ToLower().Contains(_property.ToLower())) { return true; }
                    return false;
                }
            }
        }
    }
}