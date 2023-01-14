using DiscordRPC;

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

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
                    var mapInfo = msg.BuildMap();
                    byte[] ret = new byte[mapInfo.LargeMap.Length];
                    ret = Encoding.Default.GetBytes("Updating map to: " + mapInfo.LargeMap);
                    Console.WriteLine("Map: " + mapInfo.LargeMap);
                    rpc.UpdateDetails("Exploring the world");
                    string mapName = mapInfo.LargeMap.ConvertMapName();
                    rpc.UpdateState(mapName);
                    rpc.UpdateLargeAsset(mapInfo.LargeMap, mapName);
                    rpc.UpdateSmallAsset(mapInfo.SmallMap, "Being a traveler");
                    rpc.SynchronizeState();
                    ns.Write(ret, 0, ret.Length);
                    ns.Close();
                    /*
                    if (mapInfo[1] == "null")
                    {
                        running = false;
                    }
                    */
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
    }
}
