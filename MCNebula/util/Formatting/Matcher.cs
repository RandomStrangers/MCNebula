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
using System.Text;

namespace MCNebula 
{  
    /// <summary> Finds partial matches of a 'name' against the names of the items in an enumerable. </summary>
    /// <remarks> returns number of matches found, and the matching item if only 1 match is found. </remarks>
    public static class Matcher 
    {
        /// <summary> Finds partial matches of 'color' against the list of colors. </summary>
        public static string FindColor(Player p, string color) {
            int matches;
            ColorDesc desc = Find(p, color, out matches, Colors.List,
                                  col => !col.Undefined, col => col.Name, "colors", 20);
            return desc.Undefined ? null : "&" + desc.Code;
        }
        
        /// <summary> Finds partial matches of 'name' against the list of bots in same level as player. </summary>
        public static PlayerBot FindBots(Player p, string name) {
            int matches;
            return Find(p, name, out matches, p.level.Bots.Items,
                        null, b => b.name, "bots");
        }
        
        /// <summary> Find partial matches of 'name' against the list of loaded maps/levels. </summary>
        public static Level FindLevels(Player p, string name) {
            int matches;
            return Find(p, name, out matches, LevelInfo.Loaded.Items,
                        null, l => l.name, l => l.ColoredName, "loaded levels");
        }

        /// <summary> Find partial matches of 'name' against the list of all map files. </summary>
        public static string FindMaps(Player pl, string name) {
            if (!Formatter.ValidMapName(pl, name)) return null;            
            int matches;
            return Find(pl, name, out matches, LevelInfo.AllMapNames(),
                        null, l => l, "levels", 10);
        }
        
        /// <summary> Find partial matches of 'name' against the list of ranks. </summary>
        public static Group FindRanks(Player p, string name) {
            Group.MapName(ref name);
            int matches;
            return Find(p, name, out matches, Group.GroupList,
                        null, g => Colors.Strip(g.Name), g => g.ColoredName, "ranks");
        }
        
        /// <summary> Find partial matches of 'name' against the list of zones in a map. </summary>
        public static Zone FindZones(Player p, Level lvl, string name) {
            int matches;
            return Find(p, name, out matches, lvl.Zones.Items,
                        null, z => z.Config.Name, "zones");
        }
        
        
        /// <summary> Finds partial matches of 'name' against the names of the items in the 'items' enumerable. </summary>
        /// <returns> If exactly one match, the matching item. </returns>
        public static T Find<T>(Player p, string name, out int matches, IEnumerable<T> items,
                                Predicate<T> filter, StringFormatter<T> nameGetter, string group, int limit = 5)  {
            return Find<T>(p, name, out matches, items, filter, nameGetter, nameGetter, group, limit);
        }
        
        
        /// <summary> Finds partial matches of 'name' against the names of the items in the 'items' enumerable. </summary>
        /// <returns> If exactly one match, the matching item. </returns>
        public static T Find<T>(Player p, string name, out int matches, IEnumerable<T> items,
                                Predicate<T> filter, StringFormatter<T> nameGetter, 
                                StringFormatter<T> itemFormatter, string group, int limit = 5)  {
            T match = default(T); matches = 0;
            StringBuilder output = new StringBuilder();
            const StringComparison comp = StringComparison.OrdinalIgnoreCase;

            foreach (T item in items)
            {
                if (filter != null && !filter(item)) continue;
                string itemName = nameGetter(item);
                if (itemName.Equals(name, comp)) { matches = 1; return item; }
                if (itemName.IndexOf(name, comp) < 0) continue;
                
                match = item; matches++;
                if (matches <= limit) {
                    output.Append(itemFormatter(item)).Append("&S, ");
                } else if (matches == limit + 1) {
                    output.Append("(and more), ");
                }
            }
            
            if (matches == 1) return match;
            if (matches == 0) {
                p.Message("No {0} match \"{1}\".", group, name); return default(T);
            }
            
            string count = matches > limit ? limit + "+ " : matches + " ";
            string names = output.ToString(0, output.Length - 2);
            
            p.Message("{0}{1} match \"{2}\":", count, group, name);
            p.Message(names);
            return default(T);
        }
    }
}
