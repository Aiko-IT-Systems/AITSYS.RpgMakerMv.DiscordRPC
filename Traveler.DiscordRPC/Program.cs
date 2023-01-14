using DiscordRPC;

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Traveler.DiscordRPC
{
    class Program
    {
        static void Main()
        {
			DiscordRpcClient rpc = new("684577660785000503");
            TcpListener server = new(IPAddress.Loopback, 59090);

            rpc.OnReady += (sender, msg) => { Console.WriteLine("Connected to discord with user {0}", msg.User.Username); };
            rpc.OnPresenceUpdate += (sender, msg) => { Console.WriteLine("Presence has been updated! "); };
            rpc.OnError += (s, e) => Console.WriteLine(e.Message);
			rpc.Initialize();
			rpc.RegisterUriScheme("2184270");
			rpc.Invoke();
            var random = new Random();

			server.Start();
			rpc.SetPresence(new()
            {
                Details = null,
                State = "In menu",
                Assets = new()
                {
                    SmallImageKey = "logo",
                    SmallImageText = "Traveler",
                    LargeImageKey = "https://store.steampowered.com/gfxproxy/betagfx/apps/2184270/extras/store_capsule_main.png?t=1673295897",
                    LargeImageText = "Traveler @Steam"
                },
                Timestamps = Timestamps.Now,
                Buttons = new Button[]
                {
                    new() { Label = "Wishlist on Steam", Url = "steam://store/2184270" },
                    new() { Label = "Join Discord", Url = "https://discord.gg/CKAEfC4qMr" }
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

		/*
 Example Asset when exploring the rotten temple on albehir:
Assets = new Assets()
{
	SmallImageKey = "world01-albehir",
	SmallImageText = "On Albehir",
	LargeImageKey = "world01-rottentemple",
	LargeImageText = "Exploring the Rotten Temple"
},
*/


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
