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
using System.Text;

namespace MCNebula 
{
    /// <summary> Represents which ranks are allowed (and which are disallowed) to use an item. </summary>
    public class ItemPerms 
    {
        public virtual string ItemName { get { return ""; } }
        public LevelPermission MinRank;
        public List<LevelPermission> Allowed, Disallowed;
        
        public ItemPerms(LevelPermission min) { MinRank = min; }
        
        protected void Init(LevelPermission min, List<LevelPermission> allowed,
                            List<LevelPermission> disallowed) {
            MinRank = min; Allowed = allowed; Disallowed = disallowed;
        }
        
        public void CopyPermissionsTo(ItemPerms dst) {
            dst.MinRank    = MinRank;
            dst.Allowed    = Allowed    == null ? null : new List<LevelPermission>(Allowed);
            dst.Disallowed = Disallowed == null ? null : new List<LevelPermission>(Disallowed);
        }
        
        public bool UsableBy(LevelPermission perm) {
            return (perm >= MinRank || (Allowed != null && Allowed.Contains(perm)))
                && (Disallowed == null || !Disallowed.Contains(perm));
        }
        
        public bool UsableBy(Player p) { return UsableBy(p.group.Permission); }
        
        
        public void Allow(LevelPermission rank) {
            if (Disallowed != null && Disallowed.Contains(rank)) {
                Disallowed.Remove(rank);
            } else if (Allowed == null || !Allowed.Contains(rank)) {
                if (Allowed == null) Allowed = new List<LevelPermission>();
                Allowed.Add(rank);
            }
        }
        
        public void Disallow(LevelPermission rank) {
            if (Allowed != null && Allowed.Contains(rank)) {
                Allowed.Remove(rank);
            } else if (Disallowed == null || !Disallowed.Contains(rank)) {
                if (Disallowed == null) Disallowed = new List<LevelPermission>();
                Disallowed.Add(rank);
            }
        }
        
        
        public void Describe(StringBuilder builder) {
            builder.Append(Group.GetColoredName(MinRank) + "&S+");
            
            if (Allowed != null && Allowed.Count > 0) {
                foreach (LevelPermission perm in Allowed) 
                {
                    builder.Append(", " + Group.GetColoredName(perm));
                }
                builder.Append("&S");
            }
            
            if (Disallowed != null && Disallowed.Count > 0) {
                builder.Append( " (except ");
                foreach (LevelPermission perm in Disallowed) 
                {
                    builder.Append(Group.GetColoredName(perm) + ", ");
                }
                builder.Remove(builder.Length - 2, 2);
                builder.Append("&S)");
            }
        }
        
        public string Describe() {
            StringBuilder sb = new StringBuilder();
            Describe(sb);
            return sb.ToString();
        }
        
        
        protected static void WriteHeader(StreamWriter w, string itemName, string itemDesc, 
                                          string headerName, string headerExample, string action) {
            w.WriteLine("#Version 2");
            w.WriteLine("#   This file contains the permissions to {1} {0}", itemDesc, action);
            w.WriteLine("#   How permissions work:");
            w.WriteLine("#   - If the player's rank is in Disallowed, they cannot {1} the {0}", itemName, action);
            w.WriteLine("#   - Otherwise if the player's rank is in Allowed, they can {1} the {0}", itemName, action);
            w.WriteLine("#   - Otherwise if the player's rank is >= Lowest Rank, they can {1} the {0}", itemName, action);
            w.WriteLine("#");
            w.WriteLine("#   Layout: {0} : LowestRank : Disallowed : Allowed", headerName);
            w.WriteLine("#      e.g. {0} : 60 : 80,67 : 40,41,55", headerExample);
            w.WriteLine("#   Notes:");
            w.WriteLine("#   - Disallowed and Allowed can be left empty");
            w.WriteLine("#   - Works entirely on rank permission values, not rank names");
            w.WriteLine("");
        }
        
        protected string Serialise() {
            StringBuilder sb = new StringBuilder(); // TODO: cache stringbuilder across calls?            
            sb.Append(ItemName);
            sb.Append(" : ");
            sb.Append(NumberUtils.StringifyInt((int)MinRank));
            sb.Append(" : ");
            AppendPerms(sb, Disallowed);
            sb.Append(" : ");
            AppendPerms(sb, Allowed);
            
            return sb.ToString();
        }
        
        static void AppendPerms(StringBuilder sb, List<LevelPermission> list) {
            if (list == null || list.Count == 0) return;
            string prefix = "";
            
            foreach (LevelPermission perm in list)
            {
                sb.Append(prefix);
                sb.Append(NumberUtils.StringifyInt((int)perm));
                prefix = ",";
            }
        }
        
        protected static void Deserialise(string[] args, int idx, out LevelPermission min,
                                          out List<LevelPermission> allowed, 
                                          out List<LevelPermission> disallowed) {
            min = (LevelPermission)NumberUtils.ParseInt32(args[idx]);
            disallowed = ExpandPerms(args[idx + 1]);
            allowed = ExpandPerms(args[idx + 2]);
        }
        
        static List<LevelPermission> ExpandPerms(string input) {
            if (input == null || input.Length == 0) return null;
            
            List<LevelPermission> perms = new List<LevelPermission>();
            foreach (string perm in input.SplitComma()) 
            {
                perms.Add((LevelPermission)NumberUtils.ParseInt32(perm));
            }
            return perms;
        }
    }
}
