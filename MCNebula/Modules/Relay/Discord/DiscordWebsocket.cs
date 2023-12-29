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
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MCNebula.Config;
using MCNebula.Network;
using MCNebula.Tasks;

namespace MCNebula.Modules.Relay.Discord 
{    
    public sealed class DiscordSession 
    { 
        public string ID, LastSeq;
        public int Intents = DiscordWebsocket.DEFAULT_INTENTS;
    }
    public delegate string DiscordGetStatus();
    
    /// <summary> Implements a basic websocket for communicating with Discord's gateway </summary>
    /// <remarks> https://discord.com/developers/docs/topics/gateway </remarks>
    /// <remarks> https://i.imgur.com/Lwc5Wde.png </remarks>
    public sealed class DiscordWebsocket : ClientWebSocket 
    {       
        /// <summary> Authorisation token for the bot account </summary>
        public string Token;
        public string Host;
        
        public bool CanReconnect = true, SentIdentify;
        public DiscordSession Session;
        
        /// <summary> Whether presence support is enabled </summary>
        public bool Presence = true;
        /// <summary> Presence status (E.g. online) </summary>
        public PresenceStatus Status;
        /// <summary> Presence activity (e.g. Playing) </summary>
        public PresenceActivity Activity;
        /// <summary> Callback function to retrieve the activity status message </summary>
        public DiscordGetStatus GetStatus;
        
        /// <summary> Callback invoked when a ready event has been received </summary>
        public Action<JsonObject> OnReady;
        /// <summary> Callback invoked when a resumed event has been received </summary>
        public Action<JsonObject> OnResumed;
        /// <summary> Callback invoked when a message created event has been received </summary>
        public Action<JsonObject> OnMessageCreate;
        /// <summary> Callback invoked when a channel created event has been received </summary>
        public Action<JsonObject> OnChannelCreate;
        
        readonly object sendLock = new object();
        SchedulerTask heartbeat;
        TcpClient client;
        SslStream stream;
        bool readable;

        public const int DEFAULT_INTENTS = INTENT_GUILD_MESSAGES | INTENT_DIRECT_MESSAGES | INTENT_MESSAGE_CONTENT;
        const int INTENT_GUILD_MESSAGES  = 1 << 9;
        const int INTENT_DIRECT_MESSAGES = 1 << 12;
        const int INTENT_MESSAGE_CONTENT = 1 << 15;

        const int OPCODE_DISPATCH        = 0;
        const int OPCODE_HEARTBEAT       = 1;
        const int OPCODE_IDENTIFY        = 2;
        const int OPCODE_STATUS_UPDATE   = 3;
        const int OPCODE_VOICE_STATE_UPDATE = 4;
        const int OPCODE_RESUME          = 6;
        const int OPCODE_REQUEST_SERVER_MEMBERS = 8;
        const int OPCODE_INVALID_SESSION = 9;
        const int OPCODE_HELLO           = 10;
        const int OPCODE_HEARTBEAT_ACK   = 11;
        
        
        public DiscordWebsocket(string apiPath) {
            path = apiPath;
        }
        
        // stubs
        public override bool LowLatency { set { } }
        public override IPAddress IP { get { return null; } }
        
        public void Connect() {
            client = new TcpClient();
            client.Connect(Host, 443);
            readable = true;

            stream   = HttpUtil.WrapSSLStream(client.GetStream(), Host);
            protocol = this;
            Init();
        }
        
        protected override void WriteCustomHeaders() {
            WriteHeader("Authorization: Bot " + Token);
            WriteHeader("Host: " + Host);
        }
        
        public override void Close() {
            readable = false;
            Server.Heartbeats.Cancel(heartbeat);
            try {
                client.Close();
            } catch {
                // ignore errors when closing socket
            }
        }
        
        const int REASON_INVALID_TOKEN = 4004;
        const int REASON_DENIED_INTENT = 4014;
        
        protected override void OnDisconnected(int reason) {
            SentIdentify = false;
            if (readable) Logger.Log(LogType.SystemActivity, "Discord relay bot closing: " + reason);
            Close();

            if (reason == REASON_INVALID_TOKEN) {
                CanReconnect = false;
                throw new InvalidOperationException("Discord relay: Invalid bot token provided - unable to connect");
            } else if (reason == REASON_DENIED_INTENT) {
                // privileged intent since August 2022 https://support-dev.discord.com/hc/en-us/articles/4404772028055
                CanReconnect = false;
                throw new InvalidOperationException("Discord relay: Message Content Intent is not enabled in Bot Account settings, " +
                    "therefore Discord will prevent the bot from being able to see the contents of Discord messages\n" +
                    "(See https://github.com/UnknownShadow200/MCGalaxy/wiki/Discord-relay-bot#read-permissions)");
            }
        }
        
        
        public void ReadLoop() {
            byte[] data = new byte[4096];
            readable = true;

            while (readable) 
            {
                int len = stream.Read(data, 0, 4096);
                if (len == 0) throw new IOException("stream.Read returned 0");
                
                HandleReceived(data, len);
            }
        }
        
