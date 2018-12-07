using BattleShipServer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer
{
    class OceanView
    {
        public List<Target> Targets { get; set; }
        public Ship Carrier         { get; set; } = new Ship("Carrier");    //5
        public Ship BattleShip      { get; set; } = new Ship("BattleShip"); //4
        public Ship Destroyer       { get; set; } = new Ship("Destroyer");  //3
        public Ship Submarine       { get; set; } = new Ship("Submarine");  //3
        public Ship PatrolBoat      { get; set; } = new Ship("Patrol Boat");//2

        public OceanView()
        {
            Targets = new List<Target>();
            BuildGrid();
        }

        public void BuildGrid()
        {
            Targets.Add(new Target("A1", Submarine));
            Targets.Add(new Target("A2", Submarine));
            Targets.Add(new Target("A3", Submarine));
            Targets.Add(new Target("A4"));
            Targets.Add(new Target("A5"));
            Targets.Add(new Target("B1"));
            Targets.Add(new Target("B2"));
            Targets.Add(new Target("B3"));
            Targets.Add(new Target("B4", PatrolBoat));
            Targets.Add(new Target("B5", PatrolBoat));

        }
    }
}
