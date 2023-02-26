namespace RpgMakerMv.MapInfos;

internal class Program
{
	// TODO: Show existing info and better description for Suu
	static void Main(string[] args)
	{
		Console.WriteLine("RPG Maker MV Map Tool for Discord RPC");
		Console.Write("Path to MapInfos.json: ");
		var path = args.Any() ? args[0] : Console.ReadLine();
		if (path == null)
		{
			Console.WriteLine("Error");
			Environment.Exit(1);
		}
		if (path.Contains("Game.rpgproject"))
			path = Path.Join(path.Replace("Game.rpgproject", ""), "data", "MapInfos.json");
		MapWrapper wrapper = new(path);
		wrapper.GetFileContent();
		wrapper.GenerateMapInfoList();
		if (wrapper.Maps.Any())
		{
			foreach (var map in wrapper.Maps)
			{
				if (map == null)
					Console.WriteLine("Map is null. Skipping..");
				else
				{
					Console.WriteLine($"Map {map.Name} [{map.Id}]");
					Console.WriteLine($"Generating template rpc config for map {map.Id}");
					if (!map.Params.Any())
					{
						Console.WriteLine("Creating new rpc config");
						Console.Write("Set data [Y/N]: ");
						var set = Console.ReadKey();
						ParamData data;
						if (set.Key == ConsoleKey.Y)
							data = wrapper.SetParams();
						else
							data = wrapper.SetEmptyParams();
						Console.WriteLine("");
						Console.WriteLine("Working..");
						map.Params = new()
						{
							new Param()
							{
								Type = "rpc",
								Data = data
							}
						};
					}
					else if (map.Params.Any() && map.Params.Any(x => x.Type == "rpc"))
					{
						Console.WriteLine("Rpc config already set");
						Console.Write("Checking if data set.. ");
						Console.WriteLine(map.Params.First(x => x.Type == "rpc").Data?.SmallAssetText == null ? "False" : "True");
						Console.Write("Overwrite data [Y/N]: ");
						var set = Console.ReadKey();
						if (set.Key == ConsoleKey.Y)
						{
							map.Params.First(x => x.Type == "rpc").Data = wrapper.SetParams();
							Console.WriteLine("Data applied");
						}
						else
						{
							Console.WriteLine("");
							Console.WriteLine("Skipping..");
						}
					}
					else
					{
						Console.WriteLine("Attaching new rpc config");
						Console.Write("Set data [Y/N]: ");
						var set = Console.ReadKey();
						ParamData data;
						if (set.Key == ConsoleKey.Y)
							data = wrapper.SetParams();
						else
							data = wrapper.SetEmptyParams();
						Console.WriteLine("");
						Console.WriteLine("Working..");
						map.Params.Add(new Param()
						{
							Type = "rpc",
							Data = data
						});
					}
				}
			}

			wrapper.WriteMapInfo();
		}
		else
			Console.WriteLine("Maps were empty.");
		Console.ReadKey();
		Environment.Exit(0);
	}
}