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
using System.Collections.Generic;
using MCNebula.Blocks;
using MCNebula.Maths;
using BlockID = System.UInt16;

namespace MCNebula.Commands
{ 
    /// <summary> Provides helper methods for parsing arguments for commands. </summary>
    public static class CommandParser 
    {       
        /// <summary> Attempts to parse the given argument as a boolean. </summary>
        public static bool GetBool(Player p, string input, ref bool result) {
            if (input.CaselessEq("1") || input.CaselessEq("true")
                || input.CaselessEq("yes") || input.CaselessEq("on")) {
                result = true; return true;
            }
            
            if (input.CaselessEq("0") || input.CaselessEq("false")
                || input.CaselessEq("no") || input.CaselessEq("off")) {
                result = false; return true;
            }
            
            p.Message("&W\"{0}\" is not a valid boolean.", input);
            p.Message("&WValue must be either 1/yes/on or 0/no/off");
            return false;
        }
        
        /// <summary> Attempts to parse the given argument as an enumeration member. </summary>
        public static bool GetEnum<TEnum>(Player p, string input, string argName,
                                          ref TEnum result) where TEnum : struct {
            try {
                result = (TEnum)Enum.Parse(typeof(TEnum), input, true);
                if (Enum.IsDefined(typeof(TEnum), result)) return true;
            } catch {
            }
            
            string[] names = Enum.GetNames(typeof(TEnum));
            p.Message(argName + " must be one of the following: &f" + names.Join());
            return false;
        }
        
        /// <summary> Attempts to parse the given argument as an timespan in short form. </summary>
        public static bool GetTimespan(Player p, string input, ref TimeSpan span,
                                       string action, string defUnit) {
            try {
                span = input.ParseShort(defUnit);
                // Typically span gets added to current time, so check span isn't too big here
                DateTime.UtcNow.Add(span).AddYears(1);
                return true;
            } catch (OverflowException) {
                p.Message("&WTimespan given is too big");
            } catch (ArgumentOutOfRangeException) {
                p.Message("&WTimespan given is too big");
            } catch (FormatException ex) {
                p.Message("&W{0} is not a valid quantifier.", ex.Message);
                p.Message(TimespanHelp, action);
            }
            return false;
        }
        public const string TimespanHelp = "For example, to {0} 25 and a half hours, use \"1d1h30m\".";
        
        
        /// <summary> Returns whether the given value lies within the given range </summary>
        /// <remarks> If given value is not in range, messages the player the valid range of values </remarks>
        public static bool CheckRange(Player p, int value, string argName, int min, int max) {
            if (value >= min && value <= max) return true;
            
            // Try to provide more helpful range messages
            if (max == int.MaxValue) {
                p.Message("&W{0} must be {1} or greater", argName, min);
            } else if (min == int.MinValue) {
                p.Message("&W{0} must be {1} or less", argName, max);
            } else {
                p.Message("&W{0} must be between {1} and {2}", argName, min, max);
            }
            return false;
        }
        
        /// <summary> Attempts to parse the given argument as an integer. </summary>
        public static bool GetInt(Player p, string input, string argName, ref int result,
                                  int min = int.MinValue, int max = int.MaxValue) {
            int value;
            if (!NumberUtils.TryParseInt32(input, out value)) {
                p.Message("&W\"{0}\" is not a valid integer.", input); return false;
            }
            
            if (!CheckRange(p, value, argName, min, max)) return false;
            result = value; return true;
        }
        
        /// <summary> Attempts to parse the given argument as a real number. </summary>
        public static bool GetReal(Player p, string input, string argName, ref float result,
                                   float min = float.NegativeInfinity, float max = float.MaxValue) {
            float value;
            if (!NumberUtils.TryParseSingle(input, out value)) {
                p.Message("&W\"{0}\" is not a valid number.", input); return false;
            }
            
            if (value < min || value > max) {
                p.Message("&W{0} must be between {1} and {2}", argName, 
                               min.ToString("F4"), max.ToString("F4"));
                return false;
            }
            result = value; return true;
        }
        
        
        /// <summary> Attempts to parse the given argument as an byte. </summary>
        public static bool GetByte(Player p, string input, string argName, ref byte result,
                                   byte min = byte.MinValue, byte max = byte.MaxValue) {
            int temp = 0;
            if (!GetInt(p, input, argName, ref temp, min, max)) return false;
            
            result = (byte)temp; return true;
        }
        
        /// <summary> Attempts to parse the given argument as a ushort. </summary>
        public static bool GetUShort(Player p, string input, string argName, ref ushort result,
                                     ushort min = ushort.MinValue, ushort max = ushort.MaxValue) {
            int temp = 0;
            if (!GetInt(p, input, argName, ref temp, min, max)) return false;
            
            result = (ushort)temp; return true;
        }
        
        
        /// <summary> Attempts to parse the given argument as a hex color. </summary>
        public static bool GetHex(Player p, string input, ref ColorDesc col) {
            ColorDesc tmp;
            if (!Colors.TryParseHex(input, out tmp)) {
                p.Message("&W\"#{0}\" is not a valid HEX color.", input); return false;
            }
            col = tmp; return true;
        }
        
