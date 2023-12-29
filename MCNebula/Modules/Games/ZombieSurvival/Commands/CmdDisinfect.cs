﻿/*
    Copyright 2011 MCForge
        
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
using MCNebula.Games;

namespace MCNebula.Modules.Games.ZS 
{
    sealed class CmdDisInfect : Command2 
    {
        public override string name { get { return "DisInfect"; } }
        public override string shortcut { get { return "di"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        
        public override void Use(Player p, string message, CommandData data) {
            Player who = message.Length == 0 ? p : PlayerInfo.FindMatches(p, message);
            if (who == null) return;
            
            if (!ZSGame.Instance.RoundInProgress || !ZSGame.IsInfected(who)) {
                p.Message("Cannot disinfect player");
            } else if (!who.Game.Referee) {
                ZSGame.Instance.DisinfectPlayer(who);
                Chat.MessageFrom(who, "λNICK &Swas disinfected.");
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/DisInfect [name]");
            p.Message("&HTurns [name] back into a human");
        }
    }
}
