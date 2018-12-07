using BattleShipServer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer
{
    class Target
    {
        public string GridPosition { get; set; }
        public bool   IsAlreadyHit { get; set; }
        public bool   HasShip      { get; set; }
        public Ship   Ship         { get; set; }

        public Target()
        {

        }

        public Target(string position, Ship ship)
        {
            GridPosition = position;
            HasShip      = true;
            Ship         = ship;
        }

        public Target(string position)
        {
            GridPosition = position;
        }
    }
}