        /// <summary> Attempts to parse the 3 given arguments as coordinates. </summary>
        public static bool GetCoords(Player p, string[] args, int argsOffset, ref Vec3S32 P) {
            return
                GetCoordInt(p, args[argsOffset + 0], "X coordinate", ref P.X) &&
                GetCoordInt(p, args[argsOffset + 1], "Y coordinate", ref P.Y) &&
                GetCoordInt(p, args[argsOffset + 2], "Z coordinate", ref P.Z);
        }
        
        static bool ParseRelative(ref string arg) {
            // ~ is preferred for compatibility with modern minecraft command syntax
            // # is also accepted since ~ cannot be typed in original minecraft classic
            bool relative = arg.Length > 0 && (arg[0] == '~' || arg[0] == '#');
            if (relative) arg = arg.Substring(1);
            return relative;
        }

        /// <summary> Attempts to parse the given argument as a coordinate integer. </summary>
        public static bool GetCoordInt(Player p, string arg, string argName, ref int value) {
            bool relative = ParseRelative(ref arg);
            // ~ should work as ~0
            if (relative && arg.Length == 0) return true;
            int cur = value;
            
            if (!GetInt(p, arg, argName, ref value)) return false;
            if (relative) value += cur;
            return true;
        }
        
        /// <summary> Attempts to parse the given argument as a coordinate real number. </summary>
        public static bool GetCoordFloat(Player p, string arg, string argName, ref float value) {
            bool relative = ParseRelative(ref arg);
            // ~ should work as ~0
            if (relative && arg.Length == 0) return true;
            float cur = value;
            
            if (!GetReal(p, arg, argName, ref value)) return false;
            if (relative) value += cur;
            return true;
        }
        
        
        static bool IsSkipBlock(string input, out BlockID block) {
            // Skip/None block for draw operations
            if (input.CaselessEq("skip") || input.CaselessEq("none")) {
                block = Block.Invalid; return true;
            } else {
                block = Block.Air; return false;
            }
        }
        
        /// <summary> Attempts to parse the given argument as either a block name or a block ID. </summary>
        /// <remarks> Also ensures the player is allowed to place the given block. </remarks>
        public static bool GetBlockIfAllowed(Player p, string input, string action, 
                                             out BlockID block, bool allowSkip = false) {
            if (allowSkip && IsSkipBlock(input, out block)) return true;
            
            return GetBlock(p, input, out block) && IsBlockAllowed(p, action, block);
        }
        
        /// <summary> Attempts to parse the given argument as either a block name or a block ID. </summary>
        public static bool GetBlock(Player p, string input, out BlockID block, bool allowSkip = false) {
            if (allowSkip && IsSkipBlock(input, out block)) return true;
            
            block = Block.Parse(p, input);
            if (block == Block.Invalid) p.Message("&WThere is no block \"{0}\".", input);
            return block != Block.Invalid;
        }
        
        /// <summary> Returns whether the player is allowed to place/modify/delete the given block. </summary>
        /// <remarks> Outputs information of which ranks can modify the block if not. </remarks>
        public static bool IsBlockAllowed(Player p, string action, BlockID block) {
            if (p.group.CanPlace[block]) return true;
            BlockPerms.GetPlace(block).MessageCannotUse(p, action); // TODO: Delete permissions too?
            return false;
        }
        
        
        public static int GetBlocks(Player p, string input, 
                                    List<BlockID> blocks, bool allowSkip) {
            string[] bits;
            if (!IsRawBlockRange(input, out bits)) {
                BlockID block;
                
                if (!allowSkip || !IsSkipBlock(input, out block)) {
                    if (!GetBlock(p, input, out block)) return 0;
                }
                
                blocks.Add(block);
                return 1;
            }
            
            BlockID min = 0, max = 0;
            if (!GetUShort(p, bits[0], "Raw block ID", ref min, Block.Air, Block.MaxRaw)) return 0;
            if (!GetUShort(p, bits[1], "Raw block ID", ref max, Block.Air, Block.MaxRaw)) return 0;
            
            int count = 0;
            for (BlockID raw = min; raw <= max; raw++)
            {
                BlockID b = Block.FromRaw(raw);
                if (!Block.ExistsFor(p, b)) continue;
                
                blocks.Add(b);
                count++;
            }
            
            if (count > 0) return count;
            p.Message("&WNo usable blocks exist in the range from {0} to {1}",
                      min, max);
            return 0;
        }
        
        internal static bool IsRawBlockRange(string input, out string[] bits) {
            bits = null;
            if (input.IndexOf('-') == -1) return false;
            bits = input.Split(new char[] { '-' }, 2);
            
            int tmp;
            return NumberUtils.TryParseInt32(bits[0], out tmp)
                && NumberUtils.TryParseInt32(bits[1], out tmp);
        }
    }
}
