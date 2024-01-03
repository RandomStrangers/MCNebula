/*
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
using MCNebula.Events;
using System;

namespace MCNebula.Commands.Moderation {
    public sealed class CmdXFreeze : Command2 {
        public override string name { get { return "XFreeze"; } }
        public override string shortcut { get { return "XMute"; } }

        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string message, CommandData data) {
            if (message.Length == 0) { Help(p); return; }
            string[] args = message.SplitSpaces(2);
            if (args.Length <= 1 ) {  Help(p); return; }
            string target = PlayerInfo.FindMatchesPreferOnline(p, args[0]);
            if (target == null) return;
            Command.Find("freeze").Use(p, message, data);
            Command.Find("mute").Use(p, message, data);

        }
        public override void Help(Player p) {
            p.Message("&T/XFreeze [name] [timespan] <reason>");
            p.Message("&HMutes, and freezes a player.");
            p.Message("&HFor <reason>, @number can be used as a shortcut for that rule.");
        }
    }
}
