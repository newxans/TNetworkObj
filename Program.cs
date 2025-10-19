using System.Net;
using System.Threading.Tasks;
using NetworkObj.TCP;

namespace NetworkObj
{
    class Program
    {
        static async Task Main()
        {
            Listener Server = new Listener();
            IPAddress IP = IPAddress.Any;
            int Port = 4201;

            await Server.Start(IP, Port);

            Console.ReadLine();
        }
    }
}