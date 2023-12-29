﻿/*
    Copyright 2011 MCForge
        
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
using MCNebula.Events.LevelEvents;
using MCNebula.Events.PlayerEvents;
using MCNebula.Games;

namespace MCNebula.Modules.Games.Countdown 
{    
    public partial class CountdownGame : RoundsGame 
    {       
         protected override void HookEventHandlers() {
            OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
            OnPlayerSpawningEvent.Register(HandlePlayerSpawning, Priority.High);
            OnJoinedLevelEvent.Register(HandleOnJoinedLevel, Priority.High);
            OnGettingMotdEvent.Register(HandleGettingMotd, Priority.High);
            
            base.HookEventHandlers();
        }
        
        protected override void UnhookEventHandlers() {
            OnPlayerMoveEvent.Unregister(HandlePlayerMove);
            OnPlayerSpawningEvent.Unregister(HandlePlayerSpawning);
            OnJoinedLevelEvent.Unregister(HandleOnJoinedLevel);
            OnGettingMotdEvent.Unregister(HandleGettingMotd);
            
            base.UnhookEventHandlers();
        }
        
        void HandlePlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel) {
            if (!RoundInProgress || !FreezeMode) return;
            if (!Remaining.Contains(p)) return;
            
            int freezeX = p.Extras.GetInt("MCN_CD_X");
            int freezeZ = p.Extras.GetInt("MCN_CD_Z");
            if (next.X != freezeX || next.Z != freezeZ) {
                next.X = freezeX; next.Z = freezeZ;
                p.SendPosition(next, new Orientation(yaw, pitch));
            }
            
            p.Pos = next;
            p.SetYawPitch(yaw, pitch);
            cancel = true;
        }
        
        void HandlePlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning) {
            if (!respawning || !Remaining.Contains(p)) return;
            Map.Message(p.ColoredName + " &Sis out of countdown!");
            OnPlayerDied(p);
        }
        
        void HandleOnJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce) {
            HandleJoinedCommon(p, prevLevel, level, ref announce);
        }
        
        void HandleGettingMotd(Player p, ref string motd) {
            if (p.level != Map || !FreezeMode || !RoundInProgress) return;
            motd += " horspeed=0";
        }
    }
}