        protected override void HandleData(byte[] data, int len) {
            string value   = Encoding.UTF8.GetString(data, 0, len);
            JsonReader ctx = new JsonReader(value);
            JsonObject obj = (JsonObject)ctx.Parse();
            if (obj == null) return;
            
            int opcode = NumberUtils.ParseInt32((string)obj["op"]);
            DispatchPacket(opcode, obj);
        }
        
        void DispatchPacket(int opcode, JsonObject obj) {
            if (opcode == OPCODE_DISPATCH) {
                HandleDispatch(obj);
            } else if (opcode == OPCODE_HELLO) {
                HandleHello(obj);
            } else if (opcode == OPCODE_INVALID_SESSION) {
                // See notes at https://discord.com/developers/docs/topics/gateway#resuming
                //  (note that in this implementation, if resume fails, the bot just
                //   gives up altogether instead of trying to resume again later)
                Session.ID      = null;
                Session.LastSeq = null;
                
                Logger.Log(LogType.Warning, "Discord relay: Resuming failed, trying again in 5 seconds");
                Thread.Sleep(5 * 1000);
                Identify();
            }
        }
        
        
        void HandleHello(JsonObject obj) {
            JsonObject data = (JsonObject)obj["d"];
            string interval = (string)data["heartbeat_interval"];            
            int msInterval  = NumberUtils.ParseInt32(interval);
            
            heartbeat = Server.Heartbeats.QueueRepeat(SendHeartbeat, null, 
                                          TimeSpan.FromMilliseconds(msInterval));
            Identify();
        }
        
        void HandleDispatch(JsonObject obj) {
            // update last sequence number
            object sequence;
            if (obj.TryGetValue("s", out sequence)) 
                Session.LastSeq = (string)sequence;
            
            string eventName = (string)obj["t"];
            JsonObject data;
            
            if (eventName == "READY") {
                data = (JsonObject)obj["d"];
                HandleReady(data);
                OnReady(data);
            } else if (eventName == "RESUMED") {
                data = (JsonObject)obj["d"];
                OnResumed(data);
            } else if (eventName == "MESSAGE_CREATE") {
                data = (JsonObject)obj["d"];
                OnMessageCreate(data);
            } else if (eventName == "CHANNEL_CREATE") {
                data = (JsonObject)obj["d"];
                OnChannelCreate(data);
            }
        }
        
        void HandleReady(JsonObject data) {
            object session;
            if (data.TryGetValue("session_id", out session)) 
                Session.ID = (string)session;
        }
        
        
        public void SendMessage(int opcode, JsonObject data) {
            JsonObject obj = new JsonObject()
            {
                { "op", opcode },
                { "d",  data }
            };
            SendMessage(obj);
        }
        
        public void SendMessage(JsonObject obj) {
            string str = Json.SerialiseObject(obj);
            Send(Encoding.UTF8.GetBytes(str), SendFlags.None);
        }
        
        protected override void SendRaw(byte[] data, SendFlags flags) {
            lock (sendLock) stream.Write(data);
        }
        
        void SendHeartbeat(SchedulerTask task) {
            JsonObject obj = new JsonObject();
            obj["op"] = OPCODE_HEARTBEAT;
            
            if (Session.LastSeq != null) {
                obj["d"] = NumberUtils.ParseInt32(Session.LastSeq);
            } else {
                obj["d"] = null;
            }
            SendMessage(obj);
        }
        
        public void Identify() {
            if (Session.ID != null && Session.LastSeq != null) {
                SendMessage(OPCODE_RESUME,   MakeResume());
            } else {
                SendMessage(OPCODE_IDENTIFY, MakeIdentify());
            }
            SentIdentify = true;
        }
        
        public void UpdateStatus() {
            JsonObject data = MakePresence();
            SendMessage(OPCODE_STATUS_UPDATE, data);
        }
        
        JsonObject MakeResume() {
            return new JsonObject()
            {
                { "token",      Token },
                { "session_id", Session.ID },
                { "seq",        NumberUtils.ParseInt32(Session.LastSeq) }
            };
        }
        
        JsonObject MakeIdentify() {
            JsonObject props = new JsonObject()
            {
                { "$os",      "linux" },
                { "$browser", Server.SoftwareName },
                { "$device",  Server.SoftwareName }
            };
            
            return new JsonObject()
            {
                { "token",      Token },
                { "intents",    Session.Intents },
                { "properties", props },
                { "presence",   MakePresence() }
            };
        }
        
        JsonObject MakePresence() {
            if (!Presence) return null;
            
            JsonObject activity = new JsonObject()
            {
                { "name", GetStatus() },
                { "type", (int)Activity }
            };
            return new JsonObject()
            {
                { "since",      null },
                { "activities", new JsonArray() { activity } },
                { "status",     Status.ToString() },
                { "afk",        false }
            };
        }
    }
}
