﻿/*
    Copyright 2011 MCForge
    
    Written by fenderrock87
    
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
using System.IO;
using MCNebula.Config;
using MCNebula.Games;
using MCNebula.Maths;
using BlockID = System.UInt16;

namespace MCNebula.Modules.Games.CTF
{    
    public sealed class CTFConfig : RoundsGameConfig 
    {
        public override bool AllowAutoload { get { return false; } }
        protected override string GameName { get { return "CTF"; } }
        
        [ConfigFloat("tag-distance", "Collisions", 1f)]
        public float TagDistance = 1f;
        [ConfigInt("collisions-check-interval", "Collisions", 150, 20, 2000)]
        public int CollisionsCheckInterval = 150;
    }
    
    public sealed class CTFMapConfig : RoundsGameMapConfig 
    {
        [ConfigVec3("red-spawn", null)] public Vec3U16 RedSpawn;
        [ConfigVec3("red-pos", null)] public Vec3U16 RedFlagPos;
        [ConfigBlock("red-block", null, Block.Air)]
        public BlockID RedFlagBlock;
        
        [ConfigVec3("blue-spawn", null)] public Vec3U16 BlueSpawn;
        [ConfigVec3("blue-pos", null)] public Vec3U16 BlueFlagPos;
        [ConfigBlock("blue-block", null, Block.Air)]
        public BlockID BlueFlagBlock;        
        
        [ConfigInt("map.line.z", null, 0)]
        public int ZDivider;
        [ConfigInt("game.maxpoints", null, 3)]
        public int RoundPoints = 3;
        [ConfigInt("game.tag.points-gain", null, 5)]
        public int Tag_PointsGained = 5;
        [ConfigInt("game.tag.points-lose", null, 5)]
        public int Tag_PointsLost = 5;
        [ConfigInt("game.capture.points-gain", null, 10)]
        public int Capture_PointsGained = 10;
        [ConfigInt("game.capture.points-lose", null, 10)]
        public int Capture_PointsLost = 10;

        
        const string propsDir = "properties/CTF/";
        static ConfigElement[] cfg;
        public override void Load(string map) {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(CTFMapConfig));
            LoadFrom(cfg, propsDir, map);
        }
        
        public override void Save(string map) {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(CTFMapConfig));
            SaveTo(cfg, propsDir, map);
        }
        
        public override void SetDefaults(Level lvl) {
            ZDivider = lvl.Length / 2;
            RedFlagBlock  = Block.Red;
            BlueFlagBlock = Block.Blue;

            ushort midX = (ushort)(lvl.Width / 2);
            ushort midY = (ushort)(lvl.Height / 2);
            ushort topY = (ushort)(midY + 2);
            ushort maxZ = (ushort)(lvl.Length - 1);
            
            RedFlagPos  = new Vec3U16(midX, topY,    0);
            RedSpawn    = new Vec3U16(midX, midY,    0);
            BlueFlagPos = new Vec3U16(midX, topY, maxZ);
            BlueSpawn   = new Vec3U16(midX, midY, maxZ);
        }
    }
}
