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

using DiscordRPC.Message;

namespace DiscordRPC.Events
{
	/// <summary>
	/// Called when the Discord Client is ready to send and receive messages.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnReadyEvent(DiscordRpcClient sender, ReadyMessage args);

	/// <summary>
	/// Called when connection to the Discord Client is lost. The connection will remain close and unready to accept messages until the Ready event is called again.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnCloseEvent(DiscordRpcClient sender, CloseMessage args);

	/// <summary>
	/// Called when a error has occured during the transmission of a message. For example, if a bad Rich Presence payload is sent, this event will be called explaining what went wrong.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnErrorEvent(DiscordRpcClient sender, ErrorMessage args);

	/// <summary>
	/// Called when the Discord Client has updated the presence.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnPresenceUpdateEvent(DiscordRpcClient sender, PresenceMessage args);

	/// <summary>
	/// Called when the Discord Client has subscribed to an event.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnSubscribeEvent(DiscordRpcClient sender, SubscribeMessage args);

	/// <summary>
	/// Called when the Discord Client has unsubscribed from an event.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnUnsubscribeEvent(DiscordRpcClient sender, UnsubscribeMessage args);

	/// <summary>
	/// Called when the Discord Client wishes for this process to join a game.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnJoinEvent(DiscordRpcClient sender, JoinMessage args);

	/// <summary>
	/// Called when the Discord Client wishes for this process to spectate a game.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnSpectateEvent(DiscordRpcClient sender, SpectateMessage args);

	/// <summary>
	/// Called when another discord user requests permission to join this game.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnJoinRequestedEvent(DiscordRpcClient sender, JoinRequestMessage args);


	/// <summary>
	/// The connection to the discord client was succesfull. This is called before <see cref="OnReadyEvent"/>.
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnConnectionEstablishedEvent(DiscordRpcClient sender, ConnectionEstablishedMessage args);

	/// <summary>
	/// Failed to establish any connection with discord. Discord is potentially not running?
	/// </summary>
	/// <param name="sender">The Discord client handler that sent this event</param>
	/// <param name="args">The arguments supplied with the event</param>
	public delegate void OnConnectionFailedEvent(DiscordRpcClient sender, ConnectionFailedMessage args);


	/// <summary>
	/// A RPC Message is received.
	/// </summary>
	/// <param name="sender">The handler that sent this event</param>
	/// <param name="msg">The raw message from the RPC</param>
	public delegate void OnRpcMessageEvent(object sender, IMessage msg);
}
