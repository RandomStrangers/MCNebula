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

namespace MCNebula.Blocks.Physics {
    
    public static class FireworkPhysics {
        
        public static void Do(Level lvl, ref PhysInfo C) {
            Random rand = lvl.physRandom;            
            ushort x = C.X, y = C.Y, z = C.Z;
            
            if (lvl.GetBlock(x, (ushort)(y - 1), z) != Block.StillLava)
                return;
            
            if (lvl.IsAirAt(x, (ushort)(y + 1), z)) {
                bool keepGoing = true;
                if ((lvl.Height * 80 / 100) < y)
                    keepGoing = rand.Next(1, 20) > 1;

                if (keepGoing) {
                    int bAbove = lvl.PosToInt(x, (ushort)(y + 1), z);
                    bool unblocked = bAbove < 0 || !lvl.listUpdateExists.Get(x, y + 1, z);
                    if (unblocked) {
                        PhysicsArgs args = default(PhysicsArgs);
                        args.Type1 = PhysicsArgs.Wait; args.Value1 = 1;
                        args.Type2 = PhysicsArgs.Dissipate; args.Value2 = 100;
                        
                        lvl.AddUpdate(bAbove, Block.Fireworks, default(PhysicsArgs));
                        lvl.AddUpdate(C.Index, Block.StillLava, args);
                        args.Data = C.Data.Data;
                        C.Data = args;
                        return;
                    }
                }
            }
            Firework(ref C, 4, lvl, rand);
        }
        
        static void Firework(ref PhysInfo C, int size, Level lvl, Random rand) {
            int rand1 = rand.Next(Block.Red, Block.White);
            int rand2 = rand.Next(Block.Red, Block.White);
            int min = Math.Min(rand1, rand2), max = Math.Max(rand1, rand2);
            // Not using override, since override = true makes it more likely that a colored block will be
            // generated with no extraInfo, because it sets a Check for that position with no extraInfo.
            lvl.AddUpdate(C.Index, Block.Air, default(PhysicsArgs));
            
            int index;
            ushort x = C.X, y = C.Y, z = C.Z;
            for (int yy = y - (size + 1); yy <= y + (size + 1); ++yy)
                for (int zz = z - (size + 1); zz <= z + (size + 1); ++zz)
                    for (int xx = x - (size + 1); xx <= x + (size + 1); ++xx)
            {               
                if (lvl.IsAirAt((ushort)xx, (ushort)yy, (ushort)zz, out index) && rand.Next(1, 40) < 2) {
                    PhysicsArgs args = default(PhysicsArgs);
                    args.Type1 = PhysicsArgs.Drop; args.Value1 = 100;
                    args.Type2 = PhysicsArgs.Dissipate; args.Value2 = 25;
                    lvl.AddUpdate(index, (byte)rand.Next(min, max), args);
                }
            }
        }
    }
}
