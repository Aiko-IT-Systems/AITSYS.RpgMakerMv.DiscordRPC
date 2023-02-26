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

namespace AITSYS.RpgMakerMv.Rpc;

internal static partial class MapHelper
{
	internal static Regex MapWithSubnameRegex = MapWithSubnameRegexp();
	internal static Regex MapRegex = MapRegexp();

	internal static (string Map, string? SubMap) GetMapDataFromString(this string map_data)
	{
		Console.WriteLine(map_data);
		if (map_data.IsWithSubMap())
		{
			var match = MapWithSubnameRegex.Match(map_data);
			var subMap = match.Groups["sub_map"].Value;
			var map = match.Groups["main_map"].Value;
			return (map, subMap);
		}
		else
		{
			var match = MapRegex.Match(map_data);
			var map = match.Groups["map"].Value;
			return (map, null);
		}
	}

	internal static bool IsWithSubMap(this string map_data)
		=> MapWithSubnameRegex.IsMatch(map_data);


	internal static (string LargeMap, string? SmallMap) BuildMap(this byte[] data)
	{
		var recv = Encoding.UTF8.GetString(data);
		Console.WriteLine($"Handling {recv}");
		var baseArray = recv.Split('\n');
		var (Map, SubMap) = baseArray[0].GetMapDataFromString();

		return (SubMap?.Replace(" ", "") ?? Map.Replace(" ", ""), SubMap != null ? Map.Replace(" ", "") : null);
	}

	internal static string ConvertMapName(this string name)
	{
		var mapName = name switch
		{
			"Lala Test" => "El Test",
			"menu" => "In Menu",
			_ => name
		};
		return mapName;
	}

	internal static string GetDiscordAssetPrefix(this string map)
	{
		Console.WriteLine($"Getting prefix for {map.ToLower()}");
		var seperator = "-";
		var prefix = map.ToLower() switch
		{
			"lostisland" => "world00" + seperator,
			"albehir" => "world01" + seperator,
			"fenrien" => "world02" + seperator,
			"ignar" => "world03" + seperator,
			"samudra" => "world04" + seperator,
			"felliah" => "world05" + seperator,
			"wasteland" => "world06" + seperator,
			"wastelands" => "world06" + seperator,
			"ultima" => "world07" + seperator,
			"desktop" => "world08" + seperator,
			_ => string.Empty
		};
		return prefix;
	}

	[GeneratedRegex("^(?<main_map>[\\w ]+) / {1}(?<sub_map>[\\w ]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
	private static partial Regex MapWithSubnameRegexp();

	[GeneratedRegex("^(?<map>[\\w ]+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled)]
	private static partial Regex MapRegexp();
}
