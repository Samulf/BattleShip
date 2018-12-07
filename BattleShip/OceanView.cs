using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer
{
    class OceanView
    {
        public List<Target> Targets = new List<Target>();
        public bool ShipsArePlaced { get; set; }

        public OceanView()
        {
            MockTargets();
        }

        public void MockTargets()
        {
            Targets.Add(new Target("A1"));
            Targets.Add(new Target("A2"));
            Targets.Add(new Target("A3"));
            ShipsArePlaced = true;
        }
    }
}
