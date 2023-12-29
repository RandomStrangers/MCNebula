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
using MCNebula.Eco;

namespace MCNebula.Modules.Awards
{
    public sealed class CmdAwardMod : Command2 
    {
        public override string name { get { return "AwardMod"; } }
        public override string type { get { return CommandTypes.Economy; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        static char[] awardArgs = new char[] { ':' };

        public override void Use(Player p, string message, CommandData data) {
            string[] args = message.SplitSpaces(2);
            if (args.Length < 2) { Help(p); return; }

            if (IsCreateAction(args[0])) {
                args = args[1].Split(awardArgs, 2);
                if (args.Length == 1) { 
                    p.Message("&WUse a : to separate the award name from its description."); 
                    Help(p); return;
                }
                
                string award = args[0].Trim();
                string desc  = args[1].Trim();

                if (!AwardsList.Add(award, desc)) {
                    p.Message("This award already exists."); return;
                } else {
                    Chat.MessageGlobal("Award added: &6{0} : {1}", award, desc);
                    AwardsList.Save();
                }
            } else if (IsDeleteAction(args[0])) {
                if (!AwardsList.Remove(args[1])) {
                    p.Message("This award does not exist."); return;
                } else {
                    Chat.MessageGlobal("Award removed: &6{0}", args[1]);
                    AwardsList.Save();
                }
            } else {
                Help(p);
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/AwardMod add [name] : [description]");
            p.Message("&HAdds a new award");
            p.Message("&H  e.g. &T/AwardMod add Bomb voyage : Blow up a lot of TNT");
            p.Message("&T/AwardMod del [name]");
            p.Message("&HDeletes the given award");
        }
    }
}
