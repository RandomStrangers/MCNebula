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
using BlockID = System.UInt16;

namespace MCNebula.Blocks.Physics {
    
    public static class ZombiePhysics {
        
        public static void Do(Level lvl, ref PhysInfo C) {
            Random rand = lvl.physRandom;
            ushort x = C.X, y = C.Y, z = C.Z;
            
            // Make zombie fall down
            if (lvl.IsAirAt(x, (ushort)(y - 1), z)) {
                lvl.AddUpdate(C.Index, Block.ZombieHead);
                lvl.AddUpdate(lvl.IntOffset(C.Index, 0, -1, 0), C.Block);
                lvl.AddUpdate(lvl.IntOffset(C.Index, 0, 1, 0), Block.Air);
                return;
            }
            bool checkTime = true;
            Player closest = HunterPhysics.ClosestPlayer(lvl, x, y, z);

            if (closest != null && rand.Next(1, 20) < 18) {
                ushort xx, zz;
                if (rand.Next(1, 7) <= 3) {
                    xx = (ushort)(x + Math.Sign(closest.Pos.BlockX - x));
                    if (xx != x && MoveZombie(lvl, ref C, xx, y, z)) return;
                    
                    zz = (ushort)(z + Math.Sign(closest.Pos.BlockZ - z));
                    if (zz != z && MoveZombie(lvl, ref C, x, y, zz)) return;
                } else {
                    zz = (ushort)(z + Math.Sign(closest.Pos.BlockZ - z));
                    if (zz != z && MoveZombie(lvl, ref C, x, y, zz)) return;
                    
                    xx = (ushort)(x + Math.Sign(closest.Pos.BlockX - x));
                    if (xx != x && MoveZombie(lvl, ref C, xx, y, z)) return;
                }
                checkTime = false;
            }
            
            if (checkTime && C.Data.Data < 3) {
                C.Data.Data++;
                return;
            }

            int dirsVisited = 0;
            switch (rand.Next(1, 13))
            {
                case 1:
                case 2:
                case 3:
                    if (MoveZombie(lvl, ref C, (ushort)(x - 1), y, z)) return;

                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 4;

                case 4:
                case 5:
                case 6:
                    if (MoveZombie(lvl, ref C, (ushort)(x + 1), y, z)) return;

                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 7;

                case 7:
                case 8:
                case 9:
                    if (MoveZombie(lvl, ref C, x, y, (ushort)(z + 1))) return;

                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 10;
                case 10:
                case 11:
                case 12:
                    if (MoveZombie(lvl, ref C, x, y, (ushort)(z - 1))) return;

                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 1;
            }
            lvl.AddUpdate(C.Index, Block.Air, default(PhysicsArgs));
            lvl.AddUpdate(lvl.IntOffset(C.Index, 0, 1, 0), Block.Air, default(PhysicsArgs));
        }
        
        public static void DoHead(Level lvl, ref PhysInfo C) {
            BlockID below = lvl.GetBlock(C.X, (ushort)(C.Y - 1), C.Z);
            
            if (below != Block.ZombieBody && below != Block.Creeper) {
                C.Data.Type1 = PhysicsArgs.Revert; C.Data.Value1 = Block.Air;
            }
        }
        
        static bool MoveZombie(Level lvl, ref PhysInfo C, ushort x, ushort y, ushort z) {
            int index;
            
            // Move zombie up or down blocks
            if (       lvl.IsAirAt(x, (ushort)(y - 1), z, out index) && lvl.IsAirAt(x, y,               z)) {
            } else if (lvl.IsAirAt(x, y,               z, out index) && lvl.IsAirAt(x, (ushort)(y + 1), z)) {
            } else if (lvl.IsAirAt(x, (ushort)(y + 1), z, out index) && lvl.IsAirAt(x, (ushort)(y + 2), z)) {
            } else {
                return false;
            }

            if (lvl.AddUpdate(index, C.Block)) {
                lvl.AddUpdate(lvl.IntOffset(index, 0, 1, 0), Block.ZombieHead, default(PhysicsArgs));
                lvl.AddUpdate(C.Index, Block.Air, default(PhysicsArgs));
                lvl.AddUpdate(lvl.IntOffset(C.Index, 0, 1, 0), Block.Air, default(PhysicsArgs));
                return true;
            }
            return false;
        }
    }
}