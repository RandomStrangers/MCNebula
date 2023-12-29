﻿/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using MCNebula.Blocks;
using MCNebula.Network;
using BlockID = System.UInt16;

namespace MCNebula 
{
    public static partial class Block 
    {
        static string[] coreNames = new string[Block.CORE_COUNT];
        public static bool Undefined(BlockID block) { return IsPhysicsType(block) && coreNames[block].CaselessEq("unknown"); }
        
        public static bool ExistsGlobal(BlockID b) { return ExistsFor(Player.Console, b); }
        
        public static bool ExistsFor(Player p, BlockID b) {
            if (b < Block.CORE_COUNT) return !Undefined(b);
            
            if (!p.IsSuper) return p.level.GetBlockDef(b) != null;
            return BlockDefinition.GlobalDefs[b] != null;
        }
        
        /// <summary> Gets the name for the block with the given block ID </summary>
        /// <remarks> Block names can differ depending on the player's level </remarks>
        public static string GetName(Player p, BlockID block) {
            if (IsPhysicsType(block)) return coreNames[block];
            
            BlockDefinition def;
            if (!p.IsSuper) {
                def = p.level.GetBlockDef(block);
            } else {
                def = BlockDefinition.GlobalDefs[block];
            }
            if (def != null) return def.Name.Replace(" ", "");
            
            return block < CPE_COUNT ? coreNames[block] : ToRaw(block).ToString();
        }

        public static BlockID Parse(Player p, string input) {
            BlockDefinition[] defs = p.IsSuper ? BlockDefinition.GlobalDefs : p.level.CustomBlockDefs;
            BlockID block;
            // raw ID is treated specially, before names
            if (BlockID.TryParse(input, out block)) {
                if (block < Block.CPE_COUNT || (block <= Block.MaxRaw && defs[FromRaw(block)] != null)) {
                    return FromRaw(block);
                } // TODO redo to use ExistsFor?
            }
            
            BlockDefinition def = BlockDefinition.ParseName(input, defs);
            if (def != null) return def.GetBlock();
            
            byte coreID;
            bool success = Aliases.TryGetValue(input.ToLower(), out coreID);
            return success ? coreID : Invalid;
        }
        
        public static string GetColoredName(Player p, BlockID block) {
            BlockPerms perms = BlockPerms.GetPlace(block); // TODO check Delete perms too?
            return Group.GetColor(perms.MinRank) + Block.GetName(p, block);
        }
        
        
        /// <summary> Converts a block &lt;= CPE_MAX_BLOCK into a suitable
        /// block compatible for the given classic protocol version </summary>
        public static byte ConvertClassic(byte block, byte protocolVersion) {
            // protocol version 7 only supports up to Obsidian block
            if (protocolVersion >= Server.VERSION_0030) {
                return block <= Obsidian ? block : v7_fallback[block - CobblestoneSlab];
            }
            
            // protocol version 6 only supports up to Gold block
            if (protocolVersion >= Server.VERSION_0020) {
                return block <= Gold ? block : v6_fallback[block - Iron];
            }
            
            // protocol version 5 only supports up to Glass block
            if (protocolVersion >= Server.VERSION_0019) {
                return block <= Glass ? block : v5_fallback[block - Red];
            }

            // protocol version 4 only supports up to Leaves block
            //  protocol version 3 seems to have same support
            //  TODO what even changed between 3 and 4?
            return block <= Leaves ? block : v4_fallback[block - Sponge];
        }

