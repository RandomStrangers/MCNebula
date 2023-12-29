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
using MCNebula.Commands;
using BlockID = System.UInt16;

namespace MCNebula.Drawing.Brushes 
{
    public sealed class ReplaceBrushFactory : BrushFactory 
    {
        public override string Name { get { return "Replace"; } }       
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: [block1] [block2].. [new]",
            "&HDraws by replacing existing blocks that are in the given [blocks] with [new]",
            "&H  If only [block] is given, replaces with your held block.",
        };
        
        public override Brush Construct(BrushArgs args) { return ProcessReplace(args, false); }
        
        internal static Brush ProcessReplace(BrushArgs args, bool not) {
            string[] parts = args.Message.SplitSpaces();
            if (args.Message.Length == 0) {
                args.Player.Message("You need at least one block to replace."); return null;
            }
            
            int count = parts.Length == 1 ? 1 : parts.Length - 1;
            BlockID[] toAffect = GetBlocks(args.Player, 0, count, parts);
            if (toAffect == null) return null;
            
            BlockID target;
            if (!GetTargetBlock(args, parts, out target)) return null;
            
            if (not) return new ReplaceNotBrush(toAffect, target);
            return new ReplaceBrush(toAffect, target);
        }
        
        internal static BlockID[] GetBlocks(Player p, int start, int max, string[] parts) {
            List<BlockID> blocks = new List<BlockID>(max - start);
            
            for (int i = 0; start < max; start++, i++) 
            {
                int count = CommandParser.GetBlocks(p, parts[start], blocks, false);
                if (count == 0) return null;
            }
            
            foreach (BlockID b in blocks)
            {
                if (b == Block.Invalid) continue; // "Skip" block
                if (!CommandParser.IsBlockAllowed(p, "replace", b)) return null;
            }
            return blocks.ToArray();
        }
        
        static bool GetTargetBlock(BrushArgs args, string[] parts, out BlockID target) {
            Player p = args.Player;
            target = 0;
            
            if (parts.Length == 1) {
                if (!CommandParser.IsBlockAllowed(p, "draw with", args.Block)) return false;
                
                target = args.Block; return true;
            }            
            return CommandParser.GetBlockIfAllowed(p, parts[parts.Length - 1], "draw with", out target);
        }
    }
    
    public sealed class ReplaceNotBrushFactory : BrushFactory 
    {
        public override string Name { get { return "ReplaceNot"; } }        
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: [block1] [block2].. [new]",
            "&HDraws by replacing existing blocks that not are in the given [blocks] with [new]",
            "&H  If only [block] is given, replaces with your held block.",
        };
        
        public override Brush Construct(BrushArgs args) { return ReplaceBrushFactory.ProcessReplace(args, true); }
    }
}
