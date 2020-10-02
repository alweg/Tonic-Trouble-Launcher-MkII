using System;
using System.Threading;
using System.ComponentModel;
using DiscordRPC;
using Memory;
using TTLib;

namespace TTL.Launcher.DiscordRPC
{
    public class RPCApp
    {
        private static int processIdSE = 0;
        private static int processIdRT = 0;
        private static int processIdSEGlide = 0;
        private static int gamePlaying = 0; // 0: Launcher | 1: Special Edition | 2: Special Edition (Glide) | 3: Retail Version
        private static bool showElapsedTime = false;
        private static string currentLevel;
        private static string currentVersion;

        private static readonly Mem memory = new Mem();
        private static bool CheckCurrentGame(string game)
        {
            switch (game)
            {
                case ("Launcher"): { if (gamePlaying == 0) { return true; } break; }
                case ("SE"): { if (gamePlaying == 1) { return true; } break; }
                case ("SE_Glide"): { if (gamePlaying == 2) { return true; } break; }
                case ("RT"): { if (gamePlaying == 3) { return true; } break; }
            }
            return false;
        }

        /// <summary> Represents the RPC Application. </summary>
        public static class RPC
        {
            private static RichPresence richPresence = null;
            private static DiscordRpcClient client = null;

            /// <summary> Represents the RPC Server. </summary>
            public static class Server
            {
                /// <summary> Instance of the RPC Server. </summary>
                public static BackgroundWorker RPCServer = new BackgroundWorker();
                public static bool doneUpdate;

                private static void Initialize()
                {
                    richPresence = new RichPresence { Assets = new Assets() };
                    client = new DiscordRpcClient("593797049691013130");
                    client.Initialize();
                    richPresence.Details = ("In Launcher");
                    richPresence.State = ("Currently not In-Game");
                    richPresence.Assets.LargeImageKey = ("launcher_icon");
                    richPresence.Assets.LargeImageText = ("Launcher");
                    richPresence.Assets.SmallImageKey = ("");
                    client.SetPresence(richPresence);
                    RPCServer.WorkerSupportsCancellation = true;
                }

                /// <summary> Starts RPC Server. </summary>
                /// <param name="version"> The version that should be displayed. </param>
                public static void Start()
                {
                    RPCApp.RPC.Server.Initialize();
                    if (!RPCServer.IsBusy) { RPCServer.RunWorkerAsync(); RPCApp.RPC.Server.doneUpdate = false; }
                }

                /// <summary> Stops RPC Server. </summary>
                public static void Stop()
                {
                    if (RPCServer.WorkerSupportsCancellation == true) { RPCApp.RPC.Server.doneUpdate = true; RPCServer.CancelAsync(); }
                    if (RPC.client != null && !RPC.client.IsDisposed) { RPC.client.Dispose(); }
                }

