/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Net;
using MCNebula.Network;
using MCNebula.Tasks;

namespace MCNebula
{
    /// <summary> Checks for and applies software updates. </summary>
    public static class Updater 
    {    
        public static string SourceURL = "https://github.com/RandomStrangers/MCNebula";
        public const string BaseURL = "https://github.com/RandomStrangers/MCNebula/raw/master/Uploads/";
        public const string UploadsURL = "https://github.com/RandomStrangers/MCNebula/tree/master/Uploads";

#if DEV
        const string dllURL = BaseURL + "Uploads/MCNebula_dev.dll";
        const string CurrentVersionURL = BaseURL + "Uploads/dev.txt";
#else
        const string dllURL = BaseURL + "Uploads/MCNebula_.dll";
        const string CurrentVersionURL = BaseURL + "Uploads/current_version.txt";
#endif
        const string guiURL = BaseURL + "Uploads/MCNebula.exe";
        const string cliURL = BaseURL + "Uploads/MCNebulaCLI.exe";

        public static event EventHandler NewerVersionDetected;
        
        public static void UpdaterTask(SchedulerTask task) {
            UpdateCheck();
            task.Delay = TimeSpan.FromHours(2);
        }

        static void UpdateCheck() {
            if (!Server.Config.CheckForUpdates) return;

            try {
                if (!NeedsUpdating()) {
                    Logger.Log(LogType.SystemActivity, "No update found!");
                } else if (NewerVersionDetected != null) {
                    NewerVersionDetected(null, EventArgs.Empty);
                }
            } catch (Exception ex) {
                Logger.LogError("Error checking for updates", ex);
            }
        }
        
        public static bool NeedsUpdating() {
            using (WebClient client = HttpUtil.CreateWebClient()) {
                string latest = client.DownloadString(CurrentVersionURL);
                return new Version(latest) > new Version(Server.VersionAlpha);
            }
        }

        public static void PerformUpdate() {
            try {
                try {
                    DeleteFiles("MCNebula_.update", "MCNebula.update", "MCNebulaCLI.update",
                                "prev_MCNebula_.dll", "prev_MCNebula.exe", "prev_MCNebulaCLI.exe");
                } catch {
                }
                
                WebClient client = HttpUtil.CreateWebClient();
                client.DownloadFile(dllURL, "MCNebula_.update");
                client.DownloadFile(guiURL, "MCNebula.update");
                client.DownloadFile(cliURL, "MCNebulaCLI.update");

                Server.SaveAllLevels();
                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players) pl.SaveStats();
                
                string serverDLL = Server.GetServerDLLPath();
                
                // Move current files to previous files (by moving instead of copying, 
                //  can overwrite original the files without breaking the server)
                AtomicIO.TryMove(serverDLL, "prev_MCNebula_.dll");
                AtomicIO.TryMove("MCNebula.exe", "prev_MCNebula.exe");
                AtomicIO.TryMove("MCNebulaCLI.exe", "prev_MCNebulaCLI.exe");

                // Move update files to current files
                AtomicIO.TryMove("MCNebula_.update",   serverDLL);
                AtomicIO.TryMove("MCNebula.update", "MCNebula.exe");
                AtomicIO.TryMove("MCNebulaCLI.update", "MCNebulaCLI.exe");                             

                Server.Stop(true, "Updating server.");
            } catch (Exception ex) {
                Logger.LogError("Error performing update", ex);
            }
        }
        
        static void DeleteFiles(params string[] paths) {
            foreach (string path in paths) { AtomicIO.TryDelete(path); }
        }
    }
}