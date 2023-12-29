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
using MCNebula.Events.PlayerEvents;
using MCNebula.Games;

namespace MCNebula.Commands.World {
    public sealed class CmdSpawn : Command2 {
        public override string name { get { return "Spawn"; } }
        public override string type { get { return CommandTypes.World; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) {
            if (!Hacks.CanUseRespawn(p)) {
                p.Message("You cannot use &T/Spawn &Son this map.");
                p.isFlying = false; return;
            }
            if (!IGame.CheckAllowed(p, "use &T/Spawn")) return;
            
            if (message.Length > 0) { Help(p); return; }
            PlayerActions.RespawnAt(p, p.level.SpawnPos, p.level.rotx, p.level.roty);
        }
        
        public override void Help(Player p) {
            p.Message("&T/Spawn");
            p.Message("&HTeleports you to the spawn location of the level.");
        }
    }
}
