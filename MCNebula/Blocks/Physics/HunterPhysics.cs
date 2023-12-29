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
    
    public static class HunterPhysics {

        // dir is  1 for hunting animals (they go towards the closest player)
        // dir is -1 for fleeing animals (they go away from the closest player)
        public static void Do(Level lvl, ref PhysInfo C, BlockID target, int dir) {
            Random rand = lvl.physRandom;
            ushort x = C.X, y = C.Y, z = C.Z;
            Player closest = ClosestPlayer(lvl, x, y, z);
            
            if (closest != null && rand.Next(1, 20) < 19) {
                int dirsVisited = 0;

                switch (rand.Next(1, 10)) {
                    case 1:
                    case 2:
                    case 3:
                        ushort xx = (ushort)(x + Math.Sign(closest.Pos.BlockX - x) * dir);
                        if (xx != x && MoveTo(lvl, ref C, target, xx, y, z)) return;
                        
                        dirsVisited++;
                        if (dirsVisited >= 3) break;
                        goto case 4;
                    case 4:
                    case 5:
                    case 6:
                        ushort yy = (ushort)(y + Math.Sign(closest.Pos.BlockY - y) * dir);
                        if (yy != y && MoveTo(lvl, ref C, target, x, yy, z)) return;
                        
                        dirsVisited++;
                        if (dirsVisited >= 3) break;
                        goto case 7;
                    case 7:
                    case 8:
                    case 9:
                        ushort zz = (ushort)(z + Math.Sign(closest.Pos.BlockZ - z) * dir);
                        if (zz != z && MoveTo(lvl, ref C, target, x, y, zz)) return;
                        
                        dirsVisited++;
                        if (dirsVisited >= 3) break;
                        goto case 1;
                }
            }
            
            switch (rand.Next(1, 15)) {
                case 1:
                    if (MoveTo(lvl, ref C, target, x, (ushort)(y - 1), z)) return;
                    goto case 3;
                case 2:
                    if (MoveTo(lvl, ref C, target, x, (ushort)(y + 1), z)) return;
                    goto case 6;
                case 3:
                case 4:
                case 5:
                    if (MoveTo(lvl, ref C, target, (ushort)(x - 1), y, z)) return;
                    goto case 9;
                case 6:
                case 7:
                case 8:
                    if (MoveTo(lvl, ref C, target, (ushort)(x + 1), y, z)) return;
                    goto case 12;
                case 9:
                case 10:
                case 11:
                    MoveTo(lvl, ref C, target, x, y, (ushort)(z - 1));
                    break;
                case 12:
                case 13:
                case 14:
                    MoveTo(lvl, ref C, target, x, y, (ushort)(z + 1));
                    break;
            }
        }
        
        static bool MoveTo(Level lvl, ref PhysInfo C, BlockID target, ushort x, ushort y, ushort z) {
            int index;
            BlockID block = lvl.GetBlock(x, y, z, out index);            
            if (block == target && lvl.AddUpdate(index, C.Block)) {
                lvl.AddUpdate(C.Index, target);
                return true;
            }
            return false;
        }
        
        public static Player ClosestPlayer(Level lvl, ushort x, ushort y, ushort z) {
            if (!lvl.Config.AnimalHuntAI) return null;
            int closestDist = 75;
            Player closetPlayer = null;
            Player[] players = PlayerInfo.Online.Items;
            
            foreach (Player p in players) {
                if (p.level != lvl || p.invincible) continue;
                Position pos = p.Pos;
                int curDist = Math.Abs(pos.BlockX - x) + Math.Abs(pos.BlockY - y) + Math.Abs(pos.BlockZ - z);
                if (curDist < closestDist) {
                    closestDist = curDist;
                    closetPlayer = p;
                }
            }
            return closetPlayer;
        }
    }
}