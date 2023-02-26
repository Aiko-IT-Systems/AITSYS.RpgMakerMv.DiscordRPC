using Newtonsoft.Json;

namespace RpgMakerMv.MapInfos;

public class MapWrapper
{
	public string MapPath { get; internal set; }
	internal FileStream MapInfoFile { get; set; }
	internal string MapInfoJson { get; set; } = string.Empty;
	public List<MapInfo?> Maps { get; set; } = new();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public MapWrapper(string map_path)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	{
		MapPath = map_path;
		try
		{
			File.SetAttributes(MapPath, FileAttributes.Normal);
			MapInfoFile = LoadFile(false);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			//Environment.Exit(1);
		}
	}

	private FileStream LoadFile(bool write)
		=> write ? File.Create(MapPath) : File.OpenRead(MapPath);

	public void GetFileContent()
	{
		MapInfoFile.Position = 0;
		using StreamReader reader = new(MapInfoFile);
		var content = reader.ReadToEnd();
		MapInfoJson = content;
	}

	public void GenerateMapInfoList()
	{
		Maps = JsonConvert.DeserializeObject<List<MapInfo?>>(MapInfoJson)!;
	}

	public void WriteMapInfo()
	{
		try
		{
			var writeData = JsonConvert.SerializeObject(Maps, Formatting.None);
			using StreamWriter writer = new(LoadFile(true));
			writer.Write(writeData);
			writer.Flush();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			//Environment.Exit(1);
		}
	}

	public ParamData SetParams()
	{
		Console.Write("Small asset key: ");
		var sak = Console.ReadLine();
		Console.Write("Small asset text (i.e. Worldname): ");
		var sat = Console.ReadLine();
		Console.Write("Large asset key: ");
		var lak = Console.ReadLine();
		Console.Write("Large asset text (i.e. Regionname): ");
		var lat = Console.ReadLine();
		Console.Write("Rpc details (i.e. Exploring World X): ");
		var d = Console.ReadLine();
		return new ParamData(sak, sat, lak, lat, d);
	}

	public ParamData SetEmptyParams()
		=> new ParamData(null, null, null, null, null);
}
