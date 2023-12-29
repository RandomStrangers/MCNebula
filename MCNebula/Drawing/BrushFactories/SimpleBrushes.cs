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
    public sealed class SolidBrushFactory : BrushFactory 
    {
        public override string Name { get { return "Normal"; } }
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: [block]",
            "&HDraws using the specified block.",
            "&H  If [block] is not given, your currently held block is used.",
        };
        
        public override Brush Construct(BrushArgs args) {
            Player p = args.Player;
            if (args.Message.Length == 0) {
                if (!CommandParser.IsBlockAllowed(p, "draw with", args.Block)) return null;
                return new SolidBrush(args.Block);
            }
            
            BlockID block;
            if (!CommandParser.GetBlockIfAllowed(p, args.Message, "draw with", out block)) return null;
            return new SolidBrush(block);
        }
        
        // Usually this shouldn't be overriden, but since SolidBrush is the default brush, 
        //  it's worth overriding this to avoid an unnecessary object allocation
        public override bool Validate(BrushArgs args) {
            if (args.Message.Length == 0) return true;
            BlockID block;
            return CommandParser.GetBlockIfAllowed(args.Player, args.Message, "draw with", out block);
        }
    }
    
    public sealed class CheckeredBrushFactory : BrushFactory 
    {
        public override string Name { get { return "Checkered"; } }
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: [block1] [block2] <block3>..",
            "&HDraws an alternating pattern of blocks.",
            "&H  If [block1] is not given, your currently held block is used.",
            "&H  If [block2] is not given, skip block is used.",
        };
        
        public override Brush Construct(BrushArgs args) {
            Player p = args.Player;
            // avoid allocating the arrays for the most common case
            // TODO remove?
            if (args.Message.Length == 0) {
                if (!CommandParser.IsBlockAllowed(p, "draw with", args.Block)) return null;
                return new CheckeredBrush(args.Block, Block.Invalid);
            }

            List<BlockID> toAffect;
            List<int> freqs;
            
            bool ok = FrequencyBrush.GetBlocks(args, out toAffect, out freqs, 
                                               P => false, null);
            if (!ok) return null;

            BlockID[] blocks = FrequencyBrush.Combine(toAffect, freqs);
            if (blocks.Length == 2)
                return new CheckeredBrush(blocks[0], blocks[1]);
            return new CheckeredPaletteBrush(blocks);
        }
    }

    public sealed class GridBrushFactory : BrushFactory 
    {
        public override string Name { get { return "Grid"; } }
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: [grid block]/<size> [cell block]/<size> <border>",
            "&HDraws an gridline pattern of blocks.",
            "&H  If a <size> is not given, a size of 1 is assumed.",
            "&H  If <border> block is not given, skip block is used.",
        };
        
        public override Brush Construct(BrushArgs args) {
            List<BlockID> toAffect;
            List<int> freqs;
            
            bool ok = FrequencyBrush.GetBlocks(args, out toAffect, out freqs, 
                                               P => false, null);
            if (!ok) return null;

            return new GridBrush(toAffect, freqs);
        }
    }
    
    public sealed class PasteBrushFactory : BrushFactory 
    {
        public override string Name { get { return "Paste"; } }
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: none",
            "&HDraws by pasting blocks from current &T/Copy.",
            "&TArguments: [block1] [block2]..",
            "&HDraws by pasting only the given blocks from current &T/Copy.",
            "&TArguments: not [block1] [block2]..",
            "&HDraws by pasting blocks from current &T/Copy, &Sexcept for the given blocks.",
        };
        
        public override Brush Construct(BrushArgs args) {
            CopyState cState = args.Player.CurrentCopy;
            if (cState == null) {
                args.Player.Message("You haven't copied anything yet");
                return null;
            }
            
            if (args.Message.Length == 0)
                return new SimplePasteBrush(cState);
            string[] parts = args.Message.SplitSpaces();            
            
            if (parts[0].CaselessEq("not")) {
                PasteNotBrush brush = new PasteNotBrush(cState);
                brush.Exclude = ReplaceBrushFactory.GetBlocks(args.Player, 1, parts.Length, parts);
                return brush.Exclude == null ? null : brush;
            } else {
                PasteBrush brush = new PasteBrush(cState);
                brush.Include = ReplaceBrushFactory.GetBlocks(args.Player, 0, parts.Length, parts);
                return brush.Include == null ? null : brush;
            }
        }
    }
    
    public sealed class StripedBrushFactory : BrushFactory 
    {
        public override string Name { get { return "Striped"; } }
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: [block1] [block2]",
            "&HDraws a diagonally-alternating pattern of block1 and block2.",
            "&H   If block1 is not given, the currently held block is used.",
            "&H   If block2 is not given, air is used.",
        };
        
        public override Brush Construct(BrushArgs args) {
            Player p = args.Player;
            if (args.Message.Length == 0) {
                if (!CommandParser.IsBlockAllowed(p, "draw with", args.Block)) return null;
                return new StripedBrush(args.Block, Block.Invalid);
            }
            string[] parts = args.Message.SplitSpaces();
            
            BlockID block1;
            if (!CommandParser.GetBlockIfAllowed(p, parts[0], "draw with", out block1, true)) return null;
            if (parts.Length == 1)
                return new StripedBrush(block1, Block.Invalid);
            
            BlockID block2;
            if (!CommandParser.GetBlockIfAllowed(p, parts[1], "draw with", out block2, true)) return null;
            return new StripedBrush(block1, block2);
        }
    }
    
    
    public sealed class RainbowBrushFactory : BrushFactory 
    {
        public override string Name { get { return "Rainbow"; } }
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: none or 'random'",
            "&HIf no arguments are given, draws a diagonally repeating rainbow",
            "&HIf 'random' is given, draws by randomly selecting blocks from the rainbow pattern.",
        };
        
        public override Brush Construct(BrushArgs args) {
            if (args.Message.CaselessEq("random")) 
                return new RandomRainbowBrush(RainbowBrush.blocks);
            return new RainbowBrush();
        }
    }
    
    public sealed class BWRainbowBrushFactory : BrushFactory 
    {
        public override string Name { get { return "BWRainbow"; } }
        public override string[] Help { get { return HelpString; } }
        
        static string[] HelpString = new string[] {
            "&TArguments: none or 'random'",
            "&HIf no arguments are given, draws a diagonally repeating black-white rainbow",
            "&HIf 'random' is given, draws by randomly selecting blocks from the rainbow pattern.",
        };
        
        public override Brush Construct(BrushArgs args) { 
            if (args.Message.CaselessEq("random")) 
                return new RandomRainbowBrush(BWRainbowBrush.blocks);
            return new BWRainbowBrush();
        }
    }
}
