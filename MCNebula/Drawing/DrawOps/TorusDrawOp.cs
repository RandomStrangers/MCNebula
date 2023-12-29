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
using MCNebula.Drawing.Brushes;
using MCNebula.Maths;

namespace MCNebula.Drawing.Ops 
{
    public class TorusDrawOp : ShapedDrawOp 
    {
        public override string Name { get { return "Torus"; } }
        
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) {
            double rx = XRadius, ry = YRadius, rz = ZRadius;
            double rTube = ry, rCentre = Math.Min(rx, rz) - rTube;
            return (int)(2 * Math.PI * Math.PI * rTube * rTube * Math.Abs(rCentre));
        }
        
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output) {          
            double cx = XCentre, cy = YCentre, cz = ZCentre;
            double rx = XRadius, ry = YRadius, rz = ZRadius;
            double rTube = ry, rCentre = Math.Min(rx, rz) - rTube;
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            
            for (ushort yy = p1.Y; yy <= p2.Y; yy++)
                for (ushort zz = p1.Z; zz <= p2.Z; zz++)
                    for (ushort xx = p1.X; xx <= p2.X; xx++)
            {
                double dx = xx - cx, dy = yy - cy, dz = zz - cz;
                dx *= dx; dy *= dy; dz *= dz;
                double dInner = rCentre - Math.Sqrt( dx + dz );
                
                if (dInner * dInner + dy <= rTube * rTube * 0.5 + 0.25)
                    output(Place(xx, yy, zz, brush));
            }
        }
    }
}
