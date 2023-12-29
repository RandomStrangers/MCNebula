﻿/*
    Written by BeMacized
    Assisted by RedNoodle
    
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
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
using System.Collections.Generic;
using MCNebula.Commands;
using MCNebula.Events.PlayerEvents;

namespace MCNebula.Modules.Moderation.Review
{
    class CmdReview : Command2 
    {
        public override string name { get { return "Review"; } }
        public override string shortcut { get { return "rvw"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] {
                    new CommandPerm(LevelPermission.Operator, "can see the review queue"),
                    new CommandPerm(LevelPermission.Operator, "can teleport to next in review queue"),
                    new CommandPerm(LevelPermission.Operator, "can clear the review queue"),
                }; }
        }

        public override void Use(Player p, string message, CommandData data) {
            if (message.Length == 0 || message.CaselessEq("enter")) {
                HandleEnter(p, data);
            } else if (IsListAction(message)) {
                HandleView(p, data);
            } else if (message.CaselessEq("leave")) {
                HandleLeave(p);
            } else if (message.CaselessEq("next")) {
                HandleNext(p, data);
            } else if (message.CaselessEq("clear")) {
                HandleClear(p, data); 
            } else {
                Help(p);
            }
        }
        
        void HandleEnter(Player p, CommandData data) {
            if (p.IsSuper) { p.Message("{0} cannot enter the review queue.", p.SuperName); return; }
            TimeSpan delta = p.NextReviewTime - DateTime.UtcNow;
            
            if (delta.TotalSeconds >= 0) {
                p.Message("You must wait {0} before you can request another review",
                          delta.Shorten(true, true));
                return;
            }
            
            if (Server.reviewlist.Contains(p.name)) {
                p.Message("You are already in the review queue!"); return;
            }
     
            ItemPerms nextPerms = CommandExtraPerms.Find("Review", 2);
            bool anyStaff       = false;
            
            foreach (Player pl in PlayerInfo.GetOnlineCanSee(p, data.Rank)) 
            {
                if (nextPerms.UsableBy(pl)) {
                    anyStaff = true; break;
                }
            }
            
            Server.reviewlist.Add(p.name);
            int pos = Server.reviewlist.IndexOf(p.name) + 1;
            p.Message("You entered the &creview &Squeue at &aposition #" + pos);
            
            string msg = anyStaff ? 
                "The online staff have been notified. Someone should be with you shortly." :
                "There are currently no staff online. Staff will be notified when they join the server.";
            p.Message(msg);
            
            Chat.MessageFrom(ChatScope.Perms, p, 
                             "λNICK &Srequested a review! &c(Total " + pos + " waiting)", nextPerms, null, true);
            
            p.NextReviewTime = DateTime.UtcNow.Add(Server.Config.ReviewCooldown);
        }

        void HandleView(Player p, CommandData data) {
            if (!CheckExtraPerm(p, data, 1)) return;

            List<string> inQueue = Server.reviewlist.All();
            if (inQueue.Count == 0) {
                p.Message("There are no players in the review queue."); return;
            }
            
            p.Message("&9Players in the review queue:");
            int pos = 1;
            foreach (string name in inQueue) 
            {
                Group grp = PlayerInfo.GetGroup(name);
                p.Message("&a" + pos + ". &f" + name + " &a- Current Rank: " + grp.ColoredName);
                pos++;
            }
        }
        
        void HandleLeave(Player p) {
            if (Server.reviewlist.Remove(p.name)) {
                AnnounceQueueChanged();
                p.Message("You have left the review queue!");
            } else {
                p.Message("You weren't in the review queue to begin with.");
            }
        }
        
        void HandleNext(Player p, CommandData data) {
            if (p.IsSuper) { p.Message("{0} cannot answer the review queue.", p.SuperName); return; }
            if (!CheckExtraPerm(p, data, 2)) return;

            string user = Server.reviewlist.GetAt(0);
            if (user == null) {
                p.Message("There are no players in the review queue."); return;
            }
            
            Player target = PlayerInfo.FindExact(user);
            Server.reviewlist.Remove(user);
            
            if (target == null) {
                p.Message("Player " + user + " is offline, and was removed from the review queue");
                return;
            }
            
            Command.Find("TP").Use(p, target.name, data);
            p.Message("You have been teleported to " + p.FormatNick(target));
            target.Message("Your review request has been answered by {0}.", target.FormatNick(p));
            AnnounceQueueChanged();
        }
        
        void HandleClear(Player p, CommandData data) {
            if (!CheckExtraPerm(p, data, 3)) return;
            Server.reviewlist.Clear();
            p.Message("The review queue has been cleared");
        }
        
        static void AnnounceQueueChanged() {
            List<string> inQueue = Server.reviewlist.All();
            int pos = 1;
            
            foreach (string name in inQueue) 
            {
                Player who = PlayerInfo.FindExact(name);
                if (who == null) continue;
                
                who.Message("The review queue has changed. You are now at &aposition #" + pos);
                pos++;
            }
        }
        
        public override void Help(Player p) {
            p.Message("&T/Review enter/view/leave/next/clear");
            p.Message("&HLets you enter, view, leave, or clear the review queue, or " +
                      "teleport you to the next player in the review queue.");
        }
    }
}
