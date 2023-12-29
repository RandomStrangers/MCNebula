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

namespace MCNebula.Drawing.Ops 
{
    public abstract class AdvDrawOp : DrawOp 
    {
        public int Radius { get { return (Max.X - Min.X) / 2; } }
        public int Height { get { return (Max.Y - Min.Y) + 1; } }
        
        public bool Invert;
    }
    
    public class AdvSphereDrawOp : AdvDrawOp 
    {
        public override string Name { get { return "Adv Sphere"; } }
        
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) {
            double R = Radius;
            return (long)ShapedDrawOp.EllipsoidVolume(R, R, R);
        }
        
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output) {
            int upper  = (Radius + 1) * (Radius + 1);
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            Vec3S32 C  = (Min + Max) / 2;

            for (ushort y = p1.Y; y <= p2.Y; y++)
                for (ushort z = p1.Z; z <= p2.Z; z++)
                    for (ushort x = p1.X; x <= p2.X; x++)
            {
                int dist = (C.X - x) * (C.X - x) + (C.Y - y) * (C.Y - y) + (C.Z - z) * (C.Z - z);
                if (dist < upper)
                    output(Place(x, y, z, brush));
            }
        }
    }
    
    public class AdvHollowSphereDrawOp : AdvDrawOp 
    {
        public override string Name { get { return "Adv Hollow Sphere"; } }
        
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) {
            double R = Radius;
            double outer = ShapedDrawOp.EllipsoidVolume(R,     R,     R    );
            double inner = ShapedDrawOp.EllipsoidVolume(R - 1, R - 1, R - 1);
            return (long)(outer - inner);
        }
        
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output) {
            int upper  = (Radius + 1) * (Radius + 1);
            int inner  = (Radius - 1) * (Radius - 1);
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            Vec3S32 C  = (Min + Max) / 2;

            for (ushort y = p1.Y; y <= p2.Y; y++)
                for (ushort z = p1.Z; z <= p2.Z; z++)
                    for (ushort x = p1.X; x <= p2.X; x++)
            {
                int dist = (C.X - x) * (C.X - x) + (C.Y - y) * (C.Y - y) + (C.Z - z) * (C.Z - z);
                if (dist < upper && dist >= inner)
                    output(Place(x, y, z, brush));
            }
        }
    }
}