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

namespace MCNebula.Commands.Chatting 
{
    public sealed class CmdLoginMessage : EntityPropertyCmd
    {
        public override string name { get { return "LoginMessage"; } }
        public override string shortcut { get { return "LoginMsg"; } }
        public override string type { get { return CommandTypes.Chat; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Operator, "can change the login message of others") }; }
        }
        
        public override void Use(Player p, string message, CommandData data) {
            UsePlayer(p, data, message, "login message");
        }
        
        protected override void SetPlayerData(Player p, string target, string msg) {
            PlayerOperations.SetLoginMessage(p, target, msg);
        }
        
        public override void Help(Player p) {
            p.Message("&T/LoginMessage [player] [message]");
            p.Message("&HSets the login message shown for that player.");
        }
    }
}
