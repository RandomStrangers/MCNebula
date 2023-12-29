/*
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
using BlockID = System.UInt16;

namespace MCNebula.Config
{
    public abstract class ConfigInt64Attribute : ConfigAttribute
    {
        public ConfigInt64Attribute(string name, string section)
            : base(name, section) { }

        // separate function to avoid boxing in derived classes
        protected long ParseLong(string raw, long def, long min, long max)
        {
            long value;
            if (!NumberUtils.TryParseLong(raw, out value))
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" has invalid long '{2}', using default of {1}", Name, def, raw);
                value = def;
            }

            if (value < min)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too small a long, using {1}", Name, min);
                value = min;
            }
            if (value > max)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too big a long, using {1}", Name, max);
                value = max;
            }
            return value;
        }

        public override string Serialise(object value)
        {
            if (value is long) return NumberUtils.StringifyLong((long)value);

            return base.Serialise(value);
        }
    }

    public sealed class ConfigLongAttribute : ConfigInt64Attribute
    {
        long defValue, minValue, maxValue;

        public ConfigLongAttribute()
            : this(null, null, 0, long.MinValue, long.MaxValue) { }
        public ConfigLongAttribute(string name, string section, long def,
                                  long min = long.MinValue, long max = long.MaxValue)
            : base(name, section) { defValue = def; minValue = min; maxValue = max; }

        public override object Parse(string value)
        {
            return ParseLong(value, defValue, minValue, maxValue);
        }
    }
}