                /// <summary> RPC Server Update. </summary>
                public static void Update(object sender, DoWorkEventArgs e)
                {
                    while (!RPCApp.RPC.Server.doneUpdate)
                    {
                        Thread.Sleep(2500);

                        processIdSE = memory.GetProcIdFromName("MaiD3Dvr");
                        processIdSEGlide = memory.GetProcIdFromName("MaiDFXvd");
                        processIdRT = memory.GetProcIdFromName("TonicTrouble");

                        switch ((string)Config.Properties.Get("RunningVersion"))
                        {
                            // special edition
                            case "SE-V8.1.0": { currentVersion = "Special Edition (V8.1.0)"; break; }
                            case "SE-V8.5.1": { currentVersion = "Special Edition (V8.5.1)"; break; }
                            case "SE-V8.5.2": { currentVersion = "Special Edition (V8.5.2)"; break; }
                            case "SE-V8.6.1": { currentVersion = "Special Edition (V8.6.1)"; break; }
                            case "SE-V8.6.2": { currentVersion = "Special Edition (V8.6.2)"; break; }
                            case "SE-V8.6.6": { currentVersion = "Special Edition (V8.6.6)"; break; }
                            case "SE-V8.6.8": { currentVersion = "Special Edition (V8.6.8)"; break; }
                            case "SE-V8.7.0": { currentVersion = "Special Edition (V8.7.0)"; break; }
                            case "SE-V8.7.4": { currentVersion = "Special Edition (V8.7.4)"; break; }

                            // retail version
                            case "RT-Retail Master V5": { currentVersion = "Retail Master V5"; break; }
                            case "RT-Retail Master English V3": { currentVersion = "Retail Master English V3"; break; }
                            case "RT-Retail Master German V3": { currentVersion = "Retail Master German V3"; break; }
                            case "RT-Review English": { currentVersion = "Review English"; break; }
                        }

                        if (processIdSE > 0) { gamePlaying = 1; }
                        else if (processIdSEGlide > 0) { gamePlaying = 2; }
                        else if (processIdRT > 0) { gamePlaying = 3; }
                        else 
                        { 
                            gamePlaying = 0;
                            Config.Properties.Set("LastDiscordTimeHours", 0);
                            Config.Properties.Set("LastDiscordTimeMinutes", 0);
                            Config.Properties.Set("LastDiscordTimeSeconds", 0);
                        }

                        if (CheckCurrentGame("Launcher"))
                        {
                            if (processIdSE > 0 && processIdSEGlide > 0 && processIdRT > 0)
                            {
                                try { memory.CloseProcess(); }
                                catch { }
                            }
                            if (showElapsedTime) { RPCApp.RPC.Presence.Change("In Launcher", "Currently not In-Game", "launcher_icon", "Launcher", "", false); }
                        }

                        else if (CheckCurrentGame("SE") || CheckCurrentGame("SE_Glide"))
                        {
                            if (CheckCurrentGame("SE")) { memory.OpenProcess(processIdSE); }
                            else if (CheckCurrentGame("SE_Glide")) { memory.OpenProcess(processIdSEGlide); }

                            for (int i = 0; i < Pointers.MenuCheckSE.GetUpperBound(0); i++)
                            {
                                if (Pointers.MenuCheckSE[i, 0] == (string)Config.Properties.Get("RunningVersion"))
                                {
                                    if (memory.ReadString(Pointers.MenuCheckSE[i, 1]) == "BkMenu.bmp"
                                    || memory.ReadString(Pointers.MenuCheckSE[i, 1]) == ""
                                    || memory.ReadString(Pointers.MenuCheckSE[i, 1]) == "ft.bmp"
                                    || memory.ReadString(Pointers.MenuCheckSE[i, 1]) == ".bmp")
                                    {
                                        currentLevel = "Main Menu"; break;
                                    }
                                    else { currentLevel = memory.ReadString(Pointers.LevelCheckSE[i, 1]); break; }
                                }
                            }

                            if (currentLevel != ("Main Menu") && !String.IsNullOrEmpty(currentLevel))
                            {
                                switch (currentLevel.ToLower())
                                {
                                    case "totalski": { currentLevel = "Ski Slope"; break; }
                                    case "sud0": { currentLevel = "South Plain"; break; }
                                    case "cavedoc": { currentLevel = "Doc's Cave"; break; }
                                    case "carota": { currentLevel = "Vegetable's HQ"; break; }
                                    case "north": { currentLevel = "North Plain"; break; }
                                    case "lecanyon": { currentLevel = "Canyon"; break; }
                                    case "cock01": { currentLevel = "Glacier Cocktail"; break; }
                                    case "pyramide": { currentLevel = "Pyramid"; break; }
                                    case "marmite": { currentLevel = "Pressure Cooker"; break; }
                                    case "land": { currentLevel = "Grogh's HQ"; break; }
                                    case "outro": { currentLevel = "Outro"; break; }
                                }
                            }

                            RPCApp.RPC.Presence.Change(currentVersion, $" - {currentLevel}", "ttse_icon", "Tonic Trouble: Special Edition", "ttse_logo", true);
                        }

                        else if (CheckCurrentGame("RT"))
                        {
                            memory.OpenProcess(processIdRT);

                            for (int i = 0; i < Pointers.MenuCheckRT.GetUpperBound(0); i++)
                            {
                                if (Pointers.MenuCheckRT[i, 0] == (string)Config.Properties.Get("RunningVersion"))
                                {
                                    if (memory.ReadByte(Pointers.MenuCheckRT[i, 2]) != 0)
                                    {
                                        if (memory.ReadByte(Pointers.MenuCheckRT[i, 1]) != 5
                                            || memory.ReadByte(Pointers.MenuCheckRT[i, 1]) != 4
                                            || memory.ReadByte(Pointers.MenuCheckRT[i, 1]) != 3
                                            || memory.ReadByte(Pointers.MenuCheckRT[i, 1]) != 2
                                            || memory.ReadByte(Pointers.MenuCheckRT[i, 1]) != 1
                                            || memory.ReadByte(Pointers.MenuCheckRT[i, 1]) != 0)
                                        {
                                            currentLevel = "Main Menu"; break;
                                        }
                                    }
                                    else { currentLevel = memory.ReadString(Pointers.LevelCheckRT[i, 1]); break; }
                                }
                            }

                            if (currentLevel != ("Main Menu") && !String.IsNullOrEmpty(currentLevel))
                            {
                                switch (currentLevel.ToLower())
                                {
                                    case "totalski": { currentLevel = "Ski Slope"; break; }
                                    case "sud0": { currentLevel = "South Plain"; break; }
                                    case "cavedoc": { currentLevel = "Doc's Cave"; break; }
                                    case "carota": { currentLevel = "Vegetable's HQ"; break; }
                                    case "north": { currentLevel = "North Plain"; break; }
                                    case "lecanyon": { currentLevel = "Canyon"; break; }
                                    case "cock01": { currentLevel = "Glacier Cocktail"; break; }
                                    case "pyramide": { currentLevel = "Pyramid"; break; }
                                    case "marmite": { currentLevel = "Pressure Cooker"; break; }
                                    case "land": { currentLevel = "Grogh's HQ"; break; }
                                    case "outro": { currentLevel = "Outro"; break; }
                                    case "death": { currentLevel = "Death Scene"; break; }
                                    case "bar": { currentLevel = "Cocktail Glacier (Cutscene)"; break; }
                                    case "bigboss": { currentLevel = "End Fight"; break; }
                                    case "mammyon": { currentLevel = "Canyon (Cutscene)"; break; }
                                    case "pharmmap": { currentLevel = "Pressure Cooker (Cutscene)"; break; }
                                    case "pyr_fou": { currentLevel = "Pyramid (Cutscene)"; break; }
                                    case "skymap": { currentLevel = "Crazy Town"; break; }
                                    case "spycarota": { currentLevel = "Vegetable's HQ (Cutscene)"; break; }
                                    case "spynorth": { currentLevel = "North Plain (Cutscene)"; break; }
                                    case "transit": { currentLevel = "Transit (Training flight)"; break; }
                                }
                            }

                            RPCApp.RPC.Presence.Change(currentVersion, $" - {currentLevel}", "ttrt_icon", "Tonic Trouble: Retail Version", "ttrt_logo", true);
                        }
                    }
                }

