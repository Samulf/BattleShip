using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            Console.WriteLine($"Välkommen {Username}!");

            Console.WriteLine("Ange host (lämna tomt för att vara host): ");
            Host = Console.ReadLine();

            //Du är server.
            if (string.IsNullOrEmpty(Host))
            {
                IsHost = true;
                Console.Write("Ange port att lyssna på: ");
                Port = int.Parse(Console.ReadLine());
                StartListen(Port);
            }
            //Du är klient.
            else
            {
                IsHost = false;
                Console.Write("Ange port: ");
                Port = int.Parse(Console.ReadLine());
                Console.WriteLine($"Host: {Host}");
                Console.WriteLine($"Port: {Port}\n");

                Console.WriteLine("ANSLUT?");
                Console.ReadLine();             
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
                WriteColor(false, start);
                writer.WriteLine(start);
                WriteColor(true, reader.ReadLine());
            }
        }

        public void Play()
        {
            Console.WriteLine("\n\n\n\t\t ----- BATTLESHIP BEGINS ----- \n");
            while (true)
            {
                string hitStatus = "";
                string opponentHitStatus = "";
                string opponentCommand = "";

                if (IsHost)
                {
                    opponentCommand = reader.ReadLine();
                    hitStatus       = Read(opponentCommand);
                }
                while (Client.Connected)
                {
                    Write(hitStatus);
                    opponentHitStatus = reader.ReadLine();
                    opponentCommand   = reader.ReadLine();
                    hitStatus = Read(opponentCommand);

                }


                //TODO: Dispose!!!

            }
        }


        private string Read(string command)
        {
            string Answer = "";

            WriteColor(!IsHost, $"{Player2}: {command}");

            if (string.Equals(command, "QUIT", StringComparison.InvariantCultureIgnoreCase))
            {
                Answer = "You want to quit.";
            }

            else if (string.Equals(command, "FIRE G6", StringComparison.InvariantCultureIgnoreCase))
            {
                Answer = "245 HIT. Patrol Boat";
            }
            else if (string.Equals(command, "DATE", StringComparison.InvariantCultureIgnoreCase))
            {
                Answer = DateTime.UtcNow.ToString("o");
            }
            else
            {
                Answer = "230 Miss!";
            }
            WriteColor(IsHost, Answer);
            return Answer;
        }

        private void Write(string hitStatus)
        {
            Console.Write("Send: ");
            var command = Console.ReadLine();
            FixRow();
            WriteColor(IsHost, command);
            if (hitStatus != "")
            {
                writer.WriteLine(hitStatus);
            }
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
                Console.ForegroundColor = ConsoleColor.Magenta;
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
                Console.WriteLine("Misslyckades att öppna socket. Troligtvis upptagen.");
                Environment.Exit(1);
            }
        }

        private void FixRow()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop -1);
        }


    }
}
