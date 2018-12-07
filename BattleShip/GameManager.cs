using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BattleShipServer
{
    class GameManager
    {
        public string       Username  { get; set; }
        public string       Player2   { get; set; }
        public bool         IsHost    { get; set; }
        public string       Host      { get; set; }
        public int          Port      { get; set; }
        public OceanView    OceanView { get; set; } = new OceanView();

        private TcpListener listener;
        private TcpClient Client;
        private NetworkStream NetworkStream;
        private StreamReader reader;
        private StreamWriter writer;

        public void Initialize()
        {
            Console.WriteLine("Båtar är placerade...\n");
            Console.Write("Username: ");
            Username = Console.ReadLine();
            Console.Clear();
            Console.WriteLine($"Välkommen {Username}!");

            Console.WriteLine("Ange host (lämna tomt för att vara host): ");
            Host = Console.ReadLine();
            //TODO: Ta bort hotkey 1
            if (Host == "l") Host = "localhost";

            //Du är server.
            if (string.IsNullOrEmpty(Host))
            {
                IsHost = true;
                Console.Write("Ange port att lyssna på: ");
                Port = int.Parse(Console.ReadLine());
                //TODO: Ta bort hotkey 2
                if (Port == 5) Port = 5000;
                StartListen(Port);
            }
            //Du är klient.
            else
            {
                IsHost = false;
                Console.Write("Ange port: ");
                Port = int.Parse(Console.ReadLine());
                //TODO: Ta bort hotkey 3
                if (Port == 5) Port = 5000;

                Console.WriteLine($"Host: {Host}");
                Console.WriteLine($"Port: {Port}\n");
           
            }
            Connect();
            Play();
        }

        public void Connect()
        {
            if (IsHost)
            {
                while (true)
                {
                    Console.WriteLine("Väntar på att någon ska ansluta sig...");
                    Client = listener.AcceptTcpClient();
                    NetworkStream = Client.GetStream();
                    reader = new StreamReader(NetworkStream, Encoding.UTF8);
                    writer = new StreamWriter(NetworkStream, Encoding.UTF8) { AutoFlush = true };

                    if (Client.Connected)
                    {
                        Console.WriteLine($"Klient har anslutit sig {Client.Client.RemoteEndPoint}!");
                        writer.WriteLine($"210 BATTLESHIP/1.0");
                        WriteColor(true, $"210 BATTLESHIP/1.0");
                        var text1 = reader.ReadLine();
                        Player2 = text1.Split()[1];
                        WriteColor(false, text1);
                        writer.WriteLine($"220 {Username}");
                        WriteColor(true, $"220 {Username}");
                        var start = reader.ReadLine();
                        WriteColor(false, start);
                        if(start.ToUpper() == "START")
                        {
                            //TODO: random person startar.
                            writer.WriteLine($"221 You {Player2} will start.");
                            WriteColor(true, $"221 You {Player2} will start.");
                            break;
                        }
                       
                    }
                }

            }
            else
            {
                Client = new TcpClient(Host, Port);
                NetworkStream = Client.GetStream();
                reader = new StreamReader(NetworkStream, Encoding.UTF8);
                writer = new StreamWriter(NetworkStream, Encoding.UTF8) { AutoFlush = true };

                Console.WriteLine($"Ansluten till {Client.Client.RemoteEndPoint}");
                WriteColor(true, reader.ReadLine());
                writer.WriteLine($"HELLO {Username}");
                WriteColor(false, $"HELLO {Username}");
                var text1 = reader.ReadLine();
                Player2 = text1.Split()[1];
                WriteColor(true, text1);
                var start = Console.ReadLine().ToUpper();
                FixRow();
                WriteColor(false, start);
                writer.WriteLine(start);
                WriteColor(true, reader.ReadLine());
            }
        }

        public void Play()
        {
            Console.WriteLine("\n\n\n\t\t ----- BATTLESHIP BEGINS ----- \n");

            string hitStatus = "";
            string opponentHitStatus = "";
            string opponentCommand = "";

            while (true)
            {
                try
                {
                    if (IsHost)
                    {
                        opponentCommand = reader.ReadLine();
                        hitStatus = Read(opponentCommand);
                    }

                    while (Client.Connected)
                    {
                        Write(hitStatus);
                        opponentHitStatus = reader.ReadLine();
                        opponentCommand = reader.ReadLine();
                        hitStatus = Read(opponentCommand, opponentHitStatus);

                    }
                }
                catch (Exception)
                {

                    break;
                }

                DisposeAll();

            }
        }


        private string Read(string command, string opponentHitStatus = "")
        {
            string Answer = "";
            string com1 =   "";
            string targ =   "";

            if (opponentHitStatus.Split(" ")[0] == "270")
            {
                WriteColor(IsHost, "You won the game!");
            }

            if (string.Equals(command.Trim(), "QUIT", StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Quit
                if (IsHost)
                {
                    Client.Client.Disconnect(true);
                }
         
            }
            else if (string.Equals(command.Trim(), "HELP", StringComparison.InvariantCultureIgnoreCase))
            {
               // TODO: fixa hjälp.
                Answer = "NO HELP FOR YOU";
            }
            else if (string.Equals(command.Trim(), "DATE", StringComparison.InvariantCultureIgnoreCase))
            {
                Answer = DateTime.UtcNow.ToString("o");
            }

            try
            {
                com1 = command.Split(" ")[0];
                targ = command.Split(" ")[1];
            }
            catch (Exception)
            {
                Answer = "500 Syntax error - unknown command.";
                WriteColor(!IsHost, opponentHitStatus);
                WriteColor(!IsHost, $"{Player2}: {command.ToUpper()}");
                WriteColor(IsHost, Answer);
                return Answer;
            }

            if (string.Equals(com1, "FIRE", StringComparison.InvariantCultureIgnoreCase))
            {
                var target = OceanView.Targets.Where(t => t.GridPosition == targ.ToUpper()).FirstOrDefault();

                if(target != null)
                {
                    if (target.IsAlreadyHit)
                    {
                        Answer = $"501 - target already hit ({targ})";
                    } 

                    else if (target.HasShip)
                    {   //kollar om träffen sänker skeppet.
                        var isSunk = target.Ship.Hit();
                        target.IsAlreadyHit = true;

                        if (isSunk)
                        {
                            if (OceanView.AllShipsAreSunk())
                            {   //Om alla skepp har sjunkit
                                Answer = "260 You win!";
                            }
                            else
                            {   //om bara aktuellt skepp sjunker
                                Answer = target.Ship.SinkString;
                            }
                        }
                        else
                        {   //Normal träff
                            Answer = target.Ship.HitString;
                        }
                    }
                    else
                    {
                        Answer = "230 Miss!";
                        
                        target.IsAlreadyHit = true;
                    }
                }
                else
                {
                    Answer = $"501 - Out of grid. ({targ})";
                }
            }

            else
            {
                Answer = $"501 - unknown syntax";
            }

            if(opponentHitStatus != "")
            {
                //Skriver ut om man fick träff eller miss på sitt förra command mot motståndaren.
                WriteColor(!IsHost, opponentHitStatus);
            }
            //Skriver ut vad motståndaren gjorde för command.
            WriteColor(!IsHost, $"{Player2}: {command.ToUpper()}");

            //Skriver ut vad motståndarens command gjorde (hit eller miss).
            WriteColor(IsHost, Answer);
            return Answer;
        }

        private void Write(string hitStatus)
        {
            Console.Write("Send: ");
            var command = Console.ReadLine();
            if (command.ToUpper() == "QUIT" && IsHost)
            {
                writer.WriteLine("270 - You win.");
                Client.Client.Disconnect(true);
            }
            FixRow();
            WriteColor(IsHost, command);

            if (hitStatus != "")
            {
                //Skriver i kanalen om motståndaren fick en hit eller miss etc...
                writer.WriteLine(hitStatus);
            }
            //Skriver i kanalen vad man själv skrev för command.
            writer.WriteLine(command);
        }

        private void WriteColor(bool isHost, string text)
        {
            if (isHost)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(text);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }


        public void StartListen(int port)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"Lyssnar på port: {port}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Misslyckades att öppna socket. Troligtvis upptagen. {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void DisposeAll()
        {
            writer.Dispose();
            reader.Dispose();
            NetworkStream.Dispose();
            Client.Dispose();
        }

        private void FixRow()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop -1);
        }


    }
}
