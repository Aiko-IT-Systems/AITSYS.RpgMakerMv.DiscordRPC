﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Traveler.DiscordRPC;

internal static class MapHelper
{
	internal static Regex MapWithSubnameRegex = new(@"^(?<main_map>[\w ]+) / {1}(?<sub_map>[\w ]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	internal static Regex MapRegex = new(@"^(?<map>[\w ]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

	internal static (string Map, string? SubMap) GetMapDataFromString(this string map_data)
	{
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


	internal static (string LargeMap, string SmallMap) BuildMap(this byte[] data)
	{
		string[] baseArray = Encoding.UTF8.GetString(data).Split('\n');
		var (Map, SubMap) = baseArray[0].GetMapDataFromString();

		return (SubMap ?? Map, SubMap != null ? Map : "logo");
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
		var seperator = "-";
		var prefix = map.ToLower() switch
		{
			"lostisland" => "world00" + seperator,
			"albehir" => "world01" + seperator,
			"fenrien" => "world02" + seperator,
			"ignar" => "world03" + seperator,
			"samudra" => "world04" + seperator,
			"felliah" => "world05" + seperator,
			"wastelands" => "world06" + seperator,
			"ultima" => "world07" + seperator,
			"desktop" => "world08" + seperator,
			_ => string.Empty
		};
		return prefix;
	}
}
