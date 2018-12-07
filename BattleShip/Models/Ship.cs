using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer.Models
{
    public class Ship
    {
        public string   Name     { get; set; }
        public int      Points   { get; set; }
        public bool     IsSunk   { get { return (Points == 0); } set { } }
        public string HitString  { get; set; }
        public string SinkString { get; set; }


        public Ship(string ship)
        {
            if (ship.ToUpper() == "CARRIER")
            {
                Name       = "CARRIER";
                Points     = 0;
                HitString  = "241 You hit my Carrier";
                SinkString = "251 You sunk my Carrier";
            }
            else if (ship.ToUpper() == "BATTLESHIP")
            {
                Name       = "BATTLESHIP";
                Points     = 0;
                HitString  = "242 You hit my Battleship";
                SinkString = "252 You sunk my Battleship";
            }
            else if (ship.ToUpper() == "DESTROYER")
            {
                Name       = "DESTROYER";
                Points     = 0;
                HitString  = "243 You hit my Destroyer";
                SinkString = "253 You sunk my Destroyer";
            }
            else if (ship.ToUpper() == "SUBMARINE")
            {
                Name       = "SUBMARINE";
                Points     = 3;
                HitString  = "244 You hit my Submarine";
                SinkString = "254 You sunk my Submarine";
            }
            else if (ship.ToUpper() == "PATROL BOAT")
            {
                Name       = "PATROL BOAT";
                Points     = 2;
                HitString  = "245 You hit my Patrol Boat";
                SinkString = "255 You sunk my Patrol Boat";
            }
            else
            {
                Name       = "SWIMMING MAN";
                Points     = 1;
                HitString  = "245 You hit my man!";
                SinkString = "255 You drowned my man";
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
