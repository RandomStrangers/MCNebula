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
using MCNebula.Events;
using MCNebula.Events.EconomyEvents;
using MCNebula.Events.PlayerEvents;
using MCNebula.Events.ServerEvents;

namespace MCNebula.Core {

    public sealed class CorePlugin : Plugin {
        public override string name { get { return "CorePlugin"; } }

        public override void Load(bool startup) {
            OnPlayerConnectEvent.Register(ConnectHandler.HandleConnect, Priority.Critical);
            OnPlayerCommandEvent.Register(ChatHandler.HandleCommand, Priority.Critical);
            OnChatEvent.Register(ChatHandler.HandleOnChat, Priority.Critical);
            OnPlayerStartConnectingEvent.Register(ConnectingHandler.HandleConnecting, Priority.Critical);
            
            OnSentMapEvent.Register(MiscHandlers.HandleSentMap, Priority.Critical);
            OnPlayerMoveEvent.Register(MiscHandlers.HandlePlayerMove, Priority.Critical);
            OnPlayerClickEvent.Register(MiscHandlers.HandlePlayerClick, Priority.Critical);
            OnChangedZoneEvent.Register(MiscHandlers.HandleChangedZone, Priority.Critical);
            
            OnEcoTransactionEvent.Register(EcoHandlers.HandleEcoTransaction, Priority.Critical);
            OnModActionEvent.Register(ModActionHandler.HandleModAction, Priority.Critical);
        }
        
        public override void Unload(bool shutdown) {
            OnPlayerConnectEvent.Unregister(ConnectHandler.HandleConnect);
            OnPlayerCommandEvent.Unregister(ChatHandler.HandleCommand);
            OnChatEvent.Unregister(ChatHandler.HandleOnChat);
            OnPlayerStartConnectingEvent.Unregister(ConnectingHandler.HandleConnecting);
            
            OnSentMapEvent.Unregister(MiscHandlers.HandleSentMap);
            OnPlayerMoveEvent.Unregister(MiscHandlers.HandlePlayerMove);
            OnPlayerClickEvent.Unregister(MiscHandlers.HandlePlayerClick);
            OnChangedZoneEvent.Unregister(MiscHandlers.HandleChangedZone);
            
            OnEcoTransactionEvent.Unregister(EcoHandlers.HandleEcoTransaction);
            OnModActionEvent.Unregister(ModActionHandler.HandleModAction);
        }
    }
}
