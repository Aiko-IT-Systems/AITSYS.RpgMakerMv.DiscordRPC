using DiscordRPC;

using System;
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
					var (LargeMap, SmallMap) = msg.BuildMap();
					Console.WriteLine($"Main: L::{LargeMap} S::{SmallMap ?? "none"}");
					byte[] ret = new byte[LargeMap.Length];
					ret = Encoding.Default.GetBytes("Updating map to: " + LargeMap);
					rpc.UpdateDetails("Exploring the world");
					string mapName = LargeMap.ConvertMapName();
					rpc.UpdateState(mapName);
					var prefix = SmallMap != null ? SmallMap.GetDiscordAssetPrefix() : LargeMap.GetDiscordAssetPrefix();
					var la = $"{prefix}{LargeMap.ToLower()}";
					if (la.EndsWith("wasteland"))
						la += "s";
					var sa = SmallMap != null ? prefix + SmallMap.ToLower() : "logo";
					if (sa.EndsWith("wasteland"))
						sa += "s";
					Console.WriteLine($"Updating large asset to: {la}");
					rpc.UpdateLargeAsset(la, LargeMap);
					Console.WriteLine($"Updating small asset to: {sa}");
					rpc.UpdateSmallAsset(sa, "Being a traveler");
					rpc.SynchronizeState();
					ns.Write(ret, 0, ret.Length);
					ns.Close();
					/*if (LargeMap == "close")
                    {
                        running = false;
                    }*/
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
