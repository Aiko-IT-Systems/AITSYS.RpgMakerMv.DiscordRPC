﻿// This file is part of an AITSYS project.
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

namespace DiscordRPC
{
	/// <summary>
	/// The type of event receieved by the RPC. A flag type that can be combined.
	/// </summary>
	[System.Flags]
	public enum EventType
	{
		/// <summary>
		/// No event
		/// </summary>
		None = 0,

		/// <summary>
		/// Called when the Discord Client wishes to enter a game to spectate
		/// </summary>
		Spectate = 0x1,

		/// <summary>
		/// Called when the Discord Client wishes to enter a game to play.
		/// </summary>
		Join = 0x2,

		/// <summary>
		/// Called when another Discord Client has requested permission to join this game.
		/// </summary>
		JoinRequest = 0x4
	}
}
