using System.Net;
using System.Threading.Tasks;
using NetworkObj.TCP;
using NetworkObj.Utils;

namespace NetworkObj
{
    class Program
    {
        private static int CurrentArg = 0;
        private static int Indexed = 0;
        private static bool indexedServer = false;
        private static bool indexedPort = false;
        private static int port = 4201;
        private static int server = 1;

        static async Task Main(string[] args)
        {
            foreach (string arg in args)
            {
                if ((arg == "-s" || arg == "--server") && !indexedServer)
                {
                    indexedServer = true;
                    Indexed++;

                    if (int.TryParse(args[CurrentArg + 1], out int serverVersion))
                    {
                        if (serverVersion == 1)
                        {
                            // no functionality yet to change server.
                            server = serverVersion;
                            Logger.Info("Running 4.3+ Server");
                        }
                        else
                        {
                            Environment.Exit(0);
                            break;
                        }
                    }
                    else
                    {
                        Environment.Exit(0);
                        break;
                    }
                }
                else if ((arg == "-p" || arg == "--port") && !indexedPort)
                {
                    indexedPort = true;
                    Indexed++;

                    if (int.TryParse(args[CurrentArg + 1], out int servPort))
                    {
                        port = servPort;
                    }
                    else
                    {
                        Environment.Exit(0);
                        break;
                    }
                }

                if (Indexed == 2) break;

                CurrentArg++;
            }

            Listener Server = new Listener();
            IPAddress IP = IPAddress.Any;

            await Server.Start(IP, port);

            Console.ReadLine();
        }
    }
}