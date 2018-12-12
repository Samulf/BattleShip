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
        public OceanView     Radar      { get; set; } = new OceanView(true);

        private readonly Random rnd = new Random();
        private TcpListener listener;
        private TcpClient Client;
        private NetworkStream NetworkStream;
        private StreamReader reader;
        private StreamWriter writer;
        private int PortChangeCount = 0;

        public void Initialize()
        {
            PrintIntro();

            MiddleWL("Ships are placed...\n", true);
            MiddleWL("Username: ", false, true, 8);
            Username = Console.ReadLine();
            //if (Username == "h") ShowHelp();
            //Console.ReadKey();
            Console.Clear();
            MiddleWL($"Välkommen {Username}!", true);

            MiddleWL("Ange host (lämna tomt för att vara host): ", false, true);
            Host = Console.ReadLine();
            //TODO: Ta bort hotkey 1
            if (Host == "l") Host = "localhost";

            //Du är host.
            if (string.IsNullOrEmpty(Host))
            {
                IsHost = true;
                bool portParsed;
                do
                {
                    MiddleWL("Ange port att lyssna på: ", false, true);
                    portParsed = int.TryParse(Console.ReadLine(), out int tempPort);
                    if (!portParsed)
                    {
                        MiddleWL("Felaktigt portnummer!");
                    }
                    else
                    {
                        //TODO: Ta bort hotkey 2
                        if (tempPort == 5) Port = 5000;
                        else Port = tempPort;
                    }
                } while (!portParsed);
               
                StartListen(Port);
                MiddleWL($"Listening on port: {Port}");
            }
            //Du är klient.
            else
            {
                IsHost = false;
                bool portParsed;
                do
                {
                    MiddleWL("Ange port att lyssna på: ", false, true);
                    portParsed = int.TryParse(Console.ReadLine(), out int tempPort);
                    if (!portParsed)
                    {
                        MiddleWL("Felaktigt portnummer!");
                    }
                    else
                    {
                        //TODO: Ta bort hotkey 2
                        if (tempPort == 5) Port = 5000;
                        else Port = tempPort;
                    }
                } while (!portParsed);

                MiddleWL($"Host: {Host}");
                MiddleWL($"Port: {Port}");
           
            }
            try
            {
                Connect();
                Play();
            }
            catch(IOException e)
            {
                var ex = e;
                DisposeAll();
            }
    
            Console.SetCursorPosition(Console.WindowWidth / 2, Console.CursorTop +1);
            //Console.WriteLine("-- DISCONNECTED --");
            //Console.ReadKey();

        }

        public void Connect()
        {
            if (IsHost)
            {
                while (true)
                {
                    MiddleWL("Väntar på att någon ska ansluta sig...");
                    Client = listener.AcceptTcpClient();
                    Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
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
                //TODO: om host refusar porten (felsäkring)
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
                    Console.WriteLine("(Write 'START' to start the game)");
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
            string myLastCommand = "";

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
                        //TODO: fel!?
                        myLastCommand = Write();
                        if (myLastCommand == "QUIT")
                        {
                            if (!IsHost)
                            {
                               WriteColor(true, reader.ReadLine());
                            }
                            arePlaying = false;
                            break;
                        }
                        else
                        {
                            opponentHitStatus = reader.ReadLine();
                            WriteColor(!IsHost, opponentHitStatus);
                            opponentCommand = reader.ReadLine();
                            hitStatus = Read(opponentCommand, opponentHitStatus, myLastCommand);

                            if (hitStatus == "270")
                            {
                                arePlaying = false;
                                break;
                            }
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

        private string Read(string command, string opponentHitStatus = "", string myLastCom = "")
        {
            string Answer = "";
            string com1 =   "";
            string targ =   "";
            string mylastcomPosition = "";

            try
            {
                com1 = command.Split(" ")[0];
                targ = command.Split(" ")[1];
                mylastcomPosition = myLastCom.Split(" ")[1];
            }
            catch (Exception)
            {}

            if(mylastcomPosition != "" && opponentHitStatus != "")
            {
                SetEnemyRadarLastHit(mylastcomPosition.ToUpper(), opponentHitStatus);
            }

            if (opponentHitStatus.Split(" ")[0] == "270" || com1 == "270")
            {
                WriteColor(IsHost, RCodes.ConnectionClosed.FullString);
                return "270";
            }

            if (string.Equals(command.Trim(), "QUIT", StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Quit
                if (IsHost)
                {
                    WriteColor(!IsHost, "The client wants to quit. (press any key to continue)");
                    Console.ReadKey();
                    writer.WriteLine(RCodes.ConnectionClosed.FullString + " - You have quit the game.");
                    Client.Client.Disconnect(true);
                    WriteColor(IsHost, RCodes.YouWin.FullString + " - Client has disconnected.");
                    return "270";
                }
                //TODO: be servern att avsluta själv
                else
                {
                    WriteColor(IsHost, RCodes.YouWin.FullString);
                    Answer = RCodes.SyntaxError.FullString + " (Host should quit, not client)";
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

            //if(opponentHitStatus != "")
            //{
            //    //Skriver ut om man fick träff eller miss på sitt förra command mot motståndaren.
            //    WriteColor(!IsHost, opponentHitStatus);
            //}
            //Skriver ut vad motståndaren gjorde för command.
            WriteColor(!IsHost, $"{Player2}: {command.ToUpper()}");

            //Skriver ut vad motståndarens command gjorde (hit eller miss).
            WriteColor(IsHost, Answer);
            writer.WriteLine(Answer);
            return Answer;
        }

        private string Write()
        {
            string command = ReadCommandConsole().ToUpper();
           
            if (command == "QUIT" && IsHost)
            {
                writer.WriteLine(RCodes.ConnectionClosed.FullString + " - Host has quit the game");
                Console.Clear();
                MiddleWL("YOU GAVE UP...", true);
                Console.ReadKey();
                Client.Client.Disconnect(true);
            }
            else
            {
                FixRow();
                WriteColor(IsHost, command);
                //Skriver i kanalen vad man själv skrev för command.
                writer.WriteLine(command);
                //WriteColor(true, reader.ReadLine());
            }        

            return command;
        }

        private void WriteColor(bool isHost, string text)
        {
            if (isHost)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            Console.WriteLine(text);
            Console.ResetColor();
        }

        private string ReadCommandConsole()
        {
            string command = "";

            bool isOtherCommand = true;
            while (isOtherCommand)
            {
                Console.Write("Send: ");
                command = Console.ReadLine();
                command = command.ToUpper().Trim();

                if (command == "" || string.IsNullOrEmpty(command))
                {
                    //TODO: detta kan bli fel (om man lämnar commanden tom).
                    FixRow();
                    Console.WriteLine("[Command was empty (write 'HELP' for available commands)]");
                }
                else if (command == "RADAR" || command == "R" || command == "SHOWRADAR" || command == "RADARVIEW")
                {
                    ShowRadar();
                }
                else if (command == "OCEAN" || command == "O" || command == "SHOWOCEAN" || command == "OCEANVIEW")
                {
                    ShowOceanView();
                }
                else if (command == "HELP" || command == "H" || command == "COMMANDS")
                {
                    ShowHelp();
                }
                else if (command == "QUIT" || command == "Q" || command == "EXIT")
                {
                    Console.WriteLine("QUIT THE GAMEMOOOO");
                    isOtherCommand = false;
                }
                else
                {
                    isOtherCommand = false;
                }
            }
            return command;
        }

        public void StartListen(int port)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
            }
            catch (SocketException ex)
            {
                PortChangeCount ++;
                MiddleWL($"Failed to open socket. Probably in use.");
                MiddleWL(ex.Message);
                if(PortChangeCount > 200)
                {
                    Environment.Exit(1);
                }
                else
                {
                    StartListen(port + 1);
                    Port = port + 1;
                    MiddleWL($"Port changed to {Port}");
                }

            }
        }

        private void DisposeAll()
        {
            writer.Dispose();
            reader.Dispose();
            NetworkStream.Dispose();

            Socket socket = Client.Client;
            if (Client.Connected)
            {
                if (socket.Connected)
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(true);
                }
                Client.Dispose();
            }
            //Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);            
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

        private void ShowRadar()
        {
            Radar.Print();
            Console.ReadKey();

            for (int i = 0; i < 26; i++)
            {
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

        }

        private void ShowHelp()
        {
            Console.WriteLine();
            MiddleWL("----- HELP -----");
            MiddleWL("(press 'Enter' to hide)");
            Console.WriteLine();
            HelpWL("BATTLESHIP/1.0");
            HelpWL("This is a classic BattleShip game, you connect with another player,");
            HelpWL("and you take turns to fire.");
            HelpWL("This game is not case sensitive");
            HelpWL("(it doesn't matter if you write 'HELP' or 'help' - or even 'hElP').");
            HelpWL("After you have got a connection with another player");
            HelpWL("the CLIENT have to say 'START' to start the game.");
            HelpWL("So if you are the HOST you'll have to wait for the client to start the game.");
            Console.WriteLine();
            HelpWL("COMMANDS:");
            Console.WriteLine();
            HelpWL("HELLO <username>  - This is the first command a client have to use when connected.");
            HelpWL("START             - This is how the client starts the game after you have connected.");
            HelpWL("FIRE <coordinate> - When the game has started, this is the command to fire at the enemy ship.");
            HelpWL("                    <coordinate> have to be replaced by something between A1 to J10...");
            HelpWL("                    ...example: 'FIRE C6'. Optional message after the coordinate can be applied.");
            HelpWL("OCEAN             - Shows the Ocean View. This is your ships on a map.");
            HelpWL("RADAR             - Shows where you have hit/missed previous shots to the enemy ships");
            HelpWL("HELP              - Shows help. Obviously you found it...");
            HelpWL("QUIT              - Quits the game and disconnects from the other player.");

            Console.ReadKey();

            for (int i = 0; i < 25; i++)
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
            int newWidth = Console.WindowWidth / 2 - o;

            if(newWidth >= 0)
            {
                if (isFirst)
                {
                    Console.SetCursorPosition(newWidth, Console.WindowHeight / 3);
                }
                else
                {
                    Console.SetCursorPosition(newWidth, Console.CursorTop);
                }
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

        public void HelpWL(string text)
        {
            Console.SetCursorPosition(6, Console.CursorTop);
            Console.WriteLine(text);
        }

        private void SetEnemyRadarLastHit(string myLastComPosition, string opHitStatus)
        {
            //TODO: Ifall andra spelet skickar konstig hitstatus.
            var statCode = opHitStatus.Split(" ")[0];
            var statCode2First = statCode.Substring(0, 2);
            var statCode3 = statCode.Substring(2, 1);
            //Om det är miss:
            if (statCode == "230")
            {
                var target = Radar.Targets.Where(t => t.GridPosition == myLastComPosition).FirstOrDefault();
                target.IsAlreadyHit = true;
            }
            //Om det är träff på något skepp:
            else if (statCode2First == "24")
            {
                var target = Radar.Targets.Where(t => t.GridPosition == myLastComPosition).FirstOrDefault();
                target.IsAlreadyHit = true;
                target.HasShip      = true;
                Radar.SetShip(target, statCode3);
                target.Ship.Hit();
            }
            //Om skeppet sjunker
            else if (statCode2First == "25")
            {
                var target = Radar.Targets.Where(t => t.GridPosition == myLastComPosition).FirstOrDefault();
                target.IsAlreadyHit = true;
                target.HasShip      = true;
                Radar.SetShip(target, statCode3);
                target.Ship.Hit();
            }
        }
    }
}
