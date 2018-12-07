using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer.Models
{
    public class Ship
    {
        public string   Name     { get; set; }
        public int      Points   { get; set; }
        public bool     IsSunk   { get; set; }
        public string HitString  { get; set; }
        public string SinkString { get; set; }


        public Ship(string shipType)
        {
            if (shipType.ToUpper() == "CARRIER")
            {
                Name       = "CARRIER";
                Points     = 5;
                HitString  = "241 You hit my Carrier";
                SinkString = "251 You sunk my Carrier";
            }
            if (shipType.ToUpper() == "BATTLESHIP")
            {
                Name       = "BATTLESHIP";
                Points     = 4;
                HitString  = "242 You hit my Battleship";
                SinkString = "252 You sunk my Battleship";
            }
            if (shipType.ToUpper() == "DESTROYER")
            {
                Name       = "DESTROYER";
                Points     = 3;
                HitString  = "243 You hit my Destroyer";
                SinkString = "253 You sunk my Destroyer";
            }
            if (shipType.ToUpper() == "SUBMARINE")
            {
                Name       = "SUBMARINE";
                Points     = 3;
                HitString  = "244 You hit my Submarine";
                SinkString = "254 You sunk my Submarine";
            }
            if (shipType.ToUpper() == "PATROL BOAT")
            {
                Name       = "PATROL BOAT";
                Points     = 2;
                HitString  = "245 You hit my Patrol Boat";
                SinkString = "255 You sunk my Patrol Boat";
            }
            else
            {
                Name       = "PATROL BOAT";
                Points     = 2;
                HitString  = "245 You hit my Patrol Boat";
                SinkString = "255 You sunk my Patrol Boat";
            }
        }

        public bool Hit()
        {
            Points -= 1;
            if (Points == 0)
            {
                IsSunk = true;
            }
            return IsSunk;
        }
    }
}