        static byte[] v7_fallback = {
            // CobbleSlab Rope      Sandstone Snow Fire  LightPink ForestGreen Brown
               Slab,      Mushroom, Sand,     Air, Lava, Pink,     Green,      Dirt,
            // DeepBlue Turquoise Ice    CeramicTile Magma     Pillar Crate StoneBrick
               Blue,    Cyan,     Glass, Iron,       Obsidian, White, Wood, Stone
        };
        static byte[] v6_fallback = {
            // Iron   DoubleSlab Slab  Brick TNT  Bookshelf MossyRocks   Obsidian
               Stone, Gray,      Gray, Red,  Red, Wood,     Cobblestone, Black,
            // CobbleSlab   Rope      Sandstone Snow Fire  LightPink ForestGreen Brown
               Cobblestone, Mushroom, Sand,     Air, Lava, Pink,     Green,      Dirt,
            // DeepBlue Turquoise Ice    CeramicTile Magma        Pillar Crate StoneBrick
               Blue,    Cyan,     Glass, Gold,       Cobblestone, White, Wood, Stone
        };
        static byte[] v5_fallback = {
            // Red   Orange Yellow Lime  Green Teal  Aqua  Cyan
               Sand, Sand,  Sand,  Sand, Sand, Sand, Sand, Sand,
            // Blue  Indigo Violet Magenta Pink  Black  Gray   White
               Sand, Sand,  Sand,  Sand,   Sand, Stone, Stone, Sand,
            // Dandelion Rose     BrownShroom RedShroom Gold
               Sapling,  Sapling, Sapling,    Sapling,  Sponge,
            // Iron   DoubleSlab Slab   Brick        TNT   Bookshelf MossyRocks   Obsidian
               Stone, Stone,     Stone, Cobblestone, Sand, Wood,     Cobblestone, Cobblestone,
            // CobbleSlab   Rope     Sandstone Snow Fire  LightPink ForestGreen Brown
               Cobblestone, Sapling, Sand,     Air, Lava, Sand,     Sand,       Dirt,
            // DeepBlue Turquoise Ice    CeramicTile Magma        Pillar Crate StoneBrick
               Sand,    Sand,     Glass, Stone,      Cobblestone, Stone, Wood, Stone
        };
        static byte[] v4_fallback = {
            // Sponge   Glass
               GoldOre, Leaves,
            // Red   Orange Yellow Lime  Green Teal  Aqua  Cyan
               Sand, Sand,  Sand,  Sand, Sand, Sand, Sand, Sand,
            // Blue  Indigo Violet Magenta Pink  Black  Gray   White
               Sand, Sand,  Sand,  Sand,   Sand, Stone, Stone, Sand,
            // Dandelion Rose     BrownShroom RedShroom Gold
               Sapling,  Sapling, Sapling,    Sapling,  GoldOre,
            // Iron   DoubleSlab Slab   Brick        TNT   Bookshelf MossyRocks   Obsidian
               Stone, Stone,     Stone, Cobblestone, Sand, Wood,     Cobblestone, Cobblestone,
            // CobbleSlab   Rope     Sandstone Snow Fire  LightPink ForestGreen Brown
               Cobblestone, Sapling, Sand,     Air, Lava, Sand,     Sand,       Dirt,
            // DeepBlue Turquoise Ice     CeramicTile Magma        Pillar Crate StoneBrick
               Sand,    Sand,     Leaves, Stone,      Cobblestone, Stone, Wood, Stone
        };
        
        
        /// <summary> Converts physics block IDs to their visual block IDs </summary>
        /// <remarks> If block ID is not converted, returns input block ID </remarks>
        /// <example> Op_Glass becomes Glass, Door_Log becomes Log </example>
        public static BlockID Convert(BlockID block) {
            switch (block) {
                case FlagBase: return Mushroom;
                case Op_Glass: return Glass;
                case Op_Obsidian: return Obsidian;
                case Op_Brick: return Brick;
                case Op_Stone: return Stone;
                case Op_Cobblestone: return Cobblestone;
                case Op_Air: return Air; //Must be cuboided / replaced
                case Op_Water: return StillWater;
                case Op_Lava: return StillLava;

                case 108: return Cobblestone;
                case LavaSponge: return Sponge;

                case FloatWood: return Wood;
                case FastLava: return Lava;
                case 71:
                case 72:
                    return White;
                case Door_Log: return Log;
                case Door_Obsidian: return Obsidian;
                case Door_Glass: return Glass;
                case Door_Stone: return Stone;
                case Door_Leaves: return Leaves;
                case Door_Sand: return Sand;
                case Door_Wood: return Wood;
                case Door_Green: return Green;
                case Door_TNT: return TNT;
                case Door_Slab: return Slab;
                case Door_Iron: return Iron;
                case Door_Dirt: return Dirt;
                case Door_Grass: return Grass;
                case Door_Blue: return Blue;
                case Door_Bookshelf: return Bookshelf;
                case Door_Gold: return Gold;
                case Door_Cobblestone: return Cobblestone;
                case Door_Red: return Red;

                case Door_Orange: return Orange;
                case Door_Yellow: return Yellow;
                case Door_Lime: return Lime;
                case Door_Teal: return Teal;
                case Door_Aqua: return Aqua;
                case Door_Cyan: return Cyan;
                case Door_Indigo: return Indigo;
                case Door_Purple: return Violet;
                case Door_Magenta: return Magenta;
                case Door_Pink: return Pink;
                case Door_Black: return Black;
                case Door_Gray: return Gray;
                case Door_White: return White;

                case tDoor_Log: return Log;
                case tDoor_Obsidian: return Obsidian;
                case tDoor_Glass: return Glass;
                case tDoor_Stone: return Stone;
                case tDoor_Leaves: return Leaves;
                case tDoor_Sand: return Sand;
                case tDoor_Wood: return Wood;
                case tDoor_Green: return Green;
                case tDoor_TNT: return TNT;
                case tDoor_Slab: return Slab;
                case tDoor_Air: return Air;
                case tDoor_Water: return StillWater;
                case tDoor_Lava: return StillLava;

                case oDoor_Log: return Log;
                case oDoor_Obsidian: return Obsidian;
                case oDoor_Glass: return Glass;
                case oDoor_Stone: return Stone;
                case oDoor_Leaves: return Leaves;
                case oDoor_Sand: return Sand;
                case oDoor_Wood: return Wood;
                case oDoor_Green: return Green;
                case oDoor_TNT: return TNT;
                case oDoor_Slab: return Slab;
                case oDoor_Lava: return StillLava;
                case oDoor_Water: return StillWater;

                case MB_White: return White;
                case MB_Black: return Black;
                case MB_Air: return Air;
                case MB_Water: return StillWater;
                case MB_Lava: return StillLava;

                case WaterDown: return Water;
                case LavaDown: return Lava;
                case WaterFaucet: return Aqua;
                case LavaFaucet: return Orange;

                case FiniteWater: return Water;
                case FiniteLava: return Lava;
                case FiniteFaucet: return Cyan;

                case Portal_Air: return Air;
                case Portal_Water: return StillWater;
                case Portal_Lava: return StillLava;

                case Door_Air: return Air;
                case Door_AirActivatable: return Air;
                case Door_Water: return StillWater;
                case Door_Lava: return StillLava;

                case Portal_Blue: return Cyan;
                case Portal_Orange: return Orange;

                case C4: return TNT;
                case C4Detonator: return Red;
                case TNT_Small: return TNT;
                case TNT_Big: return TNT;
                case TNT_Explosion: return Lava;

                case LavaFire: return Lava;
                case TNT_Nuke: return TNT;

                case RocketStart: return Glass;
                case RocketHead: return Gold;
                case Fireworks: return Iron;

                case Deadly_Water: return StillWater;
                case Deadly_Lava: return StillLava;
                case Deadly_Air: return Air;
                case Deadly_ActiveWater: return Water;
                case Deadly_ActiveLava: return Lava;
                case Deadly_FastLava: return Lava;

                case Magma: return Lava;
                case Geyser: return Water;
                case Checkpoint: return Air;

                case Air_Flood:
                case Door_Log_air:
                case Air_FloodLayer:
                case Air_FloodDown:
                case Air_FloodUp:
                case 205:
                case 206:
                case 207:
                case 208:
                case 209:
                case 210:
                case 213:
                case 214:
                case 215:
                case 216:
                case Door_Air_air:
                case 225:
                case 254:
                case 81:
                case 226:
                case 227:
                case 228:
                case 229:
                case 84:
                case 66:
                case 67:
                case 68:
                case 69:
                    return Air;
                case Door_Green_air: return Red;
                case Door_TNT_air: return Lava;

                case oDoor_Log_air:
                case oDoor_Obsidian_air:
                case oDoor_Glass_air:
                case oDoor_Stone_air:
                case oDoor_Leaves_air:
                case oDoor_Sand_air:
                case oDoor_Wood_air:
                case oDoor_Slab_air:
                case oDoor_Lava_air:
                case oDoor_Water_air:
                    return Air;
                case oDoor_Green_air: return Red;
                case oDoor_TNT_air: return StillLava;

                case Train: return Aqua;

                case Snake: return Black;
                case SnakeTail: return CoalOre;

                case Creeper: return TNT;
                case ZombieBody: return MossyRocks;
                case ZombieHead: return Lime;

                case Bird_White: return White;
                case Bird_Black: return Black;
                case Bird_Lava: return Lava;
                case Bird_Red: return Red;
                case Bird_Water: return Water;
                case Bird_Blue: return Blue;
                case Bird_Killer: return Lava;

                case Fish_Betta: return Blue;
                case Fish_Gold: return Gold;
                case Fish_Salmon: return Red;
                case Fish_Shark: return Gray;
                case Fish_Sponge: return Sponge;
                case Fish_LavaShark: return Obsidian;
            }
            return block;
        }
    }
}
