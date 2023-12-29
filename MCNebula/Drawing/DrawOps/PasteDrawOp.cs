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
using System;
using MCNebula.DB;
using MCNebula.Drawing.Brushes;
using MCNebula.Maths;

namespace MCNebula.Drawing.Ops 
{
    public class PasteDrawOp : DrawOp 
    {
        public override string Name { get { return "Paste"; } }        
        public CopyState CopyState;
        
        public PasteDrawOp() {
            Flags = BlockDBFlags.Pasted;
        }
        
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) {
            return CopyState.UsedBlocks;
        }
        
        public override void SetMarks(Vec3S32[] m) {
            Origin = m[0];
            CopyState cState = CopyState;
            if (cState.X != cState.OriginX) m[0].X -= (cState.Width - 1);
            if (cState.Y != cState.OriginY) m[0].Y -= (cState.Height - 1);
            if (cState.Z != cState.OriginZ) m[0].Z -= (cState.Length - 1);
            
            Min = m[0]; Max = m[0];
            Max.X += cState.Width - 1; Max.Y += cState.Height - 1; Max.Z += cState.Length - 1;
        }
        
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output) {
            CopyState state = CopyState;
            bool pasteAir = state.PasteAir;            
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            
            for (ushort y = p1.Y; y <= p2.Y; y++)
                for (ushort z = p1.Z; z <= p2.Z; z++)
                    for (ushort x = p1.X; x <= p2.X; x++)
            {
                DrawOpBlock block = Place(x, y, z, brush);
                if (pasteAir || block.Block != Block.Air) output(block);
            }
        }
    }
}
