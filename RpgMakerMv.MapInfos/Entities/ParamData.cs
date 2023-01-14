using Newtonsoft.Json;

namespace RpgMakerMv.MapInfos;

public class ParamData
{
	[JsonProperty("small_asset_key")]
	public string SmallAssetKey { get; internal set; }

	[JsonProperty("small_asset_text")]
	public string SmallAssetText { get; internal set; }

	[JsonProperty("large_asset_key")]
	public string LargeAssetKey { get; internal set; }

	[JsonProperty("large_asset_text")]
	public string LargeAssetText { get; internal set; }

	[JsonProperty("details")]
	public string Details { get; internal set; }

	public ParamData(string sak, string sat, string lak, string lat, string d)
	{
		SmallAssetKey = sak;
		SmallAssetText = sat;
		LargeAssetKey = lak;
		LargeAssetText = lat;
		Details = d;
	}
}