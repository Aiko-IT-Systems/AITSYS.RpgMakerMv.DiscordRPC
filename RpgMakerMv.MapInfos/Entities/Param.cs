using Newtonsoft.Json;
namespace RpgMakerMv.MapInfos;

public class Param
{
	[JsonProperty("type")]
	public string Type;

	[JsonProperty("data")]
	public ParamData? Data;
}