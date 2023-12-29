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
using System.IO;
using MCNebula.Drawing;
using MCNebula.Drawing.Ops;
using MCNebula.DB;
using MCNebula.Levels.IO;
using MCNebula.Maths;
using BlockID = System.UInt16;

namespace MCNebula.Commands.Moderation {    
    public sealed class CmdRestoreSelection : Command2 {        
        public override string name { get { return "RS"; } }
        public override string shortcut { get { return "RestoreSelection"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string message, CommandData data) {
            if (message.Length == 0) { Help(p); return; }
            if (!Formatter.ValidMapName(p, message)) return;
            
            string path = LevelInfo.BackupFilePath(p.level.name, message);
            if (File.Exists(path)) {
                p.Message("Select two corners for restore.");
                p.MakeSelection(2, "Selecting region for &SRestore", path, DoRestore);
            } else {
                p.Message("Backup {0} does not exist.", message);
                LevelOperations.OutputBackups(p, p.level);
            }
        }
        
        bool DoRestore(Player p, Vec3S32[] marks, object state, BlockID block) {
            string path  = (string)state;
            Level source = IMapImporter.Decode(path, "templevel", false);
            
            RestoreSelectionDrawOp op = new RestoreSelectionDrawOp();
            op.Source = source;
            if (DrawOpPerformer.Do(op, null, p, marks)) return false;
            
            // Not high enough draw limit
            source.Dispose();
            return false;
        }

        public override void Help(Player p) {
            p.Message("&T/RestoreSelection [backup name]");
            p.Message("&HRestores a previous backup of the current selection");
        }
    }
}
