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
	/// The possible commands that can be sent and received by the server.
	/// </summary>
	internal enum Command
	{
		/// <summary>
		/// event dispatch
		/// </summary>
		[EnumValue("DISPATCH")]
		Dispatch,

		/// <summary>
		/// Called to set the activity
		/// </summary>
		[EnumValue("SET_ACTIVITY")]
		SetActivity,

		/// <summary>
		/// used to subscribe to an RPC event
		/// </summary>
		[EnumValue("SUBSCRIBE")]
		Subscribe,

		/// <summary>
		/// used to unsubscribe from an RPC event
		/// </summary>
		[EnumValue("UNSUBSCRIBE")]
		Unsubscribe,

		/// <summary>
		/// Used to accept join requests.
		/// </summary>
		[EnumValue("SEND_ACTIVITY_JOIN_INVITE")]
		SendActivityJoinInvite,

		/// <summary>
		/// Used to reject join requests.
		/// </summary>
		[EnumValue("CLOSE_ACTIVITY_JOIN_REQUEST")]
		CloseActivityJoinRequest
	}
}