                /// <summary> Checks whether RPC Server is running or not. </summary>
                public static bool IsRunning()
                {
                    if (!doneUpdate) { return true; }
                    return false;
                }
            }

            /// <summary> Represents the Rich Presence. </summary>
            public static class Presence
            {
                /// <summary> Changes Discord's Rich Presence Details. </summary>
                /// <param name="Details"> Title (Version of the game) </param>
                /// <param name="State"> The current level. </param>
                /// <param name="LargeImageKey"> The icon of the game. </param>
                /// <param name="LargeImageText"> The version of the game</param>
                /// <param name="SmallImageKey"> The logo of the game. </param>
                /// <param name="showTime"> Wether to show elapsed time or not. (Launcher = false | In-Game = true)) </param>
                public static void Change(string title, string level, string icon, string version, string logo, bool showTime = false)
                {
                    if (richPresence.Timestamps == null) { richPresence.Timestamps = new Timestamps(); }
                    richPresence.Details = title;
                    richPresence.State = level;
                    richPresence.Assets.LargeImageKey = icon;
                    richPresence.Assets.LargeImageText = version;
                    richPresence.Assets.SmallImageKey = logo;
                    if (showTime && !showElapsedTime)
                    {
                        if (!Config.IsValid()) { Config.Properties.ApplyDefaults(); }

                        TimeSpan elapsedTime = new TimeSpan((int)Config.Properties.Get("LastDiscordTimeHours"), (int)Config.Properties.Get("LastDiscordTimeMinutes"), (int)Config.Properties.Get("LastDiscordTimeSeconds"));
                        richPresence.Timestamps.Start = DateTime.UtcNow - elapsedTime;

                        DateTime lastTime = DateTime.UtcNow - elapsedTime;
                        Config.Properties.Set("LastDiscordTimeHours", lastTime.Hour);
                        Config.Properties.Set("LastDiscordTimeMinutes", lastTime.Minute);
                        Config.Properties.Set("LastDiscordTimeSeconds", lastTime.Second);

                        showElapsedTime = true;
                    }
                    else if (!showTime)
                    {
                        richPresence.Timestamps.Start = null;
                        showElapsedTime = false;
                    }
                    if (!client.IsDisposed) { client.SetPresence(richPresence); }
                }
            }
        }
    }
}
