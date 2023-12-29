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
using MCNebula.Drawing.Ops;
using BlockID = System.UInt16;

namespace MCNebula.Drawing.Brushes 
{ 
    public sealed class RainbowBrush : CheckeredPaletteBrush 
    {
        public override string Name { get { return "Rainbow"; } }
        public RainbowBrush() : base(blocks) {}
        
        internal static BlockID[] blocks = new BlockID[] { 
            Block.Red,   Block.Orange,  Block.Yellow,
            Block.Lime,  Block.Green,   Block.Teal,
            Block.Aqua,  Block.Cyan,    Block.Blue,
            Block.Indigo, Block.Violet, Block.Magenta,
            Block.Pink };
    }
    
    public sealed class BWRainbowBrush : CheckeredPaletteBrush 
    {        
        public override string Name { get { return "BWRainbow"; } }
        public BWRainbowBrush() : base(blocks) {}
        
        internal static BlockID[] blocks = new BlockID[] { 
            Block.Iron,  Block.White,    Block.Gray,
            Block.Black, Block.Obsidian, Block.Black, 
            Block.Gray,  Block.White };
    }
    
    public sealed class RandomRainbowBrush : Brush 
    {
        readonly Random rnd = new Random();
        readonly BlockID[] blocks;

        public override string Name { get { return "RandomRainbow"; } }
        
        public RandomRainbowBrush(BlockID[] list) { blocks = list; }      
        
        public override BlockID NextBlock(DrawOp op) {
            return blocks[rnd.Next(blocks.Length)];
        }
    }
}
