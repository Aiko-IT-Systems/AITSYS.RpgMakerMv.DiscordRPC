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

namespace AITSYS.RpgMakerMv.Rpc.Entities;

public class RpcConfig
{
	[JsonProperty("use_button_one", NullValueHandling = NullValueHandling.Ignore)]
	public bool UseButtonOne { get; internal set; } = false;

	[JsonProperty("use_button_two", NullValueHandling = NullValueHandling.Ignore)]
	public bool UseButtonTwo { get; internal set; } = false;

	[JsonProperty("button_one_config", NullValueHandling = NullValueHandling.Ignore)]
	public RpcButtonConfig? ButtonOne { get; internal set; } = null;

	[JsonProperty("button_two_config", NullValueHandling = NullValueHandling.Ignore)]
	public RpcButtonConfig? ButtonTwo { get; internal set; } = null;
}
