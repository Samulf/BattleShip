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
                        var text1 = reader.ReadLine();
                        Player2 = text1.Split()[1];
                        WriteBlue(text1);
                        writer.WriteLine($"220 {Username}");
                        var start = reader.ReadLine();
                        if(start.ToUpper() == "START")
                        {
                            //TODO: random person startar.
                            writer.WriteLine($"221 You {Player2} will start.");
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
                WriteGreen(reader.ReadLine());
                writer.WriteLine($"HELLO {Username}");
                var text1 = reader.ReadLine();
                Player2 = text1.Split()[1];
                WriteBlue(text1);
                var start = Console.ReadLine().ToUpper();
                writer.WriteLine(start);
            }
        }

        public void Play()
        {
            while (true)
            {
                if (IsHost)
                {
                    var opponentCommand = reader.ReadLine();
                    Read(opponentCommand);
                }
                while (Client.Connected)
                {
                    Write();
                    var opponentCommand = reader.ReadLine();
                    Read(opponentCommand);

                }


                //TODO: Dispose!!!

            }
        }


        private void Read(string command)
        {
            string Answer = "";

            //var command = reader.ReadLine();
            WriteBlue($"{Player2}: {command}");

            if (string.Equals(command, "QUIT", StringComparison.InvariantCultureIgnoreCase))
            {
                Answer = "You want to quit.";
            }

            else if (string.Equals(command, "fök", StringComparison.InvariantCultureIgnoreCase))
            {
                Answer = "FOKOFF!";
            }

            else if (string.Equals(command, "DATE", StringComparison.InvariantCultureIgnoreCase))
            {
                Answer = DateTime.UtcNow.ToString("o");
            }
            else
            {
                //writer.WriteLine("feke");
                //var pete = Console.ReadLine();
                //Console.Write("Skriv: ");
                //Answer = Console.ReadLine();
                Answer = "Miss!";
            }
            writer.WriteLine(Answer);
        }

        private void Write()
        {
            Console.Write("Send: ");
            Console.ForegroundColor = ConsoleColor.Green;
            var command = Console.ReadLine();
            //Console.WriteLine(command);
            writer.WriteLine(command);
            Console.ResetColor();
        }

        private void WriteBlue (string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        private void WriteGreen(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
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


    }
}
