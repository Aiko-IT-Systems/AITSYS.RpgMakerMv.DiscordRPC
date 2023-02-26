using Newtonsoft.Json;

namespace RpgMakerMv.MapInfos;

public class ParamData
{
	[JsonProperty("small_asset_key", NullValueHandling = NullValueHandling.Include)]
	public string? SmallAssetKey { get; internal set; }

	[JsonProperty("small_asset_text", NullValueHandling = NullValueHandling.Include)]
	public string? SmallAssetText { get; internal set; }

	[JsonProperty("large_asset_key", NullValueHandling = NullValueHandling.Include)]
	public string? LargeAssetKey { get; internal set; }

	[JsonProperty("large_asset_text", NullValueHandling = NullValueHandling.Include)]
	public string? LargeAssetText { get; internal set; }

	[JsonProperty("details", NullValueHandling = NullValueHandling.Include)]
	public string? Details { get; internal set; }

	public ParamData(string? sak, string? sat, string? lak, string? lat, string? d)
	{
		SmallAssetKey = sak;
		SmallAssetText = sat;
		LargeAssetKey = lak;
		LargeAssetText = lat;
		Details = d;
	}
}