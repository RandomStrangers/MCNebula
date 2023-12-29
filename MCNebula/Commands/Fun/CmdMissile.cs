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
using System;
using MCNebula.Games;

namespace MCNebula.Commands.Fun {
    public sealed class CmdMissile : Command2 {
        public override string name { get { return "Missile"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool SuperUseable { get { return false; } }
        
        public override void Use(Player p, string message, CommandData data) {
            if (!p.level.Config.Guns) {
                p.Message("Missiles cannot be used on this map!"); return;
            }
            if (p.weapon != null && message.Length == 0) {
                p.weapon.Disable(); return;
            }

            WeaponType type = Weapon.ParseType(message);
            if (type == WeaponType.Invalid) { Help(p); return; }
            
            Missile missile = GetMissile(type);
            missile.type = type;
            missile.Enable(p);
        }
                
        static Missile GetMissile(WeaponType type) {
            if (type == WeaponType.Destroy)  return new PenetrativeMissile();
            if (type == WeaponType.Teleport) return new TeleportMissile();
            if (type == WeaponType.Explode)  return new ExplosiveMissile();
            return new Missile();
        }
        
        public override void Help(Player p) {
            p.Message("&T/Missile [at end]");
            p.Message("&HAllows you to fire missiles at people. Differs from &T/gun &Hin that the missile is guided.");
            p.Message("&HAvailable [at end] types: &Sexplode, destroy, tp");
        }
    }
}
