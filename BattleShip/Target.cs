using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer
{
    class Target
    {
        public string GridPosition { get; set; }
        public bool IsHit { get; set; }

        public Target()
        {

        }

        public Target(string position)
        {
            GridPosition = position;
        }
    }
}
