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
using MCNebula.Blocks.Physics;
using MCNebula.Drawing.Brushes;
using MCNebula.Drawing.Ops;
using MCNebula.Maths;
using MCNebula.Undo;
using BlockID = System.UInt16;
using BlockRaw = System.Byte;

namespace MCNebula.Drawing 
{
    internal struct PendingDrawOp 
    {
        public DrawOp Op;
        public Brush Brush;
        public Vec3S32[] Marks;
    }
}

namespace MCNebula.Drawing.Ops 
{
    public static class DrawOpPerformer 
    {     
        static bool CannotBuildIn(Player p, Level lvl) {
            Zone[] zones = lvl.Zones.Items;
            for (int i = 0; i < zones.Length; i++) {
                // player could potentially modify blocks in this particular zone
                if (zones[i].Access.CheckAllowed(p)) return false;
            }
            return !lvl.BuildAccess.CheckDetailed(p);
        }
        
        public static bool Do(DrawOp op, Brush brush, Player p,
                              Vec3S32[] marks, bool checkLimit = true) {
            Level lvl = p.level;
            op.Setup(p, lvl, marks);
            
            if (lvl != null && !lvl.Config.Drawing && !op.AlwaysUsable) {
                p.Message("Drawing commands are turned off on this map.");
                return false;
            }
            if (lvl != null && CannotBuildIn(p, lvl)) return false;
            
            long affected = op.BlocksAffected(lvl, marks);
            if (op.AffectedByTransform)
                p.Transform.GetBlocksAffected(ref affected);
            if (checkLimit && !op.CanDraw(marks, p, affected)) return false;
            
            if (brush != null && affected != -1) {
                const string format = "{0}({1}): affecting up to {2} blocks";
                if (!p.Ignores.DrawOutput) {
                    p.Message(format, op.Name, brush.Name, affected);
                }
            } else if (affected != -1) {
                const string format = "{0}: affecting up to {1} blocks";
                if (!p.Ignores.DrawOutput) {
                    p.Message(format, op.Name, affected);
                }
            }
            
            DoQueuedDrawOp(p, op, brush, marks);
            return true;
        }
        
        static void DoQueuedDrawOp(Player p, DrawOp op, Brush brush, Vec3S32[] marks) {
            PendingDrawOp item = new PendingDrawOp();
            item.Op = op; item.Brush = brush; item.Marks = marks;

            lock (p.pendingDrawOpsLock) {
                p.PendingDrawOps.Add(item);
                // Another thread is already processing draw ops.
                if (p.PendingDrawOps.Count > 1) return;
            }
            ProcessDrawOps(p);
        }
        
        static void ProcessDrawOps(Player p) {
            while (true) {
                PendingDrawOp item;
                lock (p.pendingDrawOpsLock) {
                    if (p.PendingDrawOps.Count == 0) return;
                    item = p.PendingDrawOps[0];
                    p.PendingDrawOps.RemoveAt(0);
                    
                    // Flush any remaining draw ops if the player has left the server.
                    // (so as to not keep alive references)
                    if (p.Socket != null && p.Socket.Disconnected) {
                        p.PendingDrawOps.Clear();
                        return;
                    }
                }
                Execute(p, item.Op, item.Brush, item.Marks);
            }
        }
        
        internal static void Execute(Player p, DrawOp op, Brush brush, Vec3S32[] marks) {
            UndoDrawOpEntry entry = new UndoDrawOpEntry();
            entry.Init(op.Name, op.Level.name);
            
            if (brush != null) brush.Configure(op, p);
            DrawOpOutputter outputter = new DrawOpOutputter(op);
            
            if (op.AffectedByTransform) {
                p.Transform.Perform(marks, op, brush, outputter.Output);
            } else {
                op.Perform(marks, brush, outputter.Output);
            }
            bool needsReload = op.TotalModified >= outputter.reloadThreshold;
            
            if (op.Undoable) entry.Finish(p);
            if (needsReload) DoReload(p, op.Level);
            op.TotalModified = 0; // reset total modified (as drawop instances are reused in static mode)
        }

