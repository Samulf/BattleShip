﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BattleShipServer
{
    public class ResponseCodes
    {
        public ResponseCode BattleShip       { get; set; }
        public ResponseCode RemotePlayerName { get; set; }
        public ResponseCode ClientStarts     { get; set; }
        public ResponseCode HostStarts       { get; set; }
        public ResponseCode Miss             { get; set; }
        public ResponseCode HitCarrier       { get; set; }
        public ResponseCode HitBattleShip    { get; set; }
        public ResponseCode HitDestroyer     { get; set; }
        public ResponseCode HitSubmarine     { get; set; }
        public ResponseCode HitPatrolBoat    { get; set; }
        public ResponseCode SunkCarrier      { get; set; }
        public ResponseCode SunkBattleShip   { get; set; }
        public ResponseCode SunkDestroyer    { get; set; }
        public ResponseCode SunkSubmarine    { get; set; }
        public ResponseCode SunkPatrolBoat   { get; set; }
        public ResponseCode YouWin           { get; set; }
        public ResponseCode ConnectionClosed { get; set; }
        public ResponseCode SyntaxError      { get; set; }
        public ResponseCode SequenceError    { get; set; }

        public ResponseCodes()
        {
            BattleShip       = new ResponseCode("210", "210 BATTLESHIP/1.0");
            RemotePlayerName = new ResponseCode("220");
            ClientStarts     = new ResponseCode("221", "221 Client Starts");
            HostStarts       = new ResponseCode("222", "222 Host Starts");
            Miss             = new ResponseCode("230", "230 Miss!");
            HitCarrier       = new ResponseCode("241", "241 You hit my Carrier");
            HitBattleShip    = new ResponseCode("242", "242 You hit my Battleship");
            HitDestroyer     = new ResponseCode("243", "243 You hit my Destroyer");
            HitSubmarine     = new ResponseCode("244", "244 You hit my Submarine");
            HitPatrolBoat    = new ResponseCode("245", "245 You hit my Patrol Boat");
            SunkCarrier      = new ResponseCode("251", "251 You sunk my Carrier");
            SunkBattleShip   = new ResponseCode("252", "252 You sunk my Battleship");
            SunkDestroyer    = new ResponseCode("253", "253 You sunk my Destroyer");
            SunkSubmarine    = new ResponseCode("254", "254 You sunk my Submarine");
            SunkPatrolBoat   = new ResponseCode("255", "255 You sunk my Patrol Boat");
            YouWin           = new ResponseCode("260", "260 You win!");
            ConnectionClosed = new ResponseCode("270", "270 Connection closed");
            SyntaxError      = new ResponseCode("500", "500 Syntax error");
            SequenceError    = new ResponseCode("501", "501 Sequence error");
        }

        public string GetReponseCodeByCode(string code)
        {
            if (code == "230") return Miss.FullString;
            else if (code == "241") return HitCarrier.FullString;
            else if (code == "242") return HitBattleShip.FullString;
            else if (code == "243") return HitDestroyer.FullString;
            else if (code == "244") return HitSubmarine.FullString;
            else if (code == "245") return HitPatrolBoat.FullString;
            else if (code == "251") return SunkCarrier.FullString;
            else if (code == "252") return SunkBattleShip.FullString;
            else if (code == "253") return SunkDestroyer.FullString;
            else if (code == "254") return SunkSubmarine.FullString;
            else if (code == "255") return SunkPatrolBoat.FullString;
            else if (code == "260") return YouWin.FullString;
            else if (code == "270") return ConnectionClosed.FullString;
            else if (code == "501") return SequenceError.FullString;
            else if (code == "500") return SyntaxError.FullString;
            else return "fock";
        }

    }
}
