using Newtonsoft.Json;
namespace RpgMakerMv.MapInfos;

public class DamageConfig
{
	[JsonProperty("critical")]
	public string Critical;

	[JsonProperty("elementId")]
	public string ElementId;

	[JsonProperty("formula")]
	public string Formula;

	[JsonProperty("type")]
	public string Type;

	[JsonProperty("variance")]
	public string Variance;
}