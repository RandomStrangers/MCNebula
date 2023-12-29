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
using System.IO;
using BlockID = System.UInt16;

namespace MCNebula.Blocks 
{
    /// <summary> Represents which ranks are allowed (and which are disallowed) to use a block. </summary>
    public sealed class BlockPerms : ItemPerms 
    {
        public BlockID ID;
        public override string ItemName { get { return ID.ToString(); } }
        
        static BlockPerms[] PlaceList  = new BlockPerms[Block.SUPPORTED_COUNT];
        static BlockPerms[] DeleteList = new BlockPerms[Block.SUPPORTED_COUNT];
        
        
        public BlockPerms(BlockID id, LevelPermission min) : base(min) {
            ID = id;
        }
        
        public BlockPerms Copy() {
            BlockPerms copy = new BlockPerms(ID, 0);
            CopyPermissionsTo(copy); return copy;
        }        
       

        public static BlockPerms GetPlace(BlockID b)  { return PlaceList[b];  }
        public static BlockPerms GetDelete(BlockID b) { return DeleteList[b]; }

        
        public static void ResendAllBlockPermissions() {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) { pl.SendCurrentBlockPermissions(); }
        }
        
        public void MessageCannotUse(Player p, string action) {
            p.Message("Only {0} can {1} {2}",
                      Describe(), action, Block.GetName(p, ID));
        }
        
        
        static readonly object ioLock = new object();
        /// <summary> Saves list of block permissions to disc. </summary>
        public static void Save() {
            try {
                lock (ioLock) SaveCore();
            } catch (Exception ex) { 
                Logger.LogError("Error saving block perms", ex); 
            }
        }
        
        static void SaveCore() {
            SaveList(Paths.PlacePermsFile,  PlaceList,  "use");
            SaveList(Paths.DeletePermsFile, DeleteList, "delete");
        }
        
        static void SaveList(string path, BlockPerms[] list, string action) {
            using (StreamWriter w = new StreamWriter(path)) {
                WriteHeader(w, "block", "each block", "Block ID", "lava", action);

                foreach (BlockPerms perms in list) 
                {
                    if (Block.Undefined(perms.ID)) continue;
                    w.WriteLine(perms.Serialise());
                }
            }
        }

        
        /// <summary> Applies new block permissions to server state. </summary>
        public static void ApplyChanges() {
            foreach (Group grp in Group.AllRanks) 
            {
                SetUsable(grp);
            }
        }
        
        public static void SetUsable(Group grp) {
            SetUsableList(PlaceList,  grp.CanPlace,  grp);
            SetUsableList(DeleteList, grp.CanDelete, grp);
        }
        
        static void SetUsableList(BlockPerms[] list, bool[] permsList, Group grp) {
            foreach (BlockPerms perms in list) 
            {
                permsList[perms.ID] = perms.UsableBy(grp.Permission);
            }
        }
        

        /// <summary> Loads list of block permissions from disc. </summary>
        public static void Load() {
            lock (ioLock) LoadCore();
            ApplyChanges();
        }

        static void LoadCore() {
            SetDefaultPerms();
            bool placeExists  = File.Exists(Paths.PlacePermsFile);
            bool deleteExists = File.Exists(Paths.DeletePermsFile);
            
            if (placeExists)  LoadFile(Paths.PlacePermsFile,  PlaceList);
            if (deleteExists) LoadFile(Paths.DeletePermsFile, DeleteList);
            if (placeExists || deleteExists) return;
            
            if (File.Exists(Paths.BlockPermsFile)) {
                LoadFile(Paths.BlockPermsFile, PlaceList);
                
                for (int i = 0; i < Block.SUPPORTED_COUNT; i++)
                    PlaceList[i].CopyPermissionsTo(DeleteList[i]);
                SetDefaultSpecialDeletePerms();
                
                try { 
                    File.Move(Paths.BlockPermsFile, Paths.BlockPermsFile + ".bak"); 
                } catch (Exception ex) {
                    Logger.LogError("Moving old block.properties file", ex);
                }
            }
            Save();
        }
        
        static void LoadFile(string path, BlockPerms[] list) {
            using (StreamReader r = new StreamReader(path)) {
                ProcessLines(r, list);
            }
        }
        
