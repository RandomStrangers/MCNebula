using System;
using System.IO;
using MCNebula.Network;
namespace MCNebula.Commands
{
    public class CmdCCHeartbeat : Command
    {
        public override string name { get { return "CCHeartbeat"; } }
        public override string shortcut { get { return "CCBeat"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Owner; } }

        public override void Use(Player p, string message)
        {
            try
            {
                Heartbeat.Heartbeats[0].Pump();
                p.Message("Heartbeat pump sent.");
                p.Message("Server URL: " + ((ClassiCubeBeat)Heartbeat.Heartbeats[0]).LastResponse);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Error with ClassiCube pump.", e);
                p.Message("Error with ClassiCube pump: " + e + ".");
            }
        }
        public override void Help(Player p)
        {
            p.Message("/CCHeartbeat - Forces a pump for the ClassiCube heartbeat.  DEBUG PURPOSES ONLY.");
        }
    }
    public sealed class CmdUrl : Command2
    {
        public override string name { get { return "ServerUrl"; } }
        public override string shortcut { get { return "url"; } }
        public override string type { get { return "information"; } }
        public override bool SuperUseable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }


        public override void Use(Player p, string message, CommandData data)
        {
            if (p.IsConsole)
            {
                p.Message("Seriously? Just go look at it!");
                p.cancelcommand = true;
            }
            else
            {
                string file = "./text/externalurl.txt";
                string contents = File.ReadAllText(file);
                p.Message("Server URL: " + contents);
                string file2 = "./text/externalurl2.txt";
                string contents2 = File.ReadAllText(file2);
                p.Message("Server URL: " + contents2);
                return;
            }
        }
        public override void Help(Player p)
        {
            p.Message("%T/ServerUrl %H- Shows the server's ClassiCube URL.");
        }
    }
}