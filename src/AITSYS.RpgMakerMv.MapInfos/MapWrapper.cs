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

public class MapWrapper
{
	public string MapPath { get; internal set; } = string.Empty;
	internal FileStream MapInfoFile { get; set; } = null!;
	internal string MapInfoJson { get; set; } = string.Empty;
	public List<MapInfo?> Maps { get; internal set; } = new();

	public MapWrapper(string map_path)
	{
		this.MapPath = map_path;
		try
		{
			File.SetAttributes(this.MapPath, FileAttributes.Normal);
			this.MapInfoFile = this.LoadFile(false);
		}
		catch (Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			//Environment.Exit(1);
		}
	}

	private FileStream LoadFile(bool write)
		=> write ? File.Create(this.MapPath) : File.OpenRead(this.MapPath);

	public void GetFileContent()
	{
		this.MapInfoFile.Position = 0;
		using StreamReader reader = new(this.MapInfoFile);
		var content = reader.ReadToEnd();
		this.MapInfoJson = content;
	}

	public void GenerateMapInfoList() => this.Maps = JsonConvert.DeserializeObject<List<MapInfo?>>(this.MapInfoJson)!;

	public void WriteMapInfo()
	{
		try
		{
			var writeData = JsonConvert.SerializeObject(this.Maps, Formatting.None);
			using StreamWriter writer = new(this.LoadFile(true));
			writer.Write(writeData);
			writer.Flush();
		}
		catch (Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			//Environment.Exit(1);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Meow")]
	public ParamData SetParams()
	{
		Console.WriteLine();
		Console.Write("Small asset key: ");
		var sak = Console.ReadLine();
		Console.WriteLine();
		Console.Write("Small asset text (i.e. Worldname): ");
		var sat = Console.ReadLine();
		Console.WriteLine();
		Console.Write("Large asset key: ");
		var lak = Console.ReadLine();
		Console.WriteLine();
		Console.Write("Large asset text (i.e. Regionname): ");
		var lat = Console.ReadLine();
		Console.WriteLine();
		Console.Write("Rpc state (i.e. Exploring World X): ");
		var d = Console.ReadLine();
		Console.WriteLine();
		Console.Write("Rpc details (i.e. In Dungeon): ");
		var s = Console.ReadLine();
		Console.WriteLine();
		return new ParamData(sak, sat, lak, lat, d, s);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Meow")]
	public ParamData SetEmptyParams()
		=> new(null, null, null, null, null, null);
}
