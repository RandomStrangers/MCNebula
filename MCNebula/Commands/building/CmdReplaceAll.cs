﻿/*
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
using MCNebula.Drawing.Brushes;
using MCNebula.Drawing.Ops;
using MCNebula.Maths;

namespace MCNebula.Commands.Building {
    
    public sealed class CmdReplaceAll : Command2 {
        public override string name { get { return "ReplaceAll"; } }
        public override string shortcut { get { return "ra"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override bool museumUsable { get { return false; } }
        public override bool SuperUseable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        
        public override void Use(Player p, string message, CommandData data) {
            BrushArgs args = new BrushArgs(p, message, Block.Air);
            Brush brush = BrushFactory.Find("Replace").Construct(args);
            if (brush == null) return;
            
            Vec3S32 max = new Vec3S32(p.level.MaxX, p.level.MaxY, p.level.MaxZ);
            Vec3S32[] marks = new Vec3S32[] { Vec3S32.Zero, max };
            
            MeasureDrawOp measure = new MeasureDrawOp();
            measure.Setup(p, p.level, marks);
            measure.Perform(marks, brush, null);
            
            if (measure.Total > p.group.DrawLimit) {
                p.Message("You tried to replace " + measure.Total + " blocks.");
                p.Message("You cannot draw more than " + p.group.DrawLimit + ".");
                return;
            }
            
            DrawOp op = new CuboidDrawOp();
            op.AffectedByTransform = false;
            if (!DrawOpPerformer.Do(op, brush, p, marks, false)) return;
            p.Message("&4/replaceall finished!");
        }
        
        
        class MeasureDrawOp : DrawOp {
            public override string Name { get { return null; } }
            public override long BlocksAffected(Level lvl, Vec3S32[] marks) { return 0; }
            public int Total = 0;
            
            public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output) {
                Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
                for (ushort y = p1.Y; y <= p2.Y; y++)
                    for (ushort z = p1.Z; z <= p2.Z; z++)
                        for (ushort x = p1.X; x <= p2.X; x++)
                {
                    Coords.X = x; Coords.Y = y; Coords.Z = z;
                    if (brush.NextBlock(this) != Block.Invalid) Total++;
                }
            }
        }

        public override void Help(Player p) {
            p.Message("&T/ReplaceAll [block] [block2].. [new]");
            p.Message("&HReplaces [block] with [new] for the entire map.");
            p.Message("&H  If more than one [block] is given, they are all replaced.");
            p.Message("&H  If only [block] is given, replaces with your held block.");
        }
    }
}
