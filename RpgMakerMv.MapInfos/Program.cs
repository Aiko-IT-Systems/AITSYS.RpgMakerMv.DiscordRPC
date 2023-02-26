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
		Console.WriteLine("Using " + path);
		MapWrapper wrapper = new(path);
		wrapper.GetFileContent();
		wrapper.GenerateMapInfoList();
		Console.Write("Populate rpc config for all maps (Skips question for every single map) [Y/N]: ");
		var res = Console.ReadKey();
		ConsoleKeyInfo? setDefault;
		if (res.Key == ConsoleKey.Y)
			setDefault = new('N', ConsoleKey.N, false, false, false);
		else
			setDefault = null;
		List<MapInfo?> NewMapInfos = new();
		if (wrapper != null && wrapper.Maps != null && wrapper.Maps.Any())
		{
			foreach (var map in wrapper.Maps)
			{
				if (map == null)
				{
					NewMapInfos.Add(null);
					Console.WriteLine("Map is null. Skipping..");
				}
				else
				{
					Console.WriteLine($"Map {map.Name} [{map.Id}]");
					Console.WriteLine($"Generating template rpc config for map {map.Id}");
					if (!map.Params.Any())
					{
						Console.WriteLine("Creating new rpc config");
						Console.Write("Set data [Y/N]: ");
						var set = setDefault ?? Console.ReadKey();
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
						NewMapInfos.Add(map);
					}
					else if (map.Params.Any() && map.Params.Any(x => x.Type == "rpc"))
					{
						Console.WriteLine("Rpc config already set");
						Console.Write("Checking if data set.. ");
						Console.WriteLine(map.Params.First(x => x.Type == "rpc").Data?.SmallAssetText == null ? "False" : "True");
						Console.Write("Overwrite data [Y/N]: ");
						var set = setDefault ?? Console.ReadKey();
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
						NewMapInfos.Add(map);
					}
					else
					{
						Console.WriteLine("Attaching new rpc config");
						Console.Write("Set data [Y/N]: ");
						var set = setDefault ?? Console.ReadKey();
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
						NewMapInfos.Add(map);
					}
				}
			}
			//wrapper.Maps = NewMapInfos;
			Console.WriteLine("Writing new MapInfo.json");
			wrapper.WriteMapInfo();
			Console.WriteLine("Done! Please close RPG Maker without saving and re-open it, otherwise the MapInfos.json will be overriden again :(");
		}
		else
			Console.WriteLine("Maps were empty.");
		Console.WriteLine("Press any key to continue.");
		Console.ReadKey();
		Environment.Exit(0);
	}
}