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

using AITSYS.RpgMakerMv.MapInfos.Entities;

namespace AITSYS.RpgMakerMv.MapInfos;

public class MapInfoGenerator
{
	public static void Main(string[] args)
	{
		Console.WriteLine("RPG Maker MV Map Tool for Discord RPC");
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.Write("Path to MapInfos.json: ");
		var path = args.Any() ? args[0] : Console.ReadLine();
		if (path == null)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Error");
			Environment.Exit(1);
		}
		if (path.Contains("Game.rpgproject"))
			path = Path.Join(path.Replace("Game.rpgproject", ""), "data", "MapInfos.json");
		Console.WriteLine("Using " + path);
		MapWrapper wrapper = new(path);
		wrapper.GetFileContent();
		wrapper.GenerateMapInfoList();
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.Write("Populate rpc config for all maps (Skips question for every single map) [Y/N]: ");
		var res = Console.ReadKey();
		var setDefault = res.Key == ConsoleKey.Y ? new('N', ConsoleKey.N, false, false, false) : (ConsoleKeyInfo?)null;
		List<MapInfo?> NewMapInfos = new();
		if (wrapper != null && wrapper.Maps != null && wrapper.Maps.Any())
		{
			foreach (var map in wrapper.Maps)
				if (map == null)
				{
					NewMapInfos.Add(null);
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Map is null. Skipping..");
				}
				else
				{
					Console.WriteLine($"Map {map.Name} [{map.Id}]");
					Console.WriteLine($"Generating template rpc config for map {map.Id}");
					if (!map.Params.Any())
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Creating new rpc config");
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("Set data [Y/N]: ");
						var set = setDefault ?? Console.ReadKey();
						var data = set.Key == ConsoleKey.Y ? wrapper.SetParams() : wrapper.SetEmptyParams();
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
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("Rpc config already set");
						Console.Write("Checking if data set.. ");
						Console.WriteLine(map.Params.First(x => x.Type == "rpc").Data?.SmallAssetText == null ? "False" : "True");
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("Overwrite data [Y/N]: ");
						var set = setDefault ?? Console.ReadKey();
						if (set.Key == ConsoleKey.Y)
						{
							Console.ForegroundColor = ConsoleColor.Green;
							map.Params.First(x => x.Type == "rpc").Data = wrapper.SetParams();
							Console.WriteLine("Data applied");
						}
						else
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.WriteLine("");
							Console.WriteLine("Skipping..");
						}
						NewMapInfos.Add(map);
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Attaching new rpc config");
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write("Set data [Y/N]: ");
						var set = setDefault ?? Console.ReadKey();
						var data = set.Key == ConsoleKey.Y ? wrapper.SetParams() : wrapper.SetEmptyParams();
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
			//wrapper.Maps = NewMapInfos;
			Console.WriteLine("Writing new MapInfo.json");
			wrapper.WriteMapInfo();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Done!");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Please close RPG Maker without saving and re-open it, otherwise the MapInfos.json will be overriden again :(");
		}
		else
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Maps were empty.");
		}

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("Press any key to continue.");
		Console.ReadKey();
		Environment.Exit(Environment.ExitCode);
	}
}
