﻿/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using System.Collections.Generic;

namespace MCNebula.Commands.Moderation 
{
    public abstract class ItemPermsCmd : Command2 
    {
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        
        protected string SetPerms(Player p, string[] args, CommandData data, ItemPerms perms, string type, 
                                string actionNoun, string actionAdjective) {
            string grpName = args[1];          
            if (!perms.UsableBy(data.Rank)) {
                p.Message("You rank cannot {1} this {0}.", type, actionNoun); return null;
            }
            
            if (grpName[0] == '+') {
                Group grp = GetGroup(p, data, grpName.Substring(1));
                if (grp == null) return null;

                perms.Allow(grp.Permission);
                return " &Sis now " + actionAdjective + " by " + grp.ColoredName;
            } else if (grpName[0] == '-') {
                Group grp = GetGroup(p, data, grpName.Substring(1));
                if (grp == null) return null;

                if (data.Rank == grp.Permission) {
                    p.Message("&WCannot deny permissions for your own rank"); return null;
                }
                
                perms.Disallow(grp.Permission);
                return " &Sis no longer " + actionAdjective + " by " + grp.ColoredName;
            } else {
                Group grp = GetGroup(p, data, grpName);
                if (grp == null) return null;

                perms.MinRank = grp.Permission;
                return " &Sis now " + actionAdjective + " by " + grp.ColoredName + "&S+";
            }
        }
        
        protected static Group GetGroup(Player p, CommandData data, string grpName) {
            Group grp = Matcher.FindRanks(p, grpName);
            if (grp == null) return null;
            
            if (grp.Permission > data.Rank) {
                p.Message("&WCannot set permissions to a rank higher than yours."); return null;
            }
            return grp;
        }
        
        protected static void Announce(Player p, string msg) {
            Chat.MessageAll("&d" + msg);
            if (p.IsSuper) p.Message(msg);
        }
    }
}
