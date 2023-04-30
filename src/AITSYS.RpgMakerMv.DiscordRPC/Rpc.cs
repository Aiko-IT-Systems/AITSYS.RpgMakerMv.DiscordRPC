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
	public static RpcData? s_defaultData { get; private set; } = null!;
	public static RpcConfig? s_defaultConfig { get; private set; } = null!;
	public static Timestamps s_startedAt { get; private set; } = null!;
	public static string s_dapp { get; private set; } = "805569792446562334";
	public static string? s_sapp { get; private set; } = null;
	public static int s_port { get; private set; } = 59090;
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
				else if (args[i] == "--port" || args[i] == "-p")
					if (int.TryParse(args[i + 1], out var port))
						s_port = port;
					else
						Console.WriteLine("Could not parse port");
			}

		Console.WriteLine("Using following app ids:\n\tDiscord: {0}\n\tSteam: {1}", s_dapp, s_sapp);
		Console.WriteLine("Starting tcp listener on 127.0.0.1:{0}", s_port);

		s_startedAt = Timestamps.Now;

		TcpListener server = new(IPAddress.Loopback, s_port);

		DiscordRpcClient rpc = new(s_dapp);

		rpc.OnReady += (sender, msg) =>
		{
			Console.WriteLine("Connected to discord with user {0}#{1}", msg.User.Username, msg.User.Discriminator);

			var buttons = Array.Empty<Button>();
			if (s_defaultConfig?.UseButtonOne ?? true)
				buttons = buttons.Append(new() { Label = s_defaultConfig?.ButtonOne?.Label ?? "Get RPC Extension", Url = s_defaultConfig?.ButtonOne?.Url ?? "https://github.com/Aiko-IT-Systems/AITSYS.RpgMakerMv.Rpc" }).ToArray();
			if (s_defaultConfig?.UseButtonTwo ?? true)
				buttons = buttons.Append(new() { Label = s_defaultConfig?.ButtonTwo?.Label ?? "RPC Extension Support", Url = s_defaultConfig?.ButtonTwo?.Url ?? "https://discord.gg/Uk7sggRBTm" }).ToArray();

			sender.SetPresence(new()
			{
				Details = s_defaultData?.Details ?? "Playing Game",
				State = s_defaultData?.State ?? null,
				Assets = new()
				{
					SmallImageKey = s_defaultData?.SmallAssetKey ?? "logo",
					SmallImageText = s_defaultData?.SmallAssetText ?? "RMMV Game",
					LargeImageKey = s_defaultData?.LargeAssetKey,
					LargeImageText = s_defaultData?.LargeAssetKey
				},
				Timestamps = s_startedAt,
				Buttons = buttons
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
						s_defaultConfig = command.RpcConfig;
						var buttons = Array.Empty<Button>();
						if (command.RpcConfig.UseButtonOne)
							buttons = buttons.Append(new() { Label = command.RpcConfig.ButtonOne!.Label, Url = command.RpcConfig.ButtonOne!.Url }).ToArray();
						if (command.RpcConfig.UseButtonTwo)
							buttons = buttons.Append(new() { Label = command.RpcConfig.ButtonTwo!.Label, Url = command.RpcConfig.ButtonTwo!.Url }).ToArray();
						rpc.UpdateButtons(buttons);
					}
					else if (command.CommandType == RpcCommandType.SetData)
					{
						if (command.RpcData == null)
							throw new InvalidDataException("Rpc data was null");
						s_defaultData = command.RpcData;
						rpc.UpdateSmallAsset(command.RpcData.SmallAssetKey, command.RpcData.SmallAssetText);
						rpc.UpdateLargeAsset(command.RpcData.LargeAssetKey, command.RpcData.LargeAssetText);
						rpc.UpdateDetails(command.RpcData.Details);
						rpc.UpdateState(command.RpcData.State);
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
}
