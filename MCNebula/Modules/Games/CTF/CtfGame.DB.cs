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
using MCNebula.Games;
using MCNebula.SQL;

namespace MCNebula.Modules.Games.CTF
{
    public partial class CTFGame : RoundsGame 
    {
        struct CtfStats { public int Points, Captures, Tags; }
        
        static ColumnDesc[] ctfTable = new ColumnDesc[] {
            new ColumnDesc("ID", ColumnType.Integer, priKey: true, autoInc: true, notNull: true),
            new ColumnDesc("Name", ColumnType.VarChar, 20),
            new ColumnDesc("Points", ColumnType.UInt24),
            new ColumnDesc("Captures", ColumnType.UInt24),
            new ColumnDesc("tags", ColumnType.UInt24),
        };
        
        static CtfStats ParseStats(ISqlRecord record) {
            CtfStats stats;
            stats.Points   = record.GetInt("Points");
            stats.Captures = record.GetInt("Captures");
            stats.Tags     = record.GetInt("Tags");
            return stats;
        }
        
        static CtfStats LoadStats(string name) {
            CtfStats stats = default(CtfStats);
            Database.ReadRows("CTF", "*",
                                record => stats = ParseStats(record),
                                "WHERE Name=@0", name);
            return stats;
        }
        
        protected override void SaveStats(Player p) {
            CtfData data = TryGet(p);
            if (data == null || data.Points == 0 && data.Captures == 0 && data.Tags == 0) return;
            
            object[] args = new object[] {
                 data.Points, data.Captures, data.Tags, 
                 p.name
            };
            
            int changed = Database.UpdateRows("CTF", "Points=@0, Captures=@1, tags=@2", 
                                              "WHERE Name=@3", args);
            if (changed == 0) {
                Database.AddRow("CTF", "Points, Captures, tags, Name", args);
            }
        }
    }
}
