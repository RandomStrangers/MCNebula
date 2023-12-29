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
using System.Collections.Generic;
using MCNebula.Games;

namespace MCNebula.Modules.Games.ZS
{
    sealed class CmdBounties : Command2 
    {
        public override string name { get { return "Bounties"; } }
        public override string type { get { return CommandTypes.Games; } }
        
        public override void Use(Player p, string message, CommandData data) {
            BountyData[] bounties = BountyData.Bounties.Items;
            if (bounties.Length == 0) {
                p.Message("There are no active bounties."); return;
            }
            
            foreach (BountyData bounty in bounties) 
            {
                Player pl = PlayerInfo.FindExact(bounty.Target);
                if (pl == null) continue;
                
                p.Message("Bounty for {0} &Sis &a{1} &S{2}.", 
                          p.FormatNick(pl), bounty.Amount, Server.Config.Currency);
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/Bounties");
            p.Message("&HOutputs a list of all active bounties on players.");
        }
    }
}
