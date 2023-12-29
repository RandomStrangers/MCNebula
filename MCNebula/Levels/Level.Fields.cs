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
using System.Collections.Generic;
using System.Threading;
using MCNebula.Blocks;
using MCNebula.Blocks.Physics;
using MCNebula.DB;
using MCNebula.Maths;
using MCNebula.Network;
using MCNebula.Util;

namespace MCNebula {
    public sealed partial class Level : IDisposable {

        /// <summary>
        /// The name of the map file, sans extension.
        /// </summary>
        public string MapName;
        /// <summary>
        /// Same as MapName, unless <cref>IsMuseum</cref>, then it will be prefixed and suffixed to denote museum.
        /// </summary>
        public string name;

        public string ColoredName { get { return Config.Color + name; } }
        public LevelConfig Config = new LevelConfig();
        
        public byte rotx, roty;
        public ushort spawnx, spawny, spawnz;
        public Position SpawnPos { get { return new Position(16 + spawnx * 32, 32 + spawny * 32, 16 + spawnz * 32); } }
            
        public BlockDefinition[] CustomBlockDefs = new BlockDefinition[Block.SUPPORTED_COUNT];
        public BlockProps[] Props = new BlockProps[Block.SUPPORTED_COUNT];
        public ExtrasCollection Extras = new ExtrasCollection();
        public VolatileArray<PlayerBot> Bots = new VolatileArray<PlayerBot>();
        bool unloadedBots;
        
        public HandleDelete[] DeleteHandlers = new HandleDelete[Block.SUPPORTED_COUNT];
        public HandlePlace[] PlaceHandlers = new HandlePlace[Block.SUPPORTED_COUNT];
        public HandleWalkthrough[] WalkthroughHandlers = new HandleWalkthrough[Block.SUPPORTED_COUNT];
        public HandlePhysics[] PhysicsHandlers = new HandlePhysics[Block.SUPPORTED_COUNT];
        internal HandlePhysics[] physicsDoorsHandlers = new HandlePhysics[Block.SUPPORTED_COUNT];
        internal AABB[] blockAABBs = new AABB[Block.SUPPORTED_COUNT];
        
        /// <summary> The width of this level (Number of blocks across in X dimension) </summary>
        public ushort Width;
        /// <summary> The height of this level (Number of blocks tall in Y dimension) </summary>
        public ushort Height;
        /// <summary> The length of this level (Number of blocks across in Z dimension) </summary>
        public ushort Length;
        /// <summary> Whether this level should be treated as a readonly museum </summary>
        public bool IsMuseum;

        public int ReloadThreshold {
            get { return Math.Max(10000, (int)(Server.Config.DrawReloadThreshold * Width * Height * Length)); }
        }
        
        /// <summary> Maximum valid X coordinate (Width - 1) </summary>
        public int MaxX { get { return Width  - 1; } }
        /// <summary> Maximum valid Y coordinate (Height - 1) </summary>
        public int MaxY { get { return Height - 1; } }
        /// <summary> Maximum valid Z coordinate (Length - 1) </summary>
        public int MaxZ { get { return Length - 1; } }
        
        public bool Changed;
         /// <summary> Whether block changes made on this level should be saved to the BlockDB and .lvl files. </summary>
        public bool SaveChanges = true;
        public bool ChangedSinceBackup;
        
        /// <summary> Whether players on this level sees server-wide chat. </summary>
        public bool SeesServerWideChat { get { return Config.ServerWideChat && Server.Config.ServerWideChat; } }
        
        internal readonly object saveLock = new object(), botsIOLock = new object();
        public BlockQueue blockqueue = new BlockQueue();
        BufferedBlockSender bulkSender;

        public List<UndoPos> UndoBuffer = new List<UndoPos>();
        public VolatileArray<Zone> Zones = new VolatileArray<Zone>();
        public BlockDB BlockDB;
        public LevelAccessController VisitAccess, BuildAccess;
        
        // Physics fields and settings
        public int physics { get { return Physicsint; } }
        int Physicsint;
        public int currentUndo;
        
        public int lastCheck, lastUpdate;
        internal FastList<Check> ListCheck = new FastList<Check>(); //A list of blocks that need to be updated
        internal FastList<Update> ListUpdate = new FastList<Update>(); //A list of block to change after calculation
        internal SparseBitSet listCheckExists, listUpdateExists;
        
        public Random physRandom = new Random();
        public bool PhysicsPaused;
        Thread physThread;
        readonly object physThreadLock = new object();
        internal readonly object physTickLock = new object();
        bool physThreadStarted = false;
        internal DateTime lastBackup;
        
        public List<C4Data> C4list = new List<C4Data>();

        public bool CanPlace  { get { return Config.Buildable && Config.BuildType != BuildType.NoModify; } }
        public bool CanDelete { get { return Config.Deletable && Config.BuildType != BuildType.NoModify; } }

        public int WinChance {
            get { return Config.RoundsPlayed == 0 ? 100 : (Config.RoundsHumanWon * 100) / Config.RoundsPlayed; }
        }
        
        internal bool hasPortals, hasMessageBlocks;
    }
}