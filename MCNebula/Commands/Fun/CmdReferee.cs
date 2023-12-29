﻿/*
    Copyright 2011 MCForge
    
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
using MCNebula.Games;
using MCNebula.Events.PlayerEvents;
using MCNebula.Network;

namespace MCNebula.Commands.Fun {
    public sealed class CmdReferee : Command2 {
        public override string name { get { return "Referee"; } }
        public override string shortcut { get { return "Ref"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool SuperUseable { get { return false; } }
        
        public override void Use(Player p, string message, CommandData data) {
            if (p.Game.Referee) {
                Chat.MessageFrom(p, "λNICK &Sis no longer a referee", Chat.FilterVisible(p));
                OnPlayerActionEvent.Call(p, PlayerAction.UnReferee);
                p.Game.Referee = false;
            } else {
                Chat.MessageFrom(p, "λNICK &Sis now a referee", Chat.FilterVisible(p));
                OnPlayerActionEvent.Call(p, PlayerAction.Referee);
                p.Game.Referee = true;
            }
            p.SetPrefix();
            
            if (p.Supports(CpeExt.InstantMOTD)) {
                p.SendMapMotd();
            } else if (p.Supports(CpeExt.HackControl)) {
                string motd = p.GetMotd();
                if (p.Game.Referee) motd += " +hax";
                p.Send(Hacks.MakeHackControl(p, motd));
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/Referee");
            p.Message("&HTurns referee mode on/off.");
            p.Message("&HReferee mode enables you to use hacks and TP in games");
            p.Message("&H  Note that leaving referee mode sends you back to spawn");
        }
    }
}
