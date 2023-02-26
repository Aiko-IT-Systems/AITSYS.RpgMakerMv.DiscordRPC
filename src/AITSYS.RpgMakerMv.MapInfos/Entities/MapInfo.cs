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

using Newtonsoft.Json;

namespace AITSYS.RpgMakerMv.MapInfos.Entities;

public class MapInfo
{
	[JsonProperty("id")]
	public int? Id { get; internal set; }

	[JsonProperty("expanded")]
	public bool? Expanded { get; internal set; }

	[JsonProperty("name")]
	public string Name { get; internal set; }

	[JsonProperty("order")]
	public int? Order { get; internal set; }

	[JsonProperty("parentId")]
	public int? ParentId { get; internal set; }

	[JsonProperty("scrollX")]
	public double? ScrollX { get; internal set; }

	[JsonProperty("scrollY")]
	public double? ScrollY { get; internal set; }

	[JsonProperty("note")]
	public string Note { get; internal set; }

	[JsonProperty("params")]
	public List<Param> Params { get; internal set; } = new();

	[JsonProperty("damage")]
	public DamageConfig Damage { get; internal set; }
}
