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
using System.Collections.Generic;
using System.IO;

namespace MCNebula.Modules.Awards
{
    /// <summary> Manages which players have which awards. </summary>
    public static class PlayerAwards 
    {  
        struct PlayerAward { public string Player; public List<string> Awards; }

        /// <summary> List of all players who have awards </summary>
        static List<PlayerAward> Awards = new List<PlayerAward>();
        
        
        /// <summary> Adds the given award to the given player's list of awards </summary>
        public static bool Give(string player, string award) {
            List<string> awards = Get(player);
            if (awards == null) {
                awards = new List<string>();                
                PlayerAward a; a.Player = player; a.Awards = awards;
                Awards.Add(a);
            }
            
            if (awards.CaselessContains(award)) return false;
            awards.Add(award);
            return true;
        }
        
        /// <summary> Removes the given award from the given player's list of awards </summary>
        public static bool Take(string player, string award) {
            List<string> awards = Get(player);
            return awards != null && awards.CaselessRemove(award);
        }
        
        public static List<string> Get(string player) {
            foreach (PlayerAward a in Awards) {
                if (a.Player.CaselessEq(player)) return a.Awards;
            }
            return null;
        }
        
        
        /// <summary> Returns a summarised form of the player's awards </summary>
        /// <returns> [number of awards player has] / [number of awards] (% of total) </returns>
        public static string Summarise(string player) {
            int total = AwardsList.Awards.Count;
            List<string> awards = Get(player);
            if (awards == null || total == 0) return "0/" + total + " (0%)";
            
            // Some awards the player has may have been deleted
            int count = 0;
            for (int i = 0; i < awards.Count; i++) {
                if (AwardsList.Exists(awards[i])) count++;
            }
            
            double percentHas = Math.Round(((double)count / total) * 100, 2);
            return count + "/" + total + " (" + percentHas + "%)";
        }

        
        static readonly object saveLock = new object();
        public static void Save() {
            lock (saveLock)
                using (StreamWriter w = new StreamWriter("text/playerAwards.txt"))
            {
                foreach (PlayerAward a in Awards)
                    w.WriteLine(a.Player + " : " + a.Awards.Join(","));
            }
        }
        
        public static void Load() {
            Awards = new List<PlayerAward>();
            PropertiesFile.Read("text/playerAwards.txt", ProcessLine, ':');
        }
        
        static void ProcessLine(string key, string value) {
            if (value.Length == 0) return;
            PlayerAward a;
            a.Player = key;
            a.Awards = new List<string>();
            
            if (value.IndexOf(',') != -1) {
                foreach (string award in value.Split(',')) {
                    a.Awards.Add(award);
                }
            } else {
                a.Awards.Add(value);
            }
            Awards.Add(a);
        }
    }
}
