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
using System.ComponentModel;
using MCNebula.Games;
using MCNebula.Modules.Games.ZS;

namespace MCNebula.Gui {
    public sealed class ZombieProperties {  
        
        [Description("Whether players are allowed to pillar in zombie survival. " +
                     "Note this can be overriden for specific maps using /ZS set.")]
        [Category("General settings")]
        [DisplayName("Pillaring allowed")]
        public bool Pillaring { get; set; }
        
        
        [Description("Max distance players are allowed to move between packets. (for speedhack detection)")]
        [Category("Other settings")]
        [DisplayName("Max move distance")]
        public float MaxMoveDistance { get; set; }
        
        [Description("Distance between players before they are considered to have 'collided'. (for infecting)")]
        [Category("Other settings")]
        [DisplayName("Hitbox precision")]
        public float HitboxPrecision { get; set; }
        
        public void LoadFromServer() {
            ZSConfig cfg = ZSGame.Instance.Config;
            
            Pillaring = !cfg.NoPillaring;          
            MaxMoveDistance = cfg.MaxMoveDist;
            HitboxPrecision = cfg.HitboxDist;
        }
        
        public void ApplyToServer() {
            ZSConfig cfg = ZSGame.Instance.Config;

            cfg.NoPillaring = !Pillaring;            
            cfg.MaxMoveDist = MaxMoveDistance;
            cfg.HitboxDist = HitboxPrecision;
        }
    }
}