        static void ProcessLines(StreamReader r, BlockPerms[] list) {
            string[] args = new string[4];
            string line;
            
            while ((line = r.ReadLine()) != null) {
                if (line.IsCommentLine()) continue;
                // Format - ID : Lowest : Disallow : Allow
                line.Replace(" ", "").FixedSplit(args, ':');
                
                BlockID block;
                if (!BlockID.TryParse(args[0], out block)) {
                    // Old format - Name : Lowest : Disallow : Allow
                    block = Block.Parse(Player.Console, args[0]);
                }
                if (block == Block.Invalid) continue;

                try {
                    LevelPermission min;
                    List<LevelPermission> allowed, disallowed;
                    
                    Deserialise(args, 1, out min, out allowed, out disallowed);
                    Set(block, min, list,  allowed, disallowed);
                } catch {
                    Logger.Log(LogType.Warning, "Hit an error on the block " + line);
                    continue;
                }
            }
        }
        
        static void Set(BlockID b, LevelPermission min, BlockPerms[] list,
                        List<LevelPermission> allowed, List<LevelPermission> disallowed) {
            BlockPerms perms = list[b];
            if (perms == null) {
                perms   = new BlockPerms(b, min);
                list[b] = perms;
            }
            perms.Init(min, allowed, disallowed);
        }
        
       
        static void SetDefaultPerms() {
            for (BlockID block = 0; block < Block.SUPPORTED_COUNT; block++) 
            {
                BlockProps props = Block.Props[block];
                LevelPermission min;
                
                if (block == Block.Invalid) {
                    min = LevelPermission.Admin;
                } else if (props.OPBlock) {
                    min = LevelPermission.Operator;
                } else if (props.IsDoor || props.IsTDoor || props.oDoorBlock != Block.Invalid) {
                    min = LevelPermission.Builder;
                } else if (props.IsPortal || props.IsMessageBlock) {
                    min = LevelPermission.AdvBuilder;
                } else {
                    min = DefaultPerm(block);
                }
                
                Set(block, min, PlaceList,  null, null);
                Set(block, min, DeleteList, null, null);
            }
            SetDefaultSpecialDeletePerms();
        }
        
        static void SetDefaultSpecialDeletePerms() {
            for (BlockID b = Block.Water; b <= Block.StillLava; b++)
                DeleteList[b].MinRank = LevelPermission.Guest;
        }
        
        static LevelPermission DefaultPerm(BlockID block) {
            switch (block) {
                case Block.Bedrock:
                case Block.Air_Flood:
                case Block.Air_FloodDown:
                case Block.Air_FloodLayer:
                case Block.Air_FloodUp:

                case Block.TNT_Big:
                case Block.TNT_Nuke:
                case Block.RocketStart:
                case Block.RocketHead:

                case Block.Creeper:
                case Block.ZombieBody:
                case Block.ZombieHead:

                case Block.Bird_Red:
                case Block.Bird_Killer:
                case Block.Bird_Blue:

                case Block.Fish_Gold:
                case Block.Fish_Sponge:
                case Block.Fish_Shark:
                case Block.Fish_Salmon:
                case Block.Fish_Betta:
                case Block.Fish_LavaShark:

                case Block.Snake:
                case Block.SnakeTail:
                case Block.FlagBase:
                    return LevelPermission.Operator;

                case Block.FloatWood:
                case Block.LavaSponge:
                case Block.Door_Log_air:
                case Block.Door_Green_air:
                case Block.Door_TNT_air:

                case Block.Water:
                case Block.Lava:
                case Block.FastLava:
                case Block.WaterDown:
                case Block.LavaDown:
                case Block.WaterFaucet:
                case Block.LavaFaucet:
                case Block.FiniteWater:
                case Block.FiniteLava:
                case Block.FiniteFaucet:
                case Block.Magma:
                case Block.Geyser:
                case Block.Deadly_Lava:
                case Block.Deadly_Water:
                case Block.Deadly_Air:
                case Block.Deadly_ActiveWater:
                case Block.Deadly_ActiveLava:
                case Block.Deadly_FastLava:
                case Block.LavaFire:

                case Block.C4:
                case Block.C4Detonator:
                case Block.TNT_Small:
                case Block.TNT_Explosion:
                case Block.Fireworks:
                case Block.Checkpoint:
                case Block.Train:

                case Block.Bird_White:
                case Block.Bird_Black:
                case Block.Bird_Water:
                case Block.Bird_Lava:
                    return LevelPermission.AdvBuilder;
            }
            return LevelPermission.Guest;
        }
    }
}
