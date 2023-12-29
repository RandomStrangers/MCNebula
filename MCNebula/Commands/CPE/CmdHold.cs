﻿/*
    Copyright 2015 MCGalaxy
        
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
using MCNebula.Network;
using BlockID = System.UInt16;

namespace MCNebula.Commands.CPE 
{    
    public sealed class CmdHold : Command2 
    {
        public override string name { get { return "Hold"; } }
        public override string shortcut { get { return "HoldThis"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) {
            if (message.Length == 0) { Help(p); return; }      
            string[] args = message.SplitSpaces(2);
            
            BlockID block;
            if (!CommandParser.GetBlock(p, args[0], out block)) return;
            bool locked = false;
            if (args.Length > 1 && !CommandParser.GetBool(p, args[1], ref locked)) return;
            
            if (Block.IsPhysicsType(block)) {
                p.Message("Cannot hold physics blocks"); return;
            }
            
            if (p.Session.SendHoldThis(block, locked)) {
                p.Message("Set your held block to {0}.", Block.GetName(p, block));
            } else {
                p.Message("Your client doesn't support changing your held block.");
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/Hold [block] <locked>");
            p.Message("&HMakes you hold the given block in your hand");
            p.Message("&H  <locked> optionally prevents you from changing it");
        }
    }
}