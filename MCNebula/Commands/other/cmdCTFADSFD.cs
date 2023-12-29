/*
    Copyright 2010 MCLawl (Modified for use with MCForge)
    
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
/*using System;
using System.Collections.Generic;
using System.Text;

namespace MCGalaxy.Commands
{
    public class CmdCtf : Command
    {
        public override string name { get { return "CTF"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }

        public override void Use(Player p, string message)
        {
            if (message.IndexOf(' ') != -1)
            {
                if (message.SplitSpaces()[0].ToLower() == "team")
                {
                    if (message.SplitSpaces()[1].ToLower() == "create")
                    {
                        if (message.SplitSpaces().Length < 3) { Help(p); return; }
                        string color = c.Parse(message.SplitSpaces()[2].ToLower());
                        char teamCol = (char)color[1];
                        switch (teamCol)
                        {
                            case '2':
                            case '5':
                            case '8':
                            case '9':
                            case 'c':
                            case 'e':
                            case 'f':
                                CtfInitTeam(p, color);
                                break;
                            default:
                                p.Message("Invalid team color chosen.");
                                return;
                        }
                        return;
                    }
                    else if (message.SplitSpaces()[1].ToLower() == "add")
                    {
                        if (message.SplitSpaces().Length < 4) { Help(p); return; }
                        Player newPlayer = PlayerInfo.Find(message.SplitSpaces()[2].ToLower());
                        if (newPlayer == null) { Help(p); return; }
                        string color = c.Parse(message.SplitSpaces()[3].ToLower());
                        AddTeamMember(p, newPlayer, color);
                        return;
                    }
                    else if (message.SplitSpaces()[1].ToLower() == "remove")
                    {
                        if (message.SplitSpaces().Length < 3) { Help(p); return; }
                        Player newPlayer = PlayerInfo.Find(message.SplitSpaces()[2].ToLower());
                        Team workTeam = newPlayer.level.teams.Find(team => team.color == newPlayer.onTeam);
                        workTeam.removePlayer(newPlayer);
                        if (newPlayer == null) { Help(p); return; }

                    }
                }
                else if (message.SplitSpaces()[0].ToLower() == "flag")
                {
                    if (message.SplitSpaces().Length < 2) { Help(p); return; }
                    string color = c.Parse(message.SplitSpaces()[1].ToLower());
                    CatchPos cpos;
                    cpos.x = 0; cpos.y = 0; cpos.z = 0; cpos.color = color; p.blockchangeObject = cpos;
                    p.Message("Place a block to determine where to place the flag.");
                    p.ClearBlockchange();
                    p.Blockchange += AddFlag;
                }
                else if (message.SplitSpaces()[0].ToLower() == "reset")
                {
                    if (message.SplitSpaces().Length < 2) { Help(p); return; }
                    if (message.SplitSpaces()[1] == "round")
                    {
                        foreach (Team team in p.level.teams)
                        {
                            team.ResetRound();
                        }
                    }
                    else { Help(p); return; }

                }
                else if (message.SplitSpaces()[0].ToLower() == "spawn")
                {
                    if (message.SplitSpaces().Length < 2) { Help(p); return; }
                    string color = c.Parse(message.SplitSpaces()[1].ToLower());
                    AddTeamSpawn(p, color);
                }
                else if (message.SplitSpaces()[0].ToLower() == "points")
                {
                    if (message.SplitSpaces().Length < 2) { Help(p); return; }
                    int i;
                    Int32.TryParse(message.SplitSpaces()[1], out i);
                    if (i == 0) { p.Message("You must indicate a numeric points value greater than 0."); return; }
                    p.level.maxroundpoints = i;
                    p.Message("Max points has been set to " + i);
                }

            }
            else if (message.SplitSpaces()[0].ToLower() == "debug")
            {
                p.Message("Player debug info: hasFlag: " + p.hasFlag + ", holdingflag: " + p.holdingFlag);
                p.Message("OnTeam: " + p.onTeam + ", inCtf: " + p.inCtf);
                Team workTeam = p.level.teams.Find(team => team.color == p.onTeam);
                p.Message("Team debug info: flagishome: " + workTeam.flagishome + ", Points: " + workTeam.points);
                p.Message("Flag base: x: " + workTeam.flagBase[0] + ", y: " + workTeam.flagBase[1] + ", z: " + workTeam.flagBase[2]);
                p.Message("Flag loc:  x: " + workTeam.flagLocation[0] + ", y: " + workTeam.flagLocation[1] + ", z: " + workTeam.flagLocation[2]);
                p.Message("Level ctfmode: " + workTeam.mapOn.ctfmode);
            } 
            else if (message.SplitSpaces()[0].ToLower() == "clear")
            {
                if (message.SplitSpaces().Length > 1 && message.SplitSpaces().Length < 1) { Help(p); return;}
                foreach (Team team in p.level.teams)
                {
                    foreach (Player p1 in team.onTeam)
                    {
                        p1.inCtf = false;
                        p1.holdingFlag = false;
                    }
                    team.onTeam.Clear();
                    p.level.ctfmode = false;
                    p.level.maxroundpoints = 0;
                    team.hasFlag = null;
                }
                p.level.teams.Clear();
                p.level.ChatLevel("Capture the flag data has been reset.");
            }
            else if (message.Length == 0)
            {
                if (!p.level.ctfmode)
                {
                    p.level.ctfmode = true;
                    GameStart(p);
                }
                else
                {
                    p.level.ctfmode = false;
                    GameEnd(p);
                }
            }
            else
            {
                Help(p);
            }
        }
        

        public void GameStart(Player p)
        {
            foreach (Team team in p.level.teams)
            {
                team.ResetRound();
            }
            if (p.level.maxroundpoints == 0)
            {
                p.level.ChatLevel("Capture the flag game start! No max points set!");
            }
            else
            {
                p.level.ChatLevel("Capture the flag game start! Game goes to " + p.level.maxroundpoints + " point(s)!");
            }
        }

        public void GameEnd(Player p)
        {
            int currentpoints = 0;
            int maxPoints = 0;
            Team winTeam = new Team();
            for (int i = 0; i < p.level.teams.Count; i++)
            {
                currentpoints = p.level.teams[i].points;
                foreach (Player derpy in p.level.teams[i].onTeam)
                {
                    derpy.holdingFlag = false;
                }
                if (currentpoints > maxPoints)
                {
                    winTeam = p.level.teams[i];
                }
            }
            p.level.ctfmode = false;
            p.level.ChatLevel(p.color + p.prefix + p.name + Server.DefaultColor + " has ended the game!");
            if (maxPoints == 0)
            {
                p.level.ChatLevel("Nobody wins!");
            }
            else
            {
                p.level.ChatLevel(winTeam.teamname + " team " + Server.DefaultColor + " has won the game with " + maxPoints + " point(s)!");
            }
            foreach (Team team in p.level.teams)
            {
                team.EndRound();
            }
            
            
        }

        public void CtfInitTeam(Player p, string color)
        {
            Level workLevel = p.level;
            Team workTeam = new Team();
            char teamCol = (char)color[1];
            if (workLevel.teams.Find(team => team.color == teamCol) != null) { p.Message("That team already exists."); return; }
            workTeam.color = teamCol;
            workTeam.points = 0;
            workTeam.maxpoints = p.level.maxroundpoints;
            workTeam.mapOn = workLevel;
            char[] temp = c.Name("&" + teamCol).ToCharArray();
            temp[0] = char.ToUpper(temp[0]);
            string tempstring = new string(temp);
            workTeam.teamname = "&" + teamCol + tempstring;
            workLevel.teams.Add(workTeam);
            workLevel.ChatLevel(workTeam.teamname + " team " + Server.DefaultColor +"has been initialized!");
        }
        public void AddTeamMember(Player p, Player newPlayer, string color)
        {
            Level workLevel = p.level;
            char teamCol = (char)color[1];
            if (workLevel.teams.Exists(team => team.color == teamCol))
            {
                if (workLevel.teams.Find(team => team.color == teamCol).onTeam.Exists(player => player.name == newPlayer.name))
                {
                    p.Message("That player is already on that team.");
                    return;
                }
                else
                {
                    workLevel.teams.Find(team => team.color == teamCol).addPlayer(newPlayer);
                    newPlayer.onTeam = teamCol;
                    newPlayer.inCtf = true;
                }
            }
            else
            {
                p.Message("That team has not been initialized on this level.");
            }
            
        }
        public void RemoveTeamMember(Player p, Player newPlayer, string color)
        {
            Level worklevel = p.level;
            char teamCol = (char)color[1];
            if (worklevel.teams.Exists(team => team.color == teamCol))
            {
                if (worklevel.teams.Find(team => team.color == teamCol).onTeam.Exists(player => player.name == newPlayer.name))
                {
                    worklevel.teams.Find(team => team.color == teamCol).removePlayer(newPlayer);
                    newPlayer.onTeam = 'z';
                    newPlayer.inCtf = false;
                    return;
                }
                else
                {
                    p.Message("That player is not on that team.");
                }
            }
            else
            {
                p.Message("That team has not been initialized on this level.");
            }
        }
        public void AddFlagbase(Player p, string color, ushort x, ushort y, ushort z)
        {
            Level worklevel = p.level;
            char teamCol = (char)color[1];
            if (worklevel.teams.Exists(team => team.color == teamCol))
            {
                Team workTeam = worklevel.teams.Find(team => team.color == teamCol);
                Level.Flag workFlag = new Level.Flag();
                workTeam.flagBase[0] = x;
                workTeam.flagBase[1] = y;
                workTeam.flagBase[2] = z;
                workFlag.x = x;
                workFlag.y = y;
                workFlag.z = z;
                workFlag.team = workTeam;
                worklevel.flags.Add(workFlag);
                p.Message(workTeam.teamname + " team" +Server.DefaultColor +" flag has been set.");
                workTeam.flagishome = true;
                p.level.Blockchange(p, x, y, z, Block.flagbase);
                p.level.Blockchange(p, x, (ushort)(y + 1), z, Block.mushroom);
                p.level.Blockchange(x, (ushort)(y + 2), z, Team.GetColorBlock(teamCol));
            }
            else
            {
                p.Message("That team has not been initialized on this level.");
            }

        }
        public void AddTeamSpawn(Player p, string color)
        {
            Level worklevel = p.level;
            char teamCol = (char)color[1];
            if (worklevel.teams.Exists(team => team.color == teamCol))
            {
                Team workTeam = worklevel.teams.Find(team => team.color == teamCol);
                workTeam.spawn[0] = (ushort)(p.pos[0] / 32);
                workTeam.spawn[1] = (ushort)(p.pos[1] / 32);
                workTeam.spawn[2] = (ushort)(p.pos[2] / 32);
                workTeam.spawn[3] = (ushort)(p.rot[0]);
                workTeam.spawn[4] = 0;
                p.Message(workTeam.teamname + " team" + Server.DefaultColor + " spawn has been set.");
                workTeam.spawnset = true;
            }
            else
            {
                p.Message("That team has not been initialized on this level.");
            }
        }

        void AddFlag(Player p, ushort x, ushort y, ushort z, byte type)
        {
            CatchPos bp = (CatchPos)p.blockchangeObject;
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            p.ClearBlockchange();
            AddFlagbase(p, bp.color, x, y, z);
        }
 /*       void AddSpawn(Player p, ushort x, ushort y, ushort z, byte type)
        {
            CatchPos bp = (CatchPos)p.blockchangeObject;
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            p.ClearBlockchange();
            AddTeamSpawn(p, bp.color, x, y, z);
        }*//*

        public override void Help(Player p)
        {
            p.Message("Please visit http://forums.mclawl.tk and visit the Help and How-To section for a detailed");
            p.Message("help feature for CTF.  There are too many functions and required things to list here!");
        }
        
    }
}*/
