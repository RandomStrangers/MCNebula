﻿/*
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
//StormCom Object Generator
//
//Full use to all StormCom Server System codes (in regards to minecraft classic) have been granted to MCForge without restriction.
//
// ~Merlin33069
using System;
using MCNebula.Drawing.Brushes;
using MCNebula.Maths;
using BlockID = System.UInt16;

namespace MCNebula.Drawing.Ops 
{
    public class AdvHollowConeDrawOp : AdvDrawOp 
    {
        public override string Name { get { return "Adv Hollow Cone"; } }
        public AdvHollowConeDrawOp(bool invert = false) { Invert = invert; }
        
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) {
            long R = Radius, H = Height;
            double outer = (int)(Math.PI / 3 * (R * R * H));
            double inner = (int)(Math.PI / 3 * ((R - 1) * (R - 1) * (H - 1)));
            return (long)(outer - inner);
        }
        
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output) {
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            Vec3S32 C  = (Min + Max) / 2;
            int height = Height;

            for (ushort y = p1.Y; y <= p2.Y; y++)
            {
                int dy         = Invert ? y - Min.Y : Max.Y - y;
                int curRadius  = Radius * (dy + 1) / height;
                int curRadius2 = Radius * (dy    ) / height;
                
                for (ushort z = p1.Z; z <= p2.Z; z++)
                    for (ushort x = p1.X; x <= p2.X; x++)
                {
                    int dx   = C.X - x, dz = C.Z - z;
                    int dist = dx * dx + dz * dz;
                    if (dist > curRadius * curRadius) continue;
                    
                    if (dist <= (curRadius - 1) * (curRadius - 1) &&
                        dist <= (curRadius2)    * (curRadius2)  ) continue;
                    output(Place(x, y, z, brush));
                }
            }
        }
    }
    
    public class AdvVolcanoDrawOp : AdvDrawOp 
    {
        public override string Name { get { return "Adv Volcano"; } }
        
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) {
            long R = Radius, H = Height;
            return (long)(Math.PI / 3 * (R * R * H));
        }
        
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output) {
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            Vec3S32 C  = (Min + Max) / 2;
            int height = Height;

            for (ushort y = p1.Y; y <= p2.Y; y++)
            {
                int dy         = Max.Y - y;
                int curRadius  = Radius * (dy + 1) / height;
                int curRadius2 = Radius * (dy    ) / height;
                
                for (ushort z = p1.Z; z <= p2.Z; z++)
                    for (ushort x = p1.X; x <= p2.X; x++)
                {
                    int dx   = C.X - x, dz = C.Z - z;
                    int dist = dx * dx + dz * dz;
                    if (dist > curRadius * curRadius) continue;
                    
                    bool layer = curRadius == 0 ||
                        !(dist <= (curRadius - 1) * (curRadius - 1) &&
                          dist <= (curRadius2   ) * (curRadius2   ) );

                    BlockID block = layer ? Block.Grass : Block.StillLava;
                    output(Place(x, y, z, block));
                }
            }
        }
    }
}
