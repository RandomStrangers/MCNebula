/*
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
using System;
using System.Collections.Generic;
using MCNebula.Network;
using MCNebula.Tasks;
using BlockID = System.UInt16;

namespace MCNebula {    
    /// <summary> Manages a list of block updates to periodically broadcast to players. </summary>
    public sealed class BlockQueue : List<ulong> {
        
        /// <summary> Time in milliseconds between ticks. </summary>
        public static int Interval = 100;
        /// <summary> Maximum number of block updates broadcasted in one tick. </summary>
        public static int UpdatesPerTick = 750;
        static BufferedBlockSender bulkSender = new BufferedBlockSender();
        
        const int posShift = 32;
        readonly object locker = new object();

        /// <summary> Flushes the block updates queue for each loaded level. </summary>
        public static void Loop(SchedulerTask task) {
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded) {
                lock (lvl.blockqueue.locker) {
                    lvl.blockqueue.Process(lvl);
                }
            }
            
            bulkSender.level = null;
            task.Delay = TimeSpan.FromMilliseconds(Interval);
        }

        /// <summary> Adds a block update to the end of the queue. </summary>
        public void Add(int index, BlockID block) {
            // Bit packing format
            // 32-63: index
            // 0-31 : block type
            ulong flags = (ulong)index << posShift;
            flags |= (ulong)block;
            
            lock (locker) Add(flags);
        }
        
        /// <summary> Removes all block updates from the queue. </summary>
        public void ClearAll() { lock (locker) Clear(); }
        
        void Process(Level lvl) {
            try {
                if (Count == 0) return;
                if (!lvl.HasPlayers()) { Clear(); return; }
                    
                bulkSender.level = lvl;
                int count = Count;
                if (count > UpdatesPerTick) count = UpdatesPerTick;

                for (int i = 0; i < count; i++) {
                    ulong flags   = this[i];
                    int index     = (int)(flags >> posShift);
                    BlockID block = (BlockID)flags;
                    bulkSender.Add(index, block);
                }
                bulkSender.Flush();
                RemoveRange(0, count);
            } catch (Exception e)  {
                Logger.LogError(e);
                Logger.Log(LogType.Warning, "Failed to flush block queue on {0}. {1} lost.", lvl.name, Count);
                Clear();
            }
        }
    }
}
