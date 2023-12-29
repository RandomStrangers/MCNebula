/*
    Copyright 2011 MCForge.
    
    Author: fenderrock87
    
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
using MCNebula.Util;

namespace MCNebula {
    public static class ProfanityFilter {
        static string[] reduceKeys, reduceValues;
        static List<string> filters;
        static bool hookedFilter;
        
        public static void Init() {
            InitReduceTable();
            LoadBadWords();
        }

        // Replace any words containing a bad word inside it (including partial bad word matches)
        public static string Parse(string text) {
            string[] words   = text.SplitSpaces();
            string[] reduced = Reduce(text).SplitSpaces();

            // Loop through each reduced word, looking for a bad word
            for (int i = 0; i < reduced.Length; i++) 
            {
                bool isFiltered = false;
                foreach (string filter in filters) 
                {
                    if (reduced[i].Contains(filter)) {
                        isFiltered = true; break;   
                    }
                }
                if (!isFiltered) continue;

                // If a bad word is found anywhere in the word, replace the word            
                words[i] = Replace(words[i]);
            }            
            return String.Join(" ", words);
        }
        
        static string Replace(string word) {
            string replacement = Server.Config.ProfanityReplacement;
            // for * repeat to ****
            return replacement.Length == 1 ? new string(replacement[0], word.Length) : replacement;
        }
        
        static void InitReduceTable() {
            if (reduceKeys != null) return;
            // Because some letters are similar (Like i and l), they are reduced to the same form.
            // For example, the word "@t3$5t ll" is reduced to "atesst ii";
            reduceKeys = "@|i3|l3|(|3|ph|6|#|l|!|1|0|9|$|5|vv|2".Split('|');
            reduceValues= "a|b|b|c|e|f|g|h|i|i|i|o|q|s|s|w|z".Split('|');
        }
        
        static void LoadBadWords() {
            TextFile filterFile = TextFile.Files["Profanity filter"];
            filterFile.EnsureExists();
            
            if (!hookedFilter) {
                hookedFilter = true;
                filterFile.OnTextChanged += LoadBadWords;
            }

            string[] lines = filterFile.GetText();
            filters = new List<string>();
            // Run the badwords through the reducer to ensure things like Ls become Is and everything is lowercase
            foreach (string line in lines) 
            {
                if (line.StartsWith("#") || line.Trim().Length == 0) continue;
                
                string word = Reduce(line);
                filters.Add(word);
            }
        }

        static string Reduce(string text) {
            text = text.ToLower();
            text = Colors.Strip(text);
            
            for (int i = 0; i < reduceKeys.Length; i++)
                text = text.Replace(reduceKeys[i], reduceValues[i]);
            return text;
        }
    }
}
