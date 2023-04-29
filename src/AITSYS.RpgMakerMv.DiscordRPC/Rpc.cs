// This file is part of an AITSYS project.
//
// Copyright (c) AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using AITSYS.RpgMakerMv.Rpc.Entities;
using AITSYS.RpgMakerMv.Rpc.Enums;

namespace AITSYS.RpgMakerMv.Rpc;

public class Rpc
{
	public static RpcData? s_defaultInfo { get; private set; } = null!;
	public static RpcConfig s_defaultConfig { get; private set; } = null!;
	public static Timestamps s_startedAt { get; private set; } = null!;
	public static string s_dapp { get; private set; } = "805569792446562334";
	public static string? s_sapp { get; private set; } = null;
	public static CancellationTokenSource s_cancellationToken { get; private set; } = new();

	public static async Task Main(string[] args)
	{
		if (args.Length != 0)
			for (var i = 0; i < args.Length; i++)
			{
				if (args[i] == "--dapp")
					s_dapp = args[i + 1];
				else if (args[i] == "--sapp")
					s_sapp = args[i + 1];
			}

		Console.WriteLine("Using following app ids:\n\tDiscord: {0}\n\tSteam: {1}", s_dapp, s_sapp);

		s_startedAt = Timestamps.Now;

		TcpListener server = new(IPAddress.Loopback, 59090);

		DiscordRpcClient rpc = new(s_dapp);

		rpc.OnReady += (sender, msg) =>
		{
			Console.WriteLine("Connected to discord with user {0}#{1}", msg.User.Username, msg.User.Discriminator);
			sender.SetPresence(new()
			{
				Details = s_defaultInfo?.Details ?? "Playing Game",
				State = s_defaultInfo?.State ?? null,
				Assets = new()
				{
					SmallImageKey = s_defaultInfo?.SmallAssetKey ?? "logo",
					SmallImageText = s_defaultInfo?.SmallAssetText ?? "RMMV Game",
					LargeImageKey = s_defaultInfo?.LargeAssetKey,
					LargeImageText = s_defaultInfo?.LargeAssetKey
				},
				Timestamps = s_startedAt,
				Buttons = new Button[]
				{
					new() { Label = "Get RPC Extension", Url = "https://github.com/Aiko-IT-Systems/AITSYS.RpgMakerMv.Rpc" },
					new() { Label = "RPC Extension Support", Url = "https://discord.gg/Uk7sggRBTm" }
				}
			});
			sender.SynchronizeState();
		};
		rpc.OnPresenceUpdate += (sender, msg) => Console.WriteLine("Presence has been updated!");
		rpc.OnError += (sender, e) => Console.WriteLine(e.Message);
		rpc.Initialize();
		if (s_sapp != null)
			rpc.RegisterUriScheme(s_sapp);
		rpc.Invoke();

		server.Start();

		while (!s_cancellationToken.IsCancellationRequested)
		{
			var client = await server.AcceptTcpClientAsync(s_cancellationToken.Token);

			var ns = client.GetStream();

			while (client.Connected)
			{
				var shutdownRequested = false;
				try
				{
					var msg = new byte[8192];
					await ns.ReadAsync(msg, s_cancellationToken.Token);
					var data = Encoding.UTF8.GetString(msg);
					var command = JsonConvert.DeserializeObject<RpcCommand>(data)!;
					Console.WriteLine(JsonConvert.SerializeObject(command, Formatting.Indented));

					if (command.CommandType == RpcCommandType.SetConfig)
					{
						if (command.RpcConfig == null)
							throw new InvalidDataException("Rpc config was null");

					}
					else if (command.CommandType == RpcCommandType.SetData)
					{
						if (command.RpcData == null)
							throw new InvalidDataException("Rpc data was null");

					}
					else if (command.CommandType == RpcCommandType.Shutdown)
						shutdownRequested = true;

					var response = Encoding.Default.GetBytes(JsonConvert.SerializeObject(new RpcResponse(RpcResponseType.Success)));
					await ns.WriteAsync(response, s_cancellationToken.Token);
				}
				catch (Exception ex)
				{
					try
					{
						var response = Encoding.Default.GetBytes(JsonConvert.SerializeObject(new RpcResponse(RpcResponseType.Failure, ex.Message)));
						await ns.WriteAsync(response, s_cancellationToken.Token);
					}
					catch (Exception)
					{ }
				}
				finally
				{
					ns.Close();
					if (shutdownRequested)
						s_cancellationToken.Cancel();
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
	*/
}
