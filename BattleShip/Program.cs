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
        //--------------------------------------- SERVER ---------------------------------------
        //static TcpListener listener;
        static GameManager GameManager;

        static void Main(string[] args)
        {
            GameManager = new GameManager();
            GameManager.Initialize();


            //Console.WriteLine("----- SERVER 1 -----\n");
            //Console.WriteLine("Välkommen till servern");

            //Console.WriteLine("Ange host (lämna tomt för att vara host): ");
            //var host = Console.ReadLine();
            //int port;

            ////Du är server.
            //if (string.IsNullOrEmpty(host))
            //{
            //    Console.Write("Ange port att lyssna på: ");
            //    port = int.Parse(Console.ReadLine());
            //    StartListen(port);
            //    PlayAsHost();
            //}
            // //Du är klient.
            //else
            //{
            //    Console.Write("Ange port: ");
            //    port = int.Parse(Console.ReadLine());
            //    Console.WriteLine($"Host: {host}");
            //    Console.WriteLine($"Port: {port}\n");
            //    Console.WriteLine("ANSLUT?");
            //    Console.ReadLine();
            //    PlayAsClient(host, port);

            //}

  

           

        }

        //static void PlayAsHost()
        //{
        //    while (true)
        //    {
        //        Console.WriteLine("Väntar på att någon ska ansluta sig...");

        //        using (var client = listener.AcceptTcpClient())
        //        using (var networkStream = client.GetStream())
        //        using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
        //        using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
        //        {
        //            Console.WriteLine($"Klient har anslutit sig {client.Client.RemoteEndPoint}!");

        //            while (client.Connected)
        //            {
        //                var command = reader.ReadLine();
        //                Console.WriteLine($"CLIENT: {command}");

        //                if (string.Equals(command, "QUIT", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    writer.WriteLine("BYE BYE");
        //                    break;
        //                }

        //                else if (string.Equals(command, "fök", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    writer.WriteLine("FOKOFF!");
        //                }

        //                else if (string.Equals(command, "DATE", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    writer.WriteLine(DateTime.UtcNow.ToString("o"));
        //                }
        //                else
        //                {
        //                    //writer.WriteLine("feke");
        //                    //var pete = Console.ReadLine();
        //                    Console.Write("Skriv: ");
        //                    var svar = Console.ReadLine();
        //                    writer.WriteLine(svar);

        //                }

        //                //writer.WriteLine($"UNKNOWN COMMAND: {command}");
        //            }
        //        }

        //    }
        //}

        //static void PlayAsClient(string host, int port)
        //{
        //    using (var client = new TcpClient(host, port))
        //    using (var networkStream = client.GetStream())
        //    using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
        //    using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
        //    {
        //        Console.WriteLine($"Ansluten till {client.Client.RemoteEndPoint}");
        //        Console.WriteLine("(QUIT för att avsluta)");
        //        while (client.Connected)
        //        {
        //            Console.Write("Skriv: ");
        //            var text = Console.ReadLine();

        //            if (text == "EXIT") break;

        //            // Skicka text
        //            writer.WriteLine(text);

        //            if (!client.Connected) break;

        //            // Läs minst en rad
        //            do
        //            {
        //                var line = reader.ReadLine();
        //                Console.WriteLine("SERVER: " + line);

        //            } while (networkStream.DataAvailable);

        //        };

        //    }
        //}

        //static void StartListen(int port, string ip = "127.0.0.1")
        //{
        //    try
        //    {
        //        var ipAny = IPAddress.Any;
        //        var parsedIP = IPAddress.Parse(ip);
        //        listener = new TcpListener(ipAny, port);
        //        listener.Start();
        //        Console.WriteLine($"Lyssnar på port: {port}");
        //    }
        //    catch (SocketException ex)
        //    {
        //        Console.WriteLine("Misslyckades att öppna socket. Troligtvis upptagen.");
        //        Environment.Exit(1);
        //    }
        //}

    }
}
