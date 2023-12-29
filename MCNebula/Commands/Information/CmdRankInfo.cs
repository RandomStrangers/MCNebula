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

namespace MCNebula.Commands.Info 
{
    public sealed class CmdRankInfo : Command2 
    {
        public override string name { get { return "RankInfo"; } }
        public override string shortcut { get { return "ri"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool UseableWhenFrozen { get { return true; } }
        public override bool MessageBlockRestricted { get { return false; } }
        
        public override void Use(Player p, string name, CommandData data) {
            if (CheckSuper(p, name, "player name")) return;
            if (name.Length == 0) name = p.name;
            
            name = PlayerInfo.FindMatchesPreferOnline(p, name);
            if (name == null) return;
            
            List<string> rankings = Server.RankInfo.FindAllExact(name);
            string nick = p.FormatNick(name);
            
            if (rankings.Count == 0) {
                p.Message("{0} &Shas no rankings.", nick); return;
            } else {
                p.Message("  Rankings for {0}:", nick);
            }
            
            foreach (string line in rankings) {
                string[] args = line.SplitSpaces();
                TimeSpan delta;
                string oldRank, newRank;
                int offset;
                
                if (args.Length <= 6) {
                    delta   = DateTime.UtcNow - long.Parse(args[2]).FromUnixTime();
                    newRank = args[3]; oldRank = args[4]; 
                    offset  = 5;
                } else {
                    // Backwards compatibility with old format
                    int min   = NumberUtils.ParseInt32(args[2]);
                    int hour  = NumberUtils.ParseInt32(args[3]);
                    int day   = NumberUtils.ParseInt32(args[4]);
                    int month = NumberUtils.ParseInt32(args[5]);
                    int year  = NumberUtils.ParseInt32(args[6]);
                    
                    delta   = DateTime.Now - new DateTime(year, month, day, hour, min, 0);
                    newRank = args[7]; oldRank = args[8]; 
                    offset  = 9;
                }
                string reason = args.Length <= offset ? "(no reason given)" : args[offset].Replace("%20", " ");
               
                p.Message("&aFrom {0} &ato {1} &a{2} ago", 
                               Group.GetColoredName(oldRank), Group.GetColoredName(newRank),
                               delta.Shorten(true, false));
                p.Message("&aBy &S{0}&a, reason: &S{1}", p.FormatNick(args[1]), reason);
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/RankInfo [player]");
            p.Message("&HReturns details about that person's rankings.");
        }
    }
}
