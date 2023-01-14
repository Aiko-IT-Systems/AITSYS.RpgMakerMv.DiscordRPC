using Newtonsoft.Json;
namespace RpgMakerMv.MapInfos;

public class MapInfo
{
	[JsonProperty("id")]
	public int? Id;

	[JsonProperty("expanded")]
	public bool? Expanded;

	[JsonProperty("name")]
	public string Name;

	[JsonProperty("order")]
	public int? Order;

	[JsonProperty("parentId")]
	public int? ParentId;

	[JsonProperty("scrollX")]
	public double? ScrollX;

	[JsonProperty("scrollY")]
	public double? ScrollY;

	[JsonProperty("note")]
	public string Note;

	[JsonProperty("params")]
	public List<Param> Params = new();

	[JsonProperty("damage")]
	public DamageConfig Damage;
}