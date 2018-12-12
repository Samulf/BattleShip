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
            Console.SetWindowSize(100, 40);

            bool playing = true;
            GameManager = new GameManager();

            while (playing)
            {
                GameManager.Initialize();
                Console.WriteLine("\n\n      -- DISCONNECTED --");
                Console.WriteLine("     CONNECT AGAIN? (Y/N)");

                var ans = Console.ReadLine();
                Console.Clear();
                if (ans.ToUpper() == "Y")
                {
                    GameManager = new GameManager();
                }
                else
                {
                    Console.SetCursorPosition(Console.WindowWidth / 2, Console.WindowHeight / 2);
                    Console.WriteLine("GOOD BYE");
                    Console.ReadKey();
                    playing = false;
                }
            }
           

        }
    }
}
