/*
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
using System;
using MCNebula.Drawing.Brushes;
using MCNebula.Drawing.Ops;

namespace MCNebula.Commands.Building {    
    public sealed class CmdSpheroid : DrawCmd {
        public override string name { get { return "Spheroid"; } }
        public override string shortcut { get { return "e"; } }
        public override CommandAlias[] Aliases {
            get { return new[] { new CommandAlias("eh", "hollow"), new CommandAlias("Cone", "cone"), new CommandAlias("Cylinder", "cylinder") }; }
        }
        
        protected override void GetBrush(DrawArgs dArgs) {
            if (dArgs.Mode == DrawMode.solid) dArgs.BrushName = "Normal";
            dArgs.BrushArgs = dArgs.Message.Splice(dArgs.ModeArgsCount, 0);
        }
        
        protected override DrawMode GetMode(string[] parts) {
            string msg = parts[0];
            if (msg == "solid")    return DrawMode.solid;
            if (msg == "hollow")   return DrawMode.hollow;
            if (msg == "vertical") return DrawMode.vertical;
            if (msg == "cylinder") return DrawMode.vertical;
            if (msg == "cone")     return DrawMode.cone;
            return DrawMode.normal;
        }
        
        protected override DrawOp GetDrawOp(DrawArgs dArgs) {
            switch (dArgs.Mode) {
                case DrawMode.hollow:   return new EllipsoidHollowDrawOp();
                case DrawMode.vertical: return new CylinderDrawOp();
                case DrawMode.cone:     return new ConeDrawOp();
            }
            return new EllipsoidDrawOp();
        }
        
        public override void Help(Player p) {
            p.Message("&T/Spheroid <brush args>");
            p.Message("&HDraws a spheroid between two points.");
            p.Message("&T/Spheroid [mode] <brush args>");
            p.Message("&HModes: &fsolid/hollow/cylinder/cone");    
            p.Message(BrushHelpLine);
        }
    }
}