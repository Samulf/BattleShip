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
            Console.WriteLine("Spela?");
            Console.ReadKey();
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
                    if (Client.Connected)
                    {
                        Console.WriteLine($"Klient har anslutit sig {Client.Client.RemoteEndPoint}!");
                        break;
                    }
                }

            }
            else
            {
                Client = new TcpClient(Host, Port);
                Console.WriteLine($"Ansluten till {Client.Client.RemoteEndPoint}");
            }

            NetworkStream = Client.GetStream();
            reader = new StreamReader(NetworkStream, Encoding.UTF8);
            writer = new StreamWriter(NetworkStream, Encoding.UTF8) { AutoFlush = true };

        }

        public void Play()
        {
            while (true)
            {
                if (IsHost)
                {
                    Read();
                }
                while (Client.Connected)
                {
                    Write();
                    Read();

                }


                //Dispose!!!

            }
        }


        private void Read()
        {
            string P2 = IsHost ? "CLIENT" : "HOST";
            string Answer = "";

            var command = reader.ReadLine();
            WriteBlue($"{P2}: {command}");

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
