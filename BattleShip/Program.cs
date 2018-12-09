using BattleShipServer;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BattleShip
{
    class Program
    {
        static GameManager GameManager;

        static void Main(string[] args)
        {
            bool playing = true;
            GameManager = new GameManager();

            while (playing)
            {
                GameManager.Initialize();
                Console.Clear();
                Console.SetCursorPosition(Console.WindowWidth / 2, Console.WindowHeight / 2);
                Console.WriteLine("-- DISCONNECTED --");
                Console.SetCursorPosition(Console.WindowWidth / 2, (Console.WindowHeight / 2) +1);
                Console.WriteLine("CONNECT AGAIN? (Y/N)");

                var ans = Console.ReadLine();
                if (ans.ToUpper() == "Y")
                {
                    GameManager.Initialize();
                }
                else
                {
                    Console.Clear();
                    Console.SetCursorPosition(Console.WindowWidth / 2, Console.WindowHeight / 2);
                    Console.WriteLine("GOOD BYE");
                    Console.ReadKey();
                    playing = false;
                }
            }
           

        }
    }
}