        static void DoReload(Player p, Level lvl) {
            LevelActions.ReloadAll(lvl, p, true);
            Server.DoGC();
        }

        
        class DrawOpOutputter {
            readonly DrawOp op;
            internal readonly int reloadThreshold;
            
            public DrawOpOutputter(DrawOp op) {
                this.op = op;
                reloadThreshold = op.Level.ReloadThreshold;
            }
            
            public void Output(DrawOpBlock b) {
                if (b.Block == Block.Invalid) return;
                Level lvl = op.Level;
                Player p = op.Player;
                if (b.X >= lvl.Width || b.Y >= lvl.Height || b.Z >= lvl.Length) return;
                
                int index = b.X + lvl.Width * (b.Z + b.Y * lvl.Length);
                BlockID old = lvl.blocks[index];
                BlockID extended = Block.ExtendedBase[old];
                if (extended > 0) old = (BlockID)(extended | lvl.FastGetExtTile(b.X, b.Y, b.Z));
                
                // Check to make sure the block is actually different
                if (old == b.Block) return;
                
                // And check that the block can be used
                Group grp = p.group;
                if (!grp.CanDelete[old] || !grp.CanPlace[b.Block]) return;
                
                // Check if player can affect block at coords in world
                AccessController denier = lvl.CanAffect(p, b.X, b.Y, b.Z);
                if (denier != null) {
                    if (p.lastAccessStatus < DateTime.UtcNow) {
                        denier.CheckDetailed(p);
                        p.lastAccessStatus = DateTime.UtcNow.AddSeconds(2);
                    }
                    return;
                }
                
                // Set the block (inlined)
                lvl.Changed = true;
                if (b.Block >= Block.Extended) {
                    lvl.blocks[index] = Block.ExtendedClass[b.Block >> Block.ExtendedShift];
                    lvl.FastSetExtTile(b.X, b.Y, b.Z, (BlockRaw)b.Block);
                } else {
                    lvl.blocks[index] = (BlockRaw)b.Block;
                    if (old >= Block.Extended) {
                        lvl.FastRevertExtTile(b.X, b.Y, b.Z);
                    }
                }
                
                lvl.BlockDB.Cache.Add(p, b.X, b.Y, b.Z, op.Flags, old, b.Block);
                p.TotalModified++; p.TotalDrawn++; // increment block stats inline
                
                // Potentially buffer the block change
                if (op.TotalModified == reloadThreshold) {
                    if (!p.Ignores.DrawOutput) {
                        p.Message("Changed over {0} blocks, preparing to reload map..", reloadThreshold);
                    }
                    lvl.blockqueue.ClearAll();
                } else if (op.TotalModified < reloadThreshold) {
                    if (!Block.VisuallyEquals(old, b.Block)) {
                        lvl.blockqueue.Add(index, b.Block);
                    }

                    if (lvl.physics > 0) {
                        if (old == Block.Sponge && b.Block != Block.Sponge)
                            OtherPhysics.DoSpongeRemoved(lvl, index, false);
                        if (old == Block.LavaSponge && b.Block != Block.LavaSponge)
                            OtherPhysics.DoSpongeRemoved(lvl, index, true);

                        if (lvl.ActivatesPhysics(b.Block)) lvl.AddCheck(index);
                    }
                }
                op.TotalModified++;
                
                
                // Attempt to prevent BlockDB in-memory cache from growing too large (> 1,000,000 entries)
                int count = lvl.BlockDB.Cache.Count;
                if (count == 0 || (count % 1000000) != 0) return;
                
                // if drawop has a read lock on BlockDB (e.g. undo/redo), we must release it here
                bool hasReadLock = false;
                if (op.BlockDBReadLock != null) {
                    op.BlockDBReadLock.Dispose();
                    hasReadLock = true;
                }
                
                using (IDisposable wLock = lvl.BlockDB.Locker.AccquireWrite(100)) {
                    if (wLock != null) lvl.BlockDB.FlushCache();
                }
                
                if (!hasReadLock) return;
                op.BlockDBReadLock = lvl.BlockDB.Locker.AccquireRead();
            }
        }
    }
}
