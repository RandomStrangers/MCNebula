﻿/*
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
///////--|----------------------------------|--\\\\\\\
//////---|  TNT WARS - Coded by edh649      |---\\\\\\
/////----|                                  |----\\\\\
////-----|  Note: Double click on // to see |-----\\\\
///------|        them in the sidebar!!     |------\\\
//-------|__________________________________|-------\\
using System;
using System.IO;
using MCNebula.Config;
using MCNebula.Games;
using MCNebula.Maths;

namespace MCNebula.Modules.Games.TW
{
    public sealed class TWConfig : RoundsGameConfig 
    {
        public override bool AllowAutoload { get { return false; } }
        protected override string GameName { get { return "TNT Wars"; } }
        
        [ConfigEnum("Mode", "Defaults", TWGameMode.TDM, typeof(TWGameMode))]
        public TWGameMode Mode = TWGameMode.TDM;
        [ConfigEnum("Difficulty", "Defaults", TWDifficulty.Normal, typeof(TWDifficulty))]
        public TWDifficulty Difficulty = TWDifficulty.Normal;
    }
    
    public sealed class TWMapConfig : RoundsGameMapConfig 
    {    
        [ConfigBool("grace-period", null, true)]
        public bool GracePeriod = true;
        [ConfigTimespan("grace-time", null, 30, false)]
        public TimeSpan GracePeriodTime = TimeSpan.FromSeconds(30);
        
        [ConfigInt("max-active-tnt", null, 1)]
        public int MaxActiveTnt = 1;
        
        [ConfigBool("team-balance", null, true)]
        public bool BalanceTeams = true;
        [ConfigBool("team-kill", null, false)]
        public bool TeamKills;
        
        [ConfigInt("score-needed", null, 150)]
        public int ScoreRequired = 150;
        [ConfigInt("scores-per-kill", null, 10)]
        public int ScorePerKill = 10;
        [ConfigInt("score-assist", null, 5)]
        public int AssistScore = 5;
        [ConfigInt("score-multi-kill-bonus", null, 5)]
        public int MultiKillBonus = 5; // Amount of extra points per player killed (if more than one) per TNT
        
        [ConfigBool("streaks", null, true)]
        public bool Streaks = true;
        public int StreakOneAmount = 3;
        public float StreakOneMultiplier = 1.25f;
        public int StreakTwoAmount = 5;
        public float StreakTwoMultiplier = 1.5f;
        public int StreakThreeAmount = 7;
        public float StreakThreeMultiplier = 2f;
        
        [ConfigVec3("red-spawn", null)]  public Vec3U16 RedSpawn;
        [ConfigVec3("blue-spawn", null)] public Vec3U16 BlueSpawn;
        
        const string propsDir = "properties/tntwars/";
        static ConfigElement[] cfg;        
        public override void Load(string map) {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(TWMapConfig));
            LoadFrom(cfg, propsDir, map);
        }
        
        public override void Save(string map) {
            if (cfg == null) cfg = ConfigElement.GetAll(typeof(TWMapConfig));
            SaveTo(cfg, propsDir, map);
        }
        
        public override void SetDefaults(Level lvl) {
            ushort midX = (ushort)(lvl.Width / 2);
            ushort midY = (ushort)(lvl.Height / 2 + 1);
            ushort maxZ = (ushort)(lvl.Length - 1);
            
            RedSpawn  = new Vec3U16(midX, midY, 0);
            BlueSpawn = new Vec3U16(midX, midY, maxZ);
        }
    }
}