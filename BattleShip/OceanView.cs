using BattleShipServer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShipServer
{
    class OceanView
    {
        public List<Target>  Targets { get; set; }
        private List<string> Letters { get; set; }
        public Ship Carrier          { get; set; } = new Ship("Carrier");    //5
        public Ship BattleShip       { get; set; } = new Ship("BattleShip"); //4
        public Ship Destroyer        { get; set; } = new Ship("Destroyer");  //3
        public Ship Submarine        { get; set; } = new Ship("Submarine");  //3
        public Ship PatrolBoat       { get; set; } = new Ship("Patrol Boat");//2

        public OceanView()
        {
            Targets = new List<Target>();
            Letters = new List<string>
            {
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G",
                "H",
                "I",
                "J"
            };
            BuildGrid();

        }

        public bool AllShipsAreSunk()
        {
            List<Ship> list = new List<Ship>
            {
                Carrier,
                BattleShip,
                Destroyer,
                Submarine,
                PatrolBoat
            };

            return list.TrueForAll(s => s.IsSunk);
        }

        public void BuildGrid()
        {
            Targets.Add(new Target("A1", Carrier));
            Targets.Add(new Target("A2", Carrier));
            Targets.Add(new Target("A3", Carrier));
            Targets.Add(new Target("A4", Carrier));
            Targets.Add(new Target("A5", Carrier));
            Targets.Add(new Target("A6"));
            Targets.Add(new Target("A7"));
            Targets.Add(new Target("A8"));
            Targets.Add(new Target("A9"));
            Targets.Add(new Target("A10"));

            Targets.Add(new Target("B1", BattleShip));
            Targets.Add(new Target("B2", BattleShip));
            Targets.Add(new Target("B3", BattleShip));
            Targets.Add(new Target("B4", BattleShip));
            Targets.Add(new Target("B5"));
            Targets.Add(new Target("B6"));
            Targets.Add(new Target("B7"));
            Targets.Add(new Target("B8"));
            Targets.Add(new Target("B9"));
            Targets.Add(new Target("B10"));

            Targets.Add(new Target("C1", Destroyer));
            Targets.Add(new Target("C2", Destroyer));
            Targets.Add(new Target("C3", Destroyer));
            Targets.Add(new Target("C4"));
            Targets.Add(new Target("C5"));
            Targets.Add(new Target("C6"));
            Targets.Add(new Target("C7"));
            Targets.Add(new Target("C8"));
            Targets.Add(new Target("C9"));
            Targets.Add(new Target("C10"));

            Targets.Add(new Target("D1", Submarine));
            Targets.Add(new Target("D2", Submarine));
            Targets.Add(new Target("D3", Submarine));
            Targets.Add(new Target("D4"));
            Targets.Add(new Target("D5"));
            Targets.Add(new Target("D6"));
            Targets.Add(new Target("D7"));
            Targets.Add(new Target("D8"));
            Targets.Add(new Target("D9"));
            Targets.Add(new Target("D10"));

            Targets.Add(new Target("E1", PatrolBoat));
            Targets.Add(new Target("E2", PatrolBoat));
            Targets.Add(new Target("E3"));
            Targets.Add(new Target("E4"));
            Targets.Add(new Target("E5"));
            Targets.Add(new Target("E6"));
            Targets.Add(new Target("E7"));
            Targets.Add(new Target("E8"));
            Targets.Add(new Target("E9"));
            Targets.Add(new Target("E10"));

            Targets.Add(new Target("F1"));
            Targets.Add(new Target("F2"));
            Targets.Add(new Target("F3"));
            Targets.Add(new Target("F4"));
            Targets.Add(new Target("F5"));
            Targets.Add(new Target("F6"));
            Targets.Add(new Target("F7"));
            Targets.Add(new Target("F8"));
            Targets.Add(new Target("F9"));
            Targets.Add(new Target("F10"));

            Targets.Add(new Target("G1"));
            Targets.Add(new Target("G2"));
            Targets.Add(new Target("G3"));
            Targets.Add(new Target("G4"));
            Targets.Add(new Target("G5"));
            Targets.Add(new Target("G6"));
            Targets.Add(new Target("G7"));
            Targets.Add(new Target("G8"));
            Targets.Add(new Target("G9"));
            Targets.Add(new Target("G10"));

            Targets.Add(new Target("H1"));
            Targets.Add(new Target("H2"));
            Targets.Add(new Target("H3"));
            Targets.Add(new Target("H4"));
            Targets.Add(new Target("H5"));
            Targets.Add(new Target("H6"));
            Targets.Add(new Target("H7"));
            Targets.Add(new Target("H8"));
            Targets.Add(new Target("H9"));
            Targets.Add(new Target("H10"));

            Targets.Add(new Target("I1"));
            Targets.Add(new Target("I2"));
            Targets.Add(new Target("I3"));
            Targets.Add(new Target("I4"));
            Targets.Add(new Target("I5"));
            Targets.Add(new Target("I6"));
            Targets.Add(new Target("I7"));
            Targets.Add(new Target("I8"));
            Targets.Add(new Target("I9"));
            Targets.Add(new Target("I10"));

            Targets.Add(new Target("J1"));
            Targets.Add(new Target("J2"));
            Targets.Add(new Target("J3"));
            Targets.Add(new Target("J4"));
            Targets.Add(new Target("J5"));
            Targets.Add(new Target("J6"));
            Targets.Add(new Target("J7"));
            Targets.Add(new Target("J8"));
            Targets.Add(new Target("J9"));
            Targets.Add(new Target("J10"));

        }

        public void Print()
        {
            Console.WriteLine("\n                    ----- OCEAN VIEW -----");
            Console.WriteLine("                    (press 'Enter' to hide)");

            for (int i = 0; i < 10; i++)
            {

                if (i == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"     CARRIER       {Carrier.Points}/5");
                    if (Carrier.Points == 0)
                        Console.Write(" (DEAD)");
                }
                else if (i == 2)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"     BATTLESHIP    {BattleShip.Points}/4");
                    if (BattleShip.Points == 0)
                        Console.Write(" (DEAD)");
                }
                else if (i == 3)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"     DESTROYER     {Destroyer.Points}/3");
                    if (Destroyer.Points == 0)
                        Console.Write(" (DEAD)");
                }
                else if (i == 4)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"     SUBMARINE     {Submarine.Points}/3");
                    if (Submarine.Points == 0)
                        Console.Write(" (DEAD)");
                }
                else if (i == 5)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write($"     PATROL BOAT   {PatrolBoat.Points}/2");
                    if (PatrolBoat.Points == 0)
                        Console.Write(" (DEAD)");
                }
                else if (i == 6)
                {
                    Console.Write("     ");
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write($"M!");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" = Miss!");
                }

                Console.ResetColor();
                Console.WriteLine("\n--------------------------------");
                Console.Write("|");

               
                for (int j = 1; j < 11; j++)
                {
                    var position = Letters[i] + j;
                    var target = Targets.Find(t => t.GridPosition == position);

                    if (target.HasShip)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;

                        if      (target.Ship.Name == "CARRIER")
                        {
                            Console.BackgroundColor = ConsoleColor.Blue;
                        }
                        else if (target.Ship.Name == "BATTLESHIP")
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                        }
                        else if (target.Ship.Name == "DESTROYER")
                        {
                            Console.BackgroundColor = ConsoleColor.Yellow;
                        }
                        else if (target.Ship.Name == "SUBMARINE")
                        {
                            Console.BackgroundColor = ConsoleColor.Cyan;
                        }
                        else if (target.Ship.Name == "PATROL BOAT")
                        {
                            Console.BackgroundColor = ConsoleColor.Magenta;
                        }

                        if (target.IsAlreadyHit)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                        }
                        Console.Write(target.GridPosition);
                    }
                    else if (target.IsAlreadyHit)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write($"M!");
                    }
                    else
                    {
                        Console.Write(target.GridPosition);
                    }

                    Console.ResetColor();

                    Console.Write("|");
                }
            }
            Console.WriteLine("\n");
        }
    }
}
