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
        public string        Username   { get; set; }
        public string        Player2    { get; set; }
        public bool          IsHost     { get; set; }
        public bool          HostStarts { get; set; }
        public string        Host       { get; set; }
        public int           Port       { get; set; }
        public ResponseCodes RCodes     { get; set; } = new ResponseCodes();
        public OceanView     OceanView  { get; set; } = new OceanView();

        private readonly Random rnd = new Random();
        private TcpListener listener;
        private TcpClient Client;
        private NetworkStream NetworkStream;
        private StreamReader reader;
        private StreamWriter writer;

        public void Initialize()
        {
            PrintIntro();
            
            
            MiddleWL("Ships are placed...\n", true);
            MiddleWL("Username: ", false, true, 8);
            Username = Console.ReadLine();

            //if (Username.ToUpper() == "OCEAN" || Username.ToUpper() == "O")
            //{
            //    ShowOceanView();
            //}

            Console.Clear();
            MiddleWL($"Välkommen {Username}!", true);

            MiddleWL("Ange host (lämna tomt för att vara host): ", false, true);
            Host = Console.ReadLine();
            //TODO: Ta bort hotkey 1
            if (Host == "l") Host = "localhost";

            //Du är server.
            if (string.IsNullOrEmpty(Host))
            {
                IsHost = true;
                MiddleWL("Ange port att lyssna på: ",false, true);
                Port = int.Parse(Console.ReadLine());
                //TODO: Ta bort hotkey 2
                if (Port == 5) Port = 5000;
                StartListen(Port);
            }
            //Du är klient.
            else
            {
                IsHost = false;
                MiddleWL("Ange port: ", false, true);
                Port = int.Parse(Console.ReadLine());
                //TODO: Ta bort hotkey 3
                if (Port == 5) Port = 5000;

                MiddleWL($"Host: {Host}");
                MiddleWL($"Port: {Port}");
           
            }
            Connect();
            Play();
            Console.SetCursorPosition(Console.WindowWidth / 2, Console.CursorTop +1);
            Console.WriteLine("-- DISCONNECTED --");
            Console.ReadKey();

        }

        public void Connect()
        {
            if (IsHost)
            {
                while (true)
                {
                    MiddleWL("Väntar på att någon ska ansluta sig...");
                    Client = listener.AcceptTcpClient();
                    NetworkStream = Client.GetStream();
                    reader = new StreamReader(NetworkStream, Encoding.UTF8);
                    writer = new StreamWriter(NetworkStream, Encoding.UTF8) { AutoFlush = true };

                    if (Client.Connected)
                    {
                        Console.Clear();
                        Console.WriteLine($"Klient har anslutit sig {Client.Client.RemoteEndPoint}!");
                        writer.WriteLine(RCodes.BattleShip.FullString);
                        WriteColor(true, RCodes.BattleShip.FullString);

                        while (true)
                        {
                            var text1 = reader.ReadLine();
                            WriteColor(false, text1);

                            try
                            {
                                var part1 = text1.Split()[0];

                                if (part1.ToUpper() == "QUIT")
                                {
                                    //TODO: Quit
                                    break;
                                }
                                else if (part1.ToUpper() == "HELLO" || part1.ToUpper() == "HELO")
                                {
                                    var part2 = text1.Split()[1];
                                    Player2 = part2;
                                    writer.WriteLine($"{RCodes.RemotePlayerName.Code} {Username}");
                                    WriteColor(true, $"{RCodes.RemotePlayerName.Code} {Username}");
                                    break;
                                }
                                else
                                {
                                    writer.Write(RCodes.SequenceError.FullString);
                                    WriteColor(true, RCodes.SequenceError.FullString);
                                }
                            }
                            catch (Exception)
                            {
                                writer.Write(RCodes.SequenceError.FullString);
                                WriteColor(true, RCodes.SequenceError.FullString);
                            }
                        }

                        while (true)
                        {
                            var start = reader.ReadLine();
                            WriteColor(false, start);
                            if (start.ToUpper() == "START")
                            {
                                int randomstart = rnd.Next(2);

                                if(randomstart == 0)
                                {
                                    writer.WriteLine($"{RCodes.ClientStarts.Code} You {Player2} will start.");
                                    WriteColor(true, $"{RCodes.ClientStarts.Code} {Player2} will start.");
                                    HostStarts = false;
                                }
                                else
                                {
                                    writer.WriteLine($"{RCodes.HostStarts.Code} Host {Username} will start.");
                                    WriteColor(true, $"{RCodes.HostStarts.Code} You {Username} will start.");
                                    HostStarts = true;
                                }
                                break;
                            }
                            else
                            {
                                writer.Write(RCodes.SequenceError.FullString);
                                WriteColor(true, RCodes.SequenceError.FullString);
                            }
                        }
                        break;
                    }
                }

            }
            //Om man är klient
            else
            {
                Client = new TcpClient(Host, Port);
                NetworkStream = Client.GetStream();
                reader = new StreamReader(NetworkStream, Encoding.UTF8);
                writer = new StreamWriter(NetworkStream, Encoding.UTF8) { AutoFlush = true };

                Console.Clear();
                Console.WriteLine($"Ansluten till {Client.Client.RemoteEndPoint}");
                WriteColor(true, reader.ReadLine());

                writer.WriteLine($"HELLO {Username}");
                WriteColor(false, $"HELLO {Username}");

                try
                {
                    var text1 = reader.ReadLine();
                    Player2 = text1.Split()[1];
                    WriteColor(true, text1);
                }
                catch (Exception)
                {

                    Player2 = "(Host)";
                    WriteColor(true, "220 " +Player2);
                }

               while (true)
                {
                    var start = Console.ReadLine().ToUpper();
                    FixRow();
                    WriteColor(false, start);
                    writer.WriteLine(start);
                    var whoStarts = reader.ReadLine();
                    WriteColor(false, whoStarts);
                    try
                    {
                        var code = whoStarts.Split(" ")[0];
                        if (code == RCodes.HostStarts.Code)
                        {
                            //TODO: Host startar
                            HostStarts = true;
                            break;
                        }
                        if (code == RCodes.ClientStarts.Code)
                        {
                            //TODO: Client startar
                            HostStarts = false;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("What??");
                        throw;
                    }
                    WriteColor(true, whoStarts);
                }
             
            }
        }

        public void Play()
        {
            Console.WriteLine("\n\n\n");
            Console.WriteLine("-- BATTLESHIP BEGINS --\n");

            string hitStatus = "";
            string opponentHitStatus = "";
            string opponentCommand = "";
            bool arePlaying = true;

            while (arePlaying)
            {
                try
                {
                    //Vem som ska börja LÄSA command från motståndaren.
                    if ((IsHost && !HostStarts) || (!IsHost && HostStarts))
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

                        if (hitStatus == "270")
                        {
                            arePlaying = false;
                            break;                            
                        }

                    }
                }
                catch (Exception)
                {
                    arePlaying = false;
                }

                DisposeAll();

            }
        }

        private string Read(string command, string opponentHitStatus = "")
        {
            string Answer = "";
            string com1 =   "";
            string targ =   "";

            try
            {
                com1 = command.Split(" ")[0];
                targ = command.Split(" ")[1];
            }
            catch (Exception)
            {}

            if (opponentHitStatus.Split(" ")[0] == "270")
            {
                WriteColor(IsHost, RCodes.YouWin.FullString);
                return "270";
            }

            if (string.Equals(command.Trim(), "QUIT", StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Quit
                if (IsHost)
                {
                    WriteColor(!IsHost, "The client wants to quit. (press any key to continue)");
                    Console.ReadKey();
                    Client.Client.Disconnect(true);
                    WriteColor(IsHost, RCodes.YouWin.FullString);
                    return "270";
                }
                //TODO: be servern att avsluta själv
                else
                {
                    WriteColor(IsHost, RCodes.YouWin.FullString);
                    return "270";
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
            else if (string.Equals(com1, "FIRE", StringComparison.InvariantCultureIgnoreCase))
            {
                if (com1 !=  "" && targ != "")
                {
                    var target = OceanView.Targets.Where(t => t.GridPosition == targ.ToUpper()).FirstOrDefault();

                    if (target != null)
                    {
                        if (target.IsAlreadyHit)
                        {
                            Answer = $"{RCodes.SequenceError.FullString} - target already hit ({targ})";
                        }

                        else if (target.HasShip)
                        {   //kollar om träffen sänker skeppet.
                            var isSunk = target.Ship.Hit();
                            target.IsAlreadyHit = true;

                            if (isSunk)
                            {
                                if (OceanView.AllShipsAreSunk())
                                {   //Om alla skepp har sjunkit
                                    Answer = RCodes.YouWin.FullString;
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
                            Answer = RCodes.Miss.FullString;
                            target.IsAlreadyHit = true;
                        }
                    }
                    else
                    {
                        Answer = $"{RCodes.SequenceError.FullString} - Out of grid. ({targ})";
                    }
                }
                
            }

            else
            {
                Answer = $"{RCodes.SyntaxError.FullString}";
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
            string command;

            while (true)
            {
                Console.Write("Send: ");
                command = Console.ReadLine();

                if (command.ToUpper() == "OCEAN")
                {
                    ShowOceanView();
                }
                else if (command.ToUpper() == "RADAR")
                {
                    Console.WriteLine("[PRINTING RADAR VIEW]");
                }
                else if (command.ToUpper() == "HELP")
                {
                    Console.WriteLine("[PRINTING HELP]");
                }
                else if (command.ToUpper() == "QUIT")
                {
                    Console.WriteLine("[QUIT GAME]");
                }
                else
                {
                    break;
                }
            }
           
            if (command.ToUpper() == "QUIT" && IsHost)
            {
                writer.WriteLine("270 - You win.");
                Client.Client.Disconnect(true);
            }
            FixRow();
            WriteColor(IsHost, command.ToUpper());

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
                MiddleWL($"Lyssnar på port: {port}");
            }
            catch (SocketException ex)
            {
                MiddleWL($"Misslyckades att öppna socket. Troligtvis upptagen. {ex.Message}");
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
                Console.SetCursorPosition(0, Console.CursorTop);
        }

        private void ShowOceanView()
        {
            OceanView.Print();
            Console.ReadKey();

            for (int i = 0; i < 26; i++)
            {
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

        }

        public void PrintIntro()
        {
            Console.WriteLine("\n\n\n\n");
            Console.WriteLine("                                          |__");
            Console.WriteLine("                                          |\\/");
            Console.WriteLine("                                          ---");
            Console.WriteLine("                                          / | [");
            Console.WriteLine("                                   !      | |||");
            Console.WriteLine("                                 _/|     _/|-++'");
            Console.WriteLine("                             +  +--|    |--|--|_ |-");
            Console.WriteLine("                          { /|__|  |/\\__|  |--- |||__/");
            Console.WriteLine("                         +---------------___[}-_===_.'____               /\\");
            Console.WriteLine("                     ____`-' ||___-{]_| _[}-  |     |_[___\\==--          \\/   _");
            Console.WriteLine("      __..._____--==/___]_|__|_____________________________[___\\==--___,-----' .7");
            Console.WriteLine("     |                                                                   BS-18/");
            Console.WriteLine("      \\_______________________________________________________________________|");
            Console.WriteLine("\n\n                                     BATTLESHIP 1.0");

            Console.ReadKey();
            Console.Clear();
        }

        public void MiddleWL(string offset, bool isFirst = false, bool OnlyWrite = false, int extraOffset = 0)
        {
            int o = offset.Length / 2 + extraOffset;
            if (isFirst)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - o, Console.WindowHeight / 3);
            }
            else
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - o, Console.CursorTop);
            }
            if (OnlyWrite)
            {
                Console.Write(offset);
            }
            else
            {
                Console.WriteLine(offset);
            }
           
        }
    }
}
