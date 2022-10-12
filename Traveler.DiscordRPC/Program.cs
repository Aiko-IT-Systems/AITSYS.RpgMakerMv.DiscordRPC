using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using DiscordRPC;

namespace Traveler.DiscordRPC
{
    class Program
    {
        static void Main()
        {
            DiscordRpcClient rpc = new DiscordRpcClient("684577660785000503");
            TcpListener server = new TcpListener(IPAddress.Loopback, 59090);

            rpc.OnReady += (sender, msg) => { Console.WriteLine("Connected to discord with user {0}", msg.User.Username); };
            rpc.OnPresenceUpdate += (sender, msg) => { Console.WriteLine("Presence has been updated! "); };
            rpc.Initialize();
            rpc.Invoke();

            server.Start();

            rpc.SetPresence(new RichPresence()
            {
                Details = "Playing Traveler",
                State = "In Menu",
                Assets = new Assets()
                {
                    SmallImageKey = "cute",
                    SmallImageText = "I am cute",
                    LargeImageKey = "logo",
                    LargeImageText = "Traveler"
                },
                Timestamps = Timestamps.Now,
                Buttons = new Button[]
                {
                    new Button() { Label = "Join Discord", Url = "https://discord.gg/NRZpu8uyW4" },
                    new Button() { Label = "Watch Developer Stream", Url = "https://www.twitch.tv/suuyasha" }
                }
            });
            rpc.SynchronizeState();
            bool running = true;
            while (running)
            {
                TcpClient client = server.AcceptTcpClient();

                NetworkStream ns = client.GetStream();

                while (client.Connected)
                {
                    byte[] msg = new byte[24];
                    ns.Read(msg, 0, msg.Length);
                    string[] mapInfo = Map(msg);
                    byte[] ret = new byte[mapInfo[0].Length];
                    ret = Encoding.Default.GetBytes("Updating map to: " + mapInfo[0]);
                    Console.WriteLine("Map: " + mapInfo[0]);
                    rpc.UpdateDetails("Exploring the world");
                    string mapName = ConvertMapName(mapInfo[1]);
                    rpc.UpdateState(mapName);
                    rpc.UpdateLargeAsset(mapInfo[0], mapName);
                    rpc.UpdateSmallAsset("logo", "Being a traveler");
                    rpc.SynchronizeState();
                    ns.Write(ret, 0, ret.Length);
                    ns.Close();
                    if (mapInfo[1] == "null")
                    {
                        running = false;
                    }
                }
            }
            rpc.ClearPresence();
            rpc.Deinitialize();
            rpc.Dispose();
            Environment.Exit(0);
        }

        static string[] Map(byte[] data)
        {
            var mapArray = new string[2];
            string[] baseArray = Encoding.UTF8.GetString(data).Split('\n');
            string[] baseDataArray = baseArray[0].Split(' ');
            string baseMap = baseDataArray[0];
            string[] baseDetailArray = baseDataArray.Skip(1).ToArray();
            string baseDetail = string.Join(" ", baseDetailArray);

            mapArray[0] = baseMap;
            mapArray[1] = baseDetail;

            return mapArray;
        }

        static string ConvertMapName(string name)
        {
            string mapName = name switch
            {
                "Lala Test" => "El Test",
                "menu" => "In Menu",
                _ => name
            };
            return mapName;
        }
    }
}
