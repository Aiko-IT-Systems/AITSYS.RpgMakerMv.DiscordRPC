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

using DiscordRPC.Converters;

namespace DiscordRPC.RPC.Payload
{
	/// <summary>
	/// See https://discordapp.com/developers/docs/topics/rpc#rpc-server-payloads-rpc-events for documentation
	/// </summary>
	internal enum ServerEvent
	{

		/// <summary>
		/// Sent when the server is ready to accept messages
		/// </summary>
		[EnumValue("READY")]
		Ready,

		/// <summary>
		/// Sent when something bad has happened
		/// </summary>
		[EnumValue("ERROR")]
		Error,

		/// <summary>
		/// Join Event 
		/// </summary>
		[EnumValue("ACTIVITY_JOIN")]
		ActivityJoin,

		/// <summary>
		/// Spectate Event
		/// </summary>
		[EnumValue("ACTIVITY_SPECTATE")]
		ActivitySpectate,

		/// <summary>
		/// Request Event
		/// </summary>
		[EnumValue("ACTIVITY_JOIN_REQUEST")]
		ActivityJoinRequest
	}
}
