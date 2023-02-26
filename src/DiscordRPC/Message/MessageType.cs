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

namespace DiscordRPC.Message
{
	/// <summary>
	/// Type of message.
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// The Discord Client is ready to send and receive messages.
		/// </summary>
		Ready,

		/// <summary>
		/// The connection to the Discord Client is lost. The connection will remain close and unready to accept messages until the Ready event is called again.
		/// </summary>
		Close,

		/// <summary>
		/// A error has occured during the transmission of a message. For example, if a bad Rich Presence payload is sent, this event will be called explaining what went wrong.
		/// </summary>
		Error,

		/// <summary>
		/// The Discord Client has updated the presence.
		/// </summary>
		PresenceUpdate,

		/// <summary>
		/// The Discord Client has subscribed to an event.
		/// </summary>
		Subscribe,

		/// <summary>
		/// The Discord Client has unsubscribed from an event.
		/// </summary>
		Unsubscribe,

		/// <summary>
		/// The Discord Client wishes for this process to join a game.
		/// </summary>
		Join,

		/// <summary>
		/// The Discord Client wishes for this process to spectate a game. 
		/// </summary>
		Spectate,

		/// <summary>
		/// Another discord user requests permission to join this game.
		/// </summary>
		JoinRequest,

		/// <summary>
		/// The connection to the discord client was succesfull. This is called before <see cref="Ready"/>.
		/// </summary>
		ConnectionEstablished,

		/// <summary>
		/// Failed to establish any connection with discord. Discord is potentially not running?
		/// </summary>
		ConnectionFailed
	}
}
