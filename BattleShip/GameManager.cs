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
        private TcpListener     listener;
        private TcpClient       Client;
        private NetworkStream   NetworkStream;
        private StreamReader    reader;
        private StreamWriter    writer;
        private int             PortChangeCount = 0;
        private bool            NiceMode = true;
        private int             ErrorCount      = 0;
        private int             MyErrorCount    = 0;
        private bool            NoError
        {   get
            {
                return (ErrorCount < 4 && MyErrorCount < 4);
            }
            set { }
        }
        private string          Mode
        {
            get
            {
                return NiceMode ? "NICE MODE" : "BRUTAL MODE";
            }
            set { }
        }

        public void Initialize()
        {
            PrintIntro();
            PrintMode();

            NoError = true;

            MiddleWL($"{Mode}\n", true);
            MiddleWL("Ships are placed...\n");
            MiddleWL("Username: ", false, true, 8);
            Username = Console.ReadLine();
            if (Username == "o") ShowOceanView();
            if (Username == "r") ShowRadar();
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
                bool isGood = Connect();

                if (isGood && NoError)
                {
                    Play();
                }

                DisposeAll();
            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);
                DisposeAll();
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Unknown hostname '{Host}'");
                Console.WriteLine(e.Message);
                DisposeAll();
            }
            catch (Exception e)
            {
                Console.WriteLine("Oops, Something went wrong");
                Console.WriteLine(e.Message);
                DisposeAll();
            }

            Console.SetCursorPosition(Console.WindowWidth / 2, Console.CursorTop +1);
        }

        public bool Connect()
        {
            bool AllGood = false;

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
                        AllGood = true;

                        Console.Clear();
                        Console.WriteLine($"Klient har anslutit sig {Client.Client.RemoteEndPoint}!");
                        writer.WriteLine(RCodes.BattleShip.FullString);
                        WriteColor(true, RCodes.BattleShip.FullString);

                        while (true)
                        {
                            var helloText = reader.ReadLine();
                            Error(helloText, false);
                            WriteColor(false, helloText);

                            try
                            {
                                var part1 = helloText.Split()[0];

                                if (part1.ToUpper() == "QUIT")
                                {
                                    AllGood = false;
                                    break;
                                }
                                else if (part1.ToUpper() == "HELLO" || part1.ToUpper() == "HELO")
                                {
                                    var part2 = helloText.Split()[1];
                                    Player2 = part2;
                                    writer.WriteLine($"{RCodes.RemotePlayerName.Code} {Username}");
                                    WriteColor(true, $"{RCodes.RemotePlayerName.Code} {Username}");
                                    break;
                                }
                                else
                                {
                                    writer.WriteLine(RCodes.SequenceError.FullString);
                                    WriteColor(true, RCodes.SequenceError.FullString);
                                    Error("501", true);
                                }
                            }
                            catch (Exception)
                            {
                                writer.Write(RCodes.SequenceError.FullString);
                                WriteColor(true, RCodes.SequenceError.FullString);
                                Error("500", true);
                                writer.Write(RCodes.ConnectionClosed.FullString);
                                Console.WriteLine(RCodes.SequenceError.FullString);
                                AllGood = false;
                                break;
                            }
                        }

                        while (true && NoError)
                        {
                            var start = reader.ReadLine();
                            WriteColor(false, start);
                            if (start.ToUpper() == "START")
                            {
                                ErrorCount = 0;

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
                            else if (start.ToUpper() == "HELP")
                            {
                                var help = WriteHelpForClient(true);
                                writer.WriteLine(help);
                                Console.WriteLine("[Client was given help]");
                            }
                            else
                            {
                                writer.WriteLine($"{RCodes.SequenceError.FullString} (Client starts the game by writing 'START')");
                                WriteColor(true, $"{RCodes.SequenceError.FullString} (Client starts the game by writing 'START')");
                                Error("501", true);
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

                var protocolFirst = RemoveUnwantedAsciiFromHead(reader.ReadLine());

                WriteColor(true, protocolFirst);

                if (protocolFirst.ToUpper() != "210 BATTLESHIP/1.0" && protocolFirst.ToUpper().Replace(" ", "") != "210BATTLESHIP/1.0")
                {
                    writer.WriteLine($"{RCodes.ConnectionClosed.FullString} - Incorrect protocol from host");
                    Console.WriteLine($"{RCodes.ConnectionClosed.FullString} - Incorrect protocol from host");
                    Console.ReadKey();
                }
                else
                {
                    AllGood = true;

                    writer.WriteLine($"HELLO {Username}");
                    WriteColor(false, $"HELLO {Username}");

                    try
                    {
                        var hostGreets = RemoveUnwantedAsciiFromHead(reader.ReadLine());
                        Error(hostGreets, false);
                        Player2 = hostGreets.Split()[1];
                        WriteColor(true, hostGreets);
                    }
                    catch (Exception)
                    {
                        Error("500", true);
                        Player2 = "(Host)";
                        WriteColor(true, "220 " + Player2);
                    }

                    while (true && NoError)
                    {
                        Console.WriteLine("(Write 'START' to start the game)");
                        var start = Console.ReadLine().ToUpper();
                        FixRow();
                        WriteColor(false, start);
                        writer.WriteLine(start);

                        var whoStarts = RemoveUnwantedAsciiFromHead(reader.ReadLine());

                        Error(whoStarts, false);

                        WriteColor(true, whoStarts);
                        try
                        {
                            var code = whoStarts.Split(" ")[0];

                            if (code == RCodes.HostStarts.Code)
                            {
                                //Host startar
                                HostStarts = true;
                                break;
                            }
                            else if (code == RCodes.ClientStarts.Code)
                            {
                                //Client startar
                                HostStarts = false;
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("What??");
                            throw;
                        }
                        //WriteColor(true, whoStarts);
                    }
                }
            }
            return AllGood;
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

            while (arePlaying && NoError)
            {
                try
                {
                    //Vem som ska börja LÄSA command från motståndaren.
                    if ((IsHost && !HostStarts) || (!IsHost && HostStarts))
                    {
                        opponentCommand = reader.ReadLine();
                        Error(opponentCommand, false);
                        if (NoError)
                        {
                            hitStatus = Read(opponentCommand);

                            WriteColor(!IsHost, $"{Player2}: {opponentCommand.ToUpper()}");
                            if (hitStatus.Split(" ")[0] == "260")
                            {
                                //TODO: ta bort
                                WriteColor(IsHost, $"{Player2} won the game! You lose...");
                            }
                            else
                            {
                                WriteColor(IsHost, hitStatus);
                            }
                            writer.WriteLine(hitStatus);
                            Error(hitStatus, true);

                        }
                    }

                    while (Client.Connected && NoError)
                    {
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
                            opponentHitStatus = RemoveUnwantedAsciiFromHead(reader.ReadLine());
                            Error(opponentHitStatus, false);
                            WriteColor(!IsHost, opponentHitStatus);

                            while(NiceMode && (opponentHitStatus.Split(" ")[0] == RCodes.SyntaxError.Code || opponentHitStatus.Split(" ")[0] == RCodes.SequenceError.Code) && NoError)
                            {
                                myLastCommand = Write();
                                opponentHitStatus = RemoveUnwantedAsciiFromHead(reader.ReadLine());
                                Error(opponentHitStatus, false);
                                WriteColor(!IsHost, opponentHitStatus);
                            }

                            if (NoError && opponentHitStatus.ToUpper() != "QUIT")
                            {
                                opponentCommand = reader.ReadLine();
                            }

                            hitStatus = Read(opponentCommand, opponentHitStatus, myLastCommand);

                            WriteColor(!IsHost, $"{Player2}: {opponentCommand.ToUpper()}");

                            if (hitStatus.Split(" ")[0] == "260")
                            {
                                //TODO: ta bort
                                WriteColor(IsHost, $"{Player2} won the game! You lose...");
                            }
                            else
                            {
                                WriteColor(IsHost, hitStatus);
                            }
                            writer.WriteLine(hitStatus);
                            Error(hitStatus, true);



                            //Motståndaren vann
                            if (hitStatus == "270" || hitStatus.Split(" ")[0] == "260" || NoError == false) 
                            {
                                arePlaying = false;
                                break;
                            }
                            //Du vann
                            else if (hitStatus == "260")
                            {
                                arePlaying = false;
                                break;
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong here.");
                    writer.WriteLine("Something went wrong here.");
                    arePlaying = false;
                }
            }
        }

        private string Read(string command, string opponentHitStatus = "", string myLastCom = "")
        {
            if (NoError)
            { 
                string Answer = "";
                string com1 =   "";
                string targ =   "";
                string mylastcomPosition = "";

                try
                {
                    com1 = command.Split(" ")[0];
                    mylastcomPosition = myLastCom.Split(" ")[1];
                
                }
                catch (Exception)
                {}

                try
                {
                    targ = command.Split(" ")[1];
                }
                catch (Exception)
                { }

                if (mylastcomPosition != "" && opponentHitStatus != "")
                {
                    SetEnemyRadarLastHit(mylastcomPosition.ToUpper(), opponentHitStatus);
                }

                if (opponentHitStatus.Split(" ")[0] == "270" || com1 == "270")
                {
                    WriteColor(IsHost, RCodes.ConnectionClosed.FullString);
                    return "270";
                }

                if (opponentHitStatus.Split(" ")[0] == "260" || com1 == "260")
                {
                    WriteColor(IsHost, RCodes.YouWin.FullString);
                    return "260";
                }

                if (string.Equals(command.Trim(), "QUIT", StringComparison.InvariantCultureIgnoreCase) || opponentHitStatus.ToUpper() == "QUIT")
                {
                    if (IsHost)
                    {
                        WriteColor(!IsHost, "The client wants to quit. (press any key to continue)");
                        Console.ReadKey();
                        writer.WriteLine(RCodes.ConnectionClosed.FullString + " - You have quit the game.");
                        Client.Client.Disconnect(true);
                        WriteColor(IsHost, RCodes.YouWin.FullString + " - Client has disconnected.");
                        return "270";
                    }
                    else
                    {
                        WriteColor(IsHost, RCodes.YouWin.FullString);
                        Answer = RCodes.SyntaxError.FullString + " (Host should quit, not client)";
                    }
         
                }
                else if (string.Equals(command.Trim(), "HELP", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (NiceMode)
                    {
                        var help = WriteHelpForClient();
                        writer.WriteLine(help);
                        var newCom = reader.ReadLine();
                        Answer = Read(newCom, opponentHitStatus, myLastCom);
                    }
                    else
                    {
                        Answer = WriteHelpForClient();
                    }
                
                
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
                else if(com1 == "HELLO" || com1 == "HELO" || com1 == "START")
                {
                    if (NiceMode)
                    {
                        writer.WriteLine(RCodes.SequenceError.FullString);
                        Error("501", true);
                        var newCom = reader.ReadLine();
                        Error(newCom, false);
                        Answer = Read(newCom, opponentHitStatus, myLastCom);
                    }
                }
                else
                {
                    if (!NoError)
                    {
                        Answer = command;
                    }
                    while (NiceMode && NoError)
                    {
                        writer.WriteLine(RCodes.SyntaxError.FullString);
                        Error("500", true);
                        var newCom = reader.ReadLine();
                        Error(newCom, false);
                        Answer = Read(newCom, opponentHitStatus, myLastCom);
                    }
                    if (!NiceMode)
                    {
                        Answer = $"{RCodes.SyntaxError.FullString}";
                    }
                }

                return Answer;
            }
            else
            {
                WriteColor(IsHost, RCodes.ConnectionClosed.FullString);
                writer.WriteLine(RCodes.ConnectionClosed.FullString);
                return "270";
            }
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
            while (isOtherCommand && NoError)
            {
                Console.Write("Send: ");
                command = Console.ReadLine();
                command = command.ToUpper().Trim();

                if (command == "" || string.IsNullOrEmpty(command))
                {
                    FixRow();
                    Console.WriteLine("[Command was empty (write 'HELP' for available commands)]");
                }
                else if (command == "RADAR" || command == "R" || command == "SHOWRADAR" || command == "RADARVIEW" || command == "TARGET" || command == "TARGETGRID")
                {
                    ShowRadar();
                }
                else if (command == "OCEAN" || command == "O" || command == "SHOWOCEAN" || command == "OCEANVIEW" || command == "OCEANGRID")
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
                else if (command.Split(" ")[0] == "FIRE")
                {
                    isOtherCommand = false;
                }
                else
                {
                    if (NiceMode)
                    {
                        //Console.WriteLine(RCodes.SyntaxError.FullString);
                        //Error(RCodes.SyntaxError.Code, true);
                        //isOtherCommand = false;
                        writer.WriteLine(command);
                        var s = reader.ReadLine();
                        Error(s, false);
                        Console.WriteLine(s);
                    }
                    else
                    {
                        isOtherCommand = false;
                    }
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
            if (writer != null)        writer.Dispose();
            if (reader != null)        reader.Dispose();
            if (NetworkStream != null) NetworkStream.Dispose();

            if (Client != null && Client.Connected)
            {
                Socket socket = Client.Client;

                if (socket != null && socket.Connected)
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

            for (int i = 0; i < 27; i++)
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

            for (int i = 0; i < 27; i++)
            {
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        private string WriteHelpForClient(bool isBeginning = false)
        {
            var lf = (char)10;
            var cr = (char)13;

            string help = "---- - HELP----- " + lf + cr +
                "This is a classic BattleShip game, you connect with another player," + lf + cr +
                "and you take turns to fire." + lf + cr +
                "This game is not case sensitive" + lf + cr +
                "(it doesn't matter if you write 'HELP' or 'help' - or even 'hElP')." + lf + cr +
                "After you have got a connection with another player" + lf + cr +
                "the CLIENT have to say 'START' to start the game." + lf + cr +
                "COMMANDS:" + lf + cr +
                "HELLO <username>  - This is the first command a client have to use when connected." + lf + cr +
                "START             - This is how the client starts the game after you have connected." + lf + cr +
                "FIRE <coordinate> - When the game has started, this is the command to fire at the enemy ship." + lf + cr +
                "                    <coordinate> have to be replaced by something between A1 to J10..." + lf + cr +
                "                    ...example: 'FIRE C6'. Optional message after the coordinate can be applied." + lf + cr +
                "HELP              - Shows help. Obviously you found it..." + lf + cr +
                "QUIT              - Quits the game and disconnects from the other player." + lf + cr + lf;

                help += "------------------------------------------------------------------------------------" + lf + cr;


            if (isBeginning)
            {
                help += "(Client starts the game by writing 'START')" + lf + cr;
            }

            return help;
            //writer.WriteLine(help);
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
            HelpWL("OCEAN             - Shows the Ocean Grid. This is your ships on a map.");
            HelpWL("TARGET/RADAR      - Shows the Target Grid, where you have hit/missed previous shots to the enemy ships");
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

        public void PrintMode()
        {
            MiddleWL("CHOOSE MODE", true);
            MiddleWL("(Nice mode is default)");
            Console.WriteLine();
            MiddleWL("[Nice mode]   - this mode lets you write another command after writing 'HELP'");
            MiddleWL("                or unknown syntax during the game.", false, false, 14);
            MiddleWL("[Brutal mode] - this mode will count 'HELP' or unknown syntax as your command");
            MiddleWL("                for that round during the game.", false, false, 15);

            Console.WriteLine();
            MiddleWL("Mode: ", false, true);
            var mode = Console.ReadLine().ToUpper();

            if (mode == "B" || mode == "BRUTAL" || mode == "BRUTAL MODE" || mode == "BRUTALMODE")
            {
                NiceMode = false;
            }
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
            var statCode = opHitStatus.Split(" ")[0];
            var statCode2First = statCode.Substring(0, 2);
            var statCode3 = statCode.Substring(2, 1);

            //Om det är miss:
            if (string.Equals(statCode, RCodes.Miss.Code))
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

        private string RemoveUnwantedAscii(string text)
        {
            byte[] asciivalues = Encoding.ASCII.GetBytes(text);

            string freshString = "";

            foreach (var b in asciivalues)
            {
                if (b > 47 && b < 58)
                {
                    var sign = (char)b;
                    freshString += sign.ToString();
                }
            }

            return freshString;
        }

        private string RemoveUnwantedAsciiFromHead(string text)
        {
            string freshString = "";

            var head = text.Split(" ")[0];
            var theRest = text.Replace(head, "");

            byte[] asciivalues = Encoding.ASCII.GetBytes(head);

            foreach (var b in asciivalues)
            {
                if (b > 47 && b < 58)
                {
                    var sign = (char)b;
                    freshString += sign.ToString();
                }
            }
            return freshString + theRest;
        }

        public void Error(string t, bool me)
        {
            try
            {

          
                var c = t.Substring(0, 3);

                if (c == RCodes.SequenceError.Code || c == RCodes.SyntaxError.Code)
                {
                    if (me)
                    {
                        MyErrorCount++;
                        if (MyErrorCount > 3)
                        {
                            NoError = false;
                        }
                    }
                    else
                    {
                        ErrorCount++;
                        if (ErrorCount >= 3)
                        {
                            NoError = false;
                        }
                    }
                }
                else
                {
                    if (me)
                    {
                        MyErrorCount = 0;
                    }
                    else
                    {
                        ErrorCount = 0;
                    }
                }
            }
            catch { }
        }
    }
}
