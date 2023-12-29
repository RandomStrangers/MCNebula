﻿/*
    Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)
 
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
using System.Collections.Generic;

namespace MCNebula.Commands.Moderation {
    public sealed class CmdWhitelist : Command2 {
        public override string name { get { return "Whitelist"; } }
        public override string shortcut { get { return "w"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Admin, "can enable/disable whitelisted only mode") }; }
        }

        public override void Use(Player p, string message, CommandData data) {
            string[] args = message.SplitSpaces();
            string cmd    = args[0];
            
            if (cmd.CaselessEq("enable")) {
                SetMode(true,  "&aON"); return;
            } else if (cmd.CaselessEq("disable")) {
                SetMode(false, "&cOFF"); return;
            }

            if (!Server.Config.WhitelistedOnly) { p.Message("Whitelist is not enabled."); return; }
            if (message.Length == 0) { List(p, ""); return; }         
            
            if (cmd.CaselessEq("add")) {
                if (args.Length < 2) { Help(p); return; }
                Add(p, args[1]);
            } else if (IsDeleteAction(cmd)) {
                if (args.Length < 2) { Help(p); return; }
                Remove(p, args[1]);
            } else if (IsListAction(cmd)) {
                string modifier = args.Length > 1 ? args[1] : "";
                List(p, modifier);
            } else if (args.Length == 1) {
                Add(p, cmd);
            } else {
                Help(p);
            }
        }

        static void SetMode(bool enabled, string desc) {
            Server.Config.WhitelistedOnly = enabled;
            SrvProperties.Save();

            Chat.MessageAll("Whitelisted only mode " + desc);
            Logger.Log(LogType.SystemActivity, "Whitelisted only mode is now " + desc);
        }
        
        static void Add(Player p, string name) {
            name = Server.FromRawUsername(name);
           
            if (!Server.whiteList.Add(name)) {
                p.Message("{0} &Sis already on the whitelist!", p.FormatNick(name));
            } else {
                Chat.MessageFromOps(p, "λNICK &Sadded &f" + name + " &Sto the whitelist.");
                Server.whiteList.Save();
                Logger.Log(LogType.UserActivity, "WHITELIST: Added " + name);
            }
        }
        
        static void Remove(Player p, string name) {
            name = Server.FromRawUsername(name);
            
            if (!Server.whiteList.Remove(name)) {
                p.Message("{0} &Sis not on the whitelist!", p.FormatNick(name));
            } else {
                Server.whiteList.Save();
                Chat.MessageFromOps(p, "λNICK &Sremoved &f" + name + " &Sfrom the whitelist.");
                Logger.Log(LogType.UserActivity, "WHITELIST: Removed " + name);
            }
        }
        
        static void List(Player p, string modifier) {
            Server.whiteList.Output(p, "whitelisted players", "Whitelist list", modifier);
        }

        public override void Help(Player p) {
            p.Message("&T/Whitelist add/del [player]");
            p.Message("&HAdds or removes [player] from the whitelist");
            p.Message("&T/Whitelist list");
            p.Message("&HLists all players who are on the whitelist");
            p.Message("&T/Whitelist enable/disable");
            p.Message("&HSets whether only whitelisted players can join the server");
        }
    }
}
