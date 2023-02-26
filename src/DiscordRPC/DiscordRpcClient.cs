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

using System;

using DiscordRPC.Events;
using DiscordRPC.Exceptions;
using DiscordRPC.IO;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using DiscordRPC.Registry;
using DiscordRPC.RPC;
using DiscordRPC.RPC.Commands;

namespace DiscordRPC
{

	/// <summary>
	/// A Discord RPC Client which is used to send Rich Presence updates and receive Join / Spectate events.
	/// </summary>
	public sealed class DiscordRpcClient : IDisposable
	{
		#region Properties


		/// <summary>
		/// Gets a value indicating if the client has registered a URI Scheme. If this is false, Join / Spectate events will fail.
		/// <para>To register a URI Scheme, call <see cref="RegisterUriScheme(string, string)"/>.</para>
		/// </summary>
		public bool HasRegisteredUriScheme { get; private set; }

		/// <summary>
		/// Gets the Application ID of the RPC Client.
		/// </summary>
		public string ApplicationID { get; private set; }

		/// <summary>
		/// Gets the Steam ID of the RPC Client. This value can be null if none was supplied.
		/// </summary>
		public string SteamID { get; private set; }

		/// <summary>
		/// Gets the ID of the process used to run the RPC Client. Discord tracks this process ID and waits for its termination. Defaults to the current application process ID.
		/// </summary>
		public int ProcessID { get; private set; }

		/// <summary>
		/// The maximum size of the message queue received from Discord. 
		/// </summary>
		public int MaxQueueSize { get; private set; }

		/// <summary>
		/// The dispose state of the client object.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// The logger used this client and its associated components. <see cref="ILogger"/> are not called safely and can come from any thread. It is upto the <see cref="ILogger"/> to account for this and apply appropriate thread safe methods.
		/// </summary>
		public ILogger Logger
		{
			get => this._logger;
			set
			{
				this._logger = value;
				if (this._connection != null) this._connection.Logger = value;
			}
		}
		private ILogger _logger;

		/// <summary>
		/// Indicates if the client will automatically invoke the events without <see cref="Invoke"/> having to be called. 
		/// </summary>
		public bool AutoEvents { get; private set; }

		/// <summary>
		/// Skips sending presences that are identical to the current one.
		/// </summary>
		public bool SkipIdenticalPresence { get; set; }
		#endregion

		/// <summary>
		/// The pipe the discord client is on, ranging from 0 to 9. Use -1 to scan through all pipes.
		/// <para>This property can be used for testing multiple clients. For example, if a Discord Client was on pipe 0, the Discord Canary is most likely on pipe 1.</para>
		/// </summary>
		public int TargetPipe { get; private set; }

		private readonly RpcConnection _connection;

		/// <summary>
		/// The current presence that the client has. Gets set with <see cref="SetPresence(RichPresence)"/> and updated on <see cref="OnPresenceUpdate"/>.
		/// </summary>
		public RichPresence CurrentPresence { get; private set; }

		/// <summary>
		/// Current subscription to events. Gets set with <see cref="Subscribe(EventType)"/>, <see cref="UnsubscribeMessage"/> and updated on <see cref="OnSubscribe"/>, <see cref="OnUnsubscribe"/>.
		/// </summary>
		public EventType Subscription { get; private set; }

		/// <summary>
		/// The current discord user. This is updated with the ready event and will be null until the event is fired from the connection.
		/// </summary>
		public User CurrentUser { get; private set; }

		/// <summary>
		/// The current configuration the connection is using. Only becomes available after a ready event.
		/// </summary>
		public Configuration Configuration { get; private set; }

		/// <summary>
		/// Represents if the client has been <see cref="Initialize"/>
		/// </summary>
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// Forces the connection to shutdown gracefully instead of just aborting the connection.
		/// <para>This option helps prevents ghosting in applications where the Process ID is a host and the game is executed within the host (ie: the Unity3D editor). This will tell Discord that we have no presence and we are closing the connection manually, instead of waiting for the process to terminate.</para>
		/// </summary>
		public bool ShutdownOnly
		{
			get => this._shutdownOnly;
			set
			{
				this._shutdownOnly = value;
				if (this._connection != null) this._connection.ShutdownOnly = value;
			}
		}
		private bool _shutdownOnly = true;
		private readonly object _sync = new();

		#region Events

		/// <summary>
		/// Called when the discord client is ready to send and receive messages.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnReadyEvent OnReady;

		/// <summary>
		/// Called when connection to the Discord Client is lost. The connection will remain close and unready to accept messages until the Ready event is called again.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnCloseEvent OnClose;

		/// <summary>
		/// Called when a error has occured during the transmission of a message. For example, if a bad Rich Presence payload is sent, this event will be called explaining what went wrong.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnErrorEvent OnError;

		/// <summary>
		/// Called when the Discord Client has updated the presence.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnPresenceUpdateEvent OnPresenceUpdate;

		/// <summary>
		/// Called when the Discord Client has subscribed to an event.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnSubscribeEvent OnSubscribe;

		/// <summary>
		/// Called when the Discord Client has unsubscribed from an event.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnUnsubscribeEvent OnUnsubscribe;

		/// <summary>
		/// Called when the Discord Client wishes for this process to join a game.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnJoinEvent OnJoin;

		/// <summary>
		/// Called when the Discord Client wishes for this process to spectate a game.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnSpectateEvent OnSpectate;

		/// <summary>
		/// Called when another discord user requests permission to join this game.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnJoinRequestedEvent OnJoinRequested;

		/// <summary>
		/// The connection to the discord client was succesfull. This is called before <see cref="MessageType.Ready"/>.
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnConnectionEstablishedEvent OnConnectionEstablished;

		/// <summary>
		/// Failed to establish any connection with discord. Discord is potentially not running?
		/// <para>If <see cref="AutoEvents"/> is true then this event will execute on a different thread. If it is not true however, then this event is not invoked untill <see cref="Invoke"/> and will be on the calling thread.</para>
		/// </summary>
		public event OnConnectionFailedEvent OnConnectionFailed;

		/// <summary>
		/// The RPC Connection has sent a message. Called before any other event and executed from the RPC Thread.
		/// </summary>
		public event OnRpcMessageEvent OnRpcMessage;
		#endregion

		#region Initialization

		/// <summary>
		/// Creates a new Discord RPC Client which can be used to send Rich Presence and receive Join / Spectate events.
		/// </summary>
		/// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
		public DiscordRpcClient(string applicationID) : this(applicationID, -1) { }

		/// <summary>
		/// Creates a new Discord RPC Client which can be used to send Rich Presence and receive Join / Spectate events. This constructor exposes more advance features such as custom NamedPipeClients and Loggers.
		/// </summary>
		/// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
		/// <param name="pipe">The pipe to connect too. If -1, then the client will scan for the first available instance of Discord.</param>
		/// <param name="logger">The logger used to report messages. If null, then a <see cref="NullLogger"/> will be created and logs will be ignored.</param>
		/// <param name="autoEvents">Should events be automatically invoked from the RPC Thread as they arrive from discord?</param>
		/// <param name="client">The pipe client to use and communicate to discord through. If null, the default <see cref="ManagedNamedPipeClient"/> will be used.</param>
		public DiscordRpcClient(string applicationID, int pipe = -1, ILogger logger = null, bool autoEvents = true, INamedPipeClient client = null)
		{
			//Make sure appID is NOT null.
			if (string.IsNullOrEmpty(applicationID))
				throw new ArgumentNullException(nameof(applicationID));

			//Store the properties
			this.ApplicationID = applicationID.Trim();
			this.TargetPipe = pipe;
			this.ProcessID = Environment.ProcessId;
			this.HasRegisteredUriScheme = false;
			this.AutoEvents = autoEvents;
			this.SkipIdenticalPresence = true;

			//Prepare the logger
			this._logger = logger ?? new NullLogger();

			//Create the RPC client, giving it the important details
			this._connection = new RpcConnection(this.ApplicationID, this.ProcessID, this.TargetPipe, client ?? new ManagedNamedPipeClient(), autoEvents ? 0 : 128U)
			{
				ShutdownOnly = _shutdownOnly,
				Logger = _logger
			};

			//Subscribe to its event
			this._connection.OnRpcMessage += (sender, msg) =>
			{
				OnRpcMessage?.Invoke(this, msg);

				if (this.AutoEvents)
					this.ProcessMessage(msg);
			};
		}

		#endregion

		#region Message Handling
		/// <summary>
		/// Dequeues all the messages from Discord, processes them and then invoke appropriate event handlers. This will process the message and update the internal state before invoking the events. Returns the messages that were invoked in the order they were invoked.
		/// <para>This method cannot be used if <see cref="AutoEvents"/> is enabled.</para>
		/// </summary>
		/// <returns>Returns the messages that were invoked and in the order they were invoked.</returns>
		public IMessage[] Invoke()
		{
			if (this.AutoEvents)
			{
				this.Logger.Error("Cannot Invoke client when AutomaticallyInvokeEvents has been set.");
				return Array.Empty<IMessage>();
				//throw new InvalidOperationException("Cannot Invoke client when AutomaticallyInvokeEvents has been set.");
			}

			//Dequeue all the messages and process them
			var messages = this._connection.DequeueMessages();
			for (var i = 0; i < messages.Length; i++)
			{
				//Do a bit of pre-processing
				var message = messages[i];
				this.ProcessMessage(message);
			}

			//Finally, return the messages
			return messages;
		}

		/// <summary>
		/// Processes the message, updating our internal state and then invokes the events.
		/// </summary>
		/// <param name="message"></param>
		private void ProcessMessage(IMessage message)
		{
			if (message == null) return;
			switch (message.Type)
			{
				//We got a update, so we will update our current presence
				case MessageType.PresenceUpdate:
					lock (this._sync)
					{
						if (message is PresenceMessage pm)
						{
							//We need to merge these presences together
							if (pm.Presence == null)
							{
								this.CurrentPresence = null;
							}
							else if (this.CurrentPresence == null)
							{
								this.CurrentPresence = new RichPresence().Merge(pm.Presence);
							}
							else
							{
								this.CurrentPresence.Merge(pm.Presence);
							}

							//Update the message
							pm.Presence = this.CurrentPresence;
						}
					}

					OnPresenceUpdate?.Invoke(this, message as PresenceMessage);

					break;

				//Update our configuration
				case MessageType.Ready:
					if (message is ReadyMessage rm)
					{
						lock (this._sync)
						{
							this.Configuration = rm.Configuration;
							this.CurrentUser = rm.User;
						}

						//Resend our presence and subscription
						this.SynchronizeState();
					}

					OnReady?.Invoke(this, message as ReadyMessage);

					break;

				case MessageType.Close:
					OnClose?.Invoke(this, message as CloseMessage);
					break;

				case MessageType.Error:
					OnError?.Invoke(this, message as ErrorMessage);
					break;

				//Update the request's CDN for the avatar helpers
				case MessageType.JoinRequest:
					if (this.Configuration != null)
					{
						//Update the User object within the join request if the current Cdn
						var jrm = message as JoinRequestMessage;
						jrm?.User.SetConfiguration(this.Configuration);
					}
					OnJoinRequested?.Invoke(this, message as JoinRequestMessage);
					break;

				case MessageType.Subscribe:
					lock (this._sync)
					{
						var sub = message as SubscribeMessage;
						this.Subscription |= sub.Event;
					}

					OnSubscribe?.Invoke(this, message as SubscribeMessage);

					break;

				case MessageType.Unsubscribe:
					lock (this._sync)
					{
						var unsub = message as UnsubscribeMessage;
						this.Subscription &= ~unsub.Event;
					}

					OnUnsubscribe?.Invoke(this, message as UnsubscribeMessage);

					break;

				case MessageType.Join:
					OnJoin?.Invoke(this, message as JoinMessage);
					break;

				case MessageType.Spectate:
					OnSpectate?.Invoke(this, message as SpectateMessage);
					break;

				case MessageType.ConnectionEstablished:
					OnConnectionEstablished?.Invoke(this, message as ConnectionEstablishedMessage);
					break;

				case MessageType.ConnectionFailed:
					OnConnectionFailed?.Invoke(this, message as ConnectionFailedMessage);
					break;

				//We got a message we dont know what to do with.
				default:
					this.Logger.Error("Message was queued with no appropriate handle! {0}", message.Type);
					break;
			}
		}
		#endregion

		/// <summary>
		/// Respond to a Join Request. All requests will timeout after 30 seconds.
		/// <para>Because of the 30 second timeout, it is recommended to call <seealso cref="Invoke"/> faster than every 15 seconds to give your users adequate time to respond to the request.</para>
		/// </summary>
		/// <param name="request">The request that is being responded too.</param>
		/// <param name="acceptRequest">Accept the join request.</param>
		public void Respond(JoinRequestMessage request, bool acceptRequest)
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (this._connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			if (!this.IsInitialized)
				throw new UninitializedException();

			this._connection.EnqueueCommand(new RespondCommand() { Accept = acceptRequest, UserID = request.User.ID.ToString() });
		}

		/// <summary>
		/// Sets the Rich Presence.
		/// </summary>
		/// <param name="presence">The Rich Presence to set on the current Discord user.</param>
		public void SetPresence(RichPresence presence)
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (this._connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			if (!this.IsInitialized)
				this.Logger.Warning("The client is not yet initialized, storing the presence as a state instead.");

			//Send the event
			if (presence == null)
			{
				//Clear the presence
				if (!this.SkipIdenticalPresence || this.CurrentPresence != null)
					this._connection.EnqueueCommand(new PresenceCommand() { PID = this.ProcessID, Presence = null });
			}
			else
			{
				//Send valid presence
				//Validate the presence with our settings
				if (presence.HasSecrets() && !this.HasRegisteredUriScheme)
					throw new BadPresenceException("Cannot send a presence with secrets as this object has not registered a URI scheme. Please enable the uri scheme registration in the DiscordRpcClient constructor.");

				if (presence.HasParty() && presence.Party.Max < presence.Party.Size)
					throw new BadPresenceException("Presence maximum party size cannot be smaller than the current size.");

				if (presence.HasSecrets() && !presence.HasParty())
					this.Logger.Warning("The presence has set the secrets but no buttons will show as there is no party available.");

				//Send the presence, but only if we are not skipping
				if (!this.SkipIdenticalPresence || !presence.Matches(this.CurrentPresence))
					this._connection.EnqueueCommand(new PresenceCommand() { PID = this.ProcessID, Presence = presence.Clone() });
			}

			//Update our local store
			lock (this._sync)
			{
				this.CurrentPresence = presence?.Clone();
			}
		}

		#region Updates

		/// <summary>
		/// Updates only the <see cref="RichPresence.Buttons"/> of the <see cref="CurrentPresence"/> and updates/removes the buttons. Returns the newly edited Rich Presence.
		/// </summary>
		/// <param name="button">The buttons of the Rich Presence</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateButtons(Button[] button = null)
		{
			if (!this.IsInitialized)
			{
				throw new UninitializedException();
			}

			// Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			// Update the buttons.
			presence.Buttons = button;
			this.SetPresence(presence);

			return presence;
		}

		/// <summary>
		/// Updates only the <see cref="RichPresence.Buttons"/> of the <see cref="CurrentPresence"/> and updates the button with the given index. Returns the newly edited Rich Presence.
		/// </summary>
		/// <param name="button">The buttons of the Rich Presence</param>
		/// <param name="index">The number of the button</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence SetButton(Button button, int index = 0)
		{
			if (!this.IsInitialized)
			{
				throw new UninitializedException();
			}

			// Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			// Update the buttons
			presence.Buttons[index] = button;
			this.SetPresence(presence);

			return presence;
		}

		/// <summary>
		/// Updates only the <see cref="BaseRichPresence.Details"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Returns the newly edited Rich Presence.
		/// </summary>
		/// <param name="details">The details of the Rich Presence</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateDetails(string details)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Details = details;
			this.SetPresence(presence);
			return presence;
		}
		/// <summary>
		/// Updates only the <see cref="BaseRichPresence.State"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Returns the newly edited Rich Presence.
		/// </summary>
		/// <param name="state">The state of the Rich Presence</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateState(string state)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.State = state;
			this.SetPresence(presence);
			return presence;
		}
		/// <summary>
		/// Updates only the <see cref="BaseRichPresence.Party"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Returns the newly edited Rich Presence.
		/// </summary>
		/// <param name="party">The party of the Rich Presence</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateParty(Party party)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Party = party;
			this.SetPresence(presence);
			return presence;
		}
		/// <summary>
		/// Updates the <see cref="Party.Size"/> of the <see cref="CurrentPresence"/> and sends the update presence to Discord. Returns the newly edited Rich Presence.
		/// <para>Will return null if no presence exists and will throw a new <see cref="NullReferenceException"/> if the Party does not exist.</para>
		/// </summary>
		/// <param name="size">The new size of the party. It cannot be greater than <see cref="Party.Max"/></param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdatePartySize(int size)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Ensure it has a party
			if (presence.Party == null)
				throw new BadPresenceException("Cannot set the size of the party if the party does not exist");

			//Update the value 
			presence.Party.Size = size;
			this.SetPresence(presence);
			return presence;
		}

		/// <summary>
		/// Updates the <see cref="Party.Size"/> of the <see cref="CurrentPresence"/> and sends the update presence to Discord. Returns the newly edited Rich Presence.
		/// <para>Will return null if no presence exists and will throw a new <see cref="NullReferenceException"/> if the Party does not exist.</para>
		/// </summary>
		/// <param name="size">The new size of the party. It cannot be greater than <see cref="Party.Max"/></param>
		/// <param name="max">The new size of the party. It cannot be smaller than <see cref="Party.Size"/></param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdatePartySize(int size, int max)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Ensure it has a party
			if (presence.Party == null)
				throw new BadPresenceException("Cannot set the size of the party if the party does not exist");

			//Update the value 
			presence.Party.Size = size;
			presence.Party.Max = max;
			this.SetPresence(presence);
			return presence;
		}

		/// <summary>
		/// Updates the large <see cref="Assets"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Both <paramref name="key"/> and <paramref name="tooltip"/> are optional and will be ignored it null.
		/// </summary>
		/// <param name="key">Optional: The new key to set the asset too</param>
		/// <param name="tooltip">Optional: The new tooltip to display on the asset</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateLargeAsset(string key = null, string tooltip = null)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();
			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Assets ??= new Assets();
			presence.Assets.LargeImageKey = key ?? presence.Assets.LargeImageKey;
			presence.Assets.LargeImageText = tooltip ?? presence.Assets.LargeImageText;
			this.SetPresence(presence);
			return presence;
		}

		/// <summary>
		/// Updates the small <see cref="Assets"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Both <paramref name="key"/> and <paramref name="tooltip"/> are optional and will be ignored it null.
		/// </summary>
		/// <param name="key">Optional: The new key to set the asset too</param>
		/// <param name="tooltip">Optional: The new tooltip to display on the asset</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateSmallAsset(string key = null, string tooltip = null)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();
			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Assets ??= new Assets();
			presence.Assets.SmallImageKey = key ?? presence.Assets.SmallImageKey;
			presence.Assets.SmallImageText = tooltip ?? presence.Assets.SmallImageText;
			this.SetPresence(presence);
			return presence;
		}

		/// <summary>
		/// Updates the <see cref="Secrets"/> of the <see cref="CurrentPresence"/> and sends the updated presence to Discord. Will override previous secret entirely.
		/// </summary>
		/// <param name="secrets">The new secret to send to discord.</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateSecrets(Secrets secrets)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Secrets = secrets;
			this.SetPresence(presence);
			return presence;
		}

		/// <summary>
		/// Sets the start time of the <see cref="CurrentPresence"/> to now and sends the updated presence to Discord.
		/// </summary>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateStartTime() => this.UpdateStartTime(DateTime.UtcNow);

		/// <summary>
		/// Sets the start time of the <see cref="CurrentPresence"/> and sends the updated presence to Discord.
		/// </summary>
		/// <param name="time">The new time for the start</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateStartTime(DateTime time)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Timestamps ??= new Timestamps();
			presence.Timestamps.Start = time;
			this.SetPresence(presence);
			return presence;
		}

		/// <summary>
		/// Sets the end time of the <see cref="CurrentPresence"/> to now and sends the updated presence to Discord.
		/// </summary>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateEndTime() => this.UpdateEndTime(DateTime.UtcNow);

		/// <summary>
		/// Sets the end time of the <see cref="CurrentPresence"/> and sends the updated presence to Discord.
		/// </summary>
		/// <param name="time">The new time for the end</param>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateEndTime(DateTime time)
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Timestamps ??= new Timestamps();
			presence.Timestamps.End = time;
			this.SetPresence(presence);
			return presence;
		}

		/// <summary>
		/// Sets the start and end time of <see cref="CurrentPresence"/> to null and sends it to Discord.
		/// </summary>
		/// <returns>Updated Rich Presence</returns>
		public RichPresence UpdateClearTime()
		{
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Clone the presence
			RichPresence presence;
			lock (this._sync)
			{
				presence = this.CurrentPresence == null ? new RichPresence() : this.CurrentPresence.Clone();
			}

			//Update the value 
			presence.Timestamps = null;
			this.SetPresence(presence);
			return presence;
		}
		#endregion

		/// <summary>
		/// Clears the Rich Presence. Use this just before disposal to prevent ghosting.
		/// </summary>
		public void ClearPresence()
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (!this.IsInitialized)
				throw new UninitializedException();

			if (this._connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			//Just a wrapper function for sending null
			this.SetPresence(null);
		}

		#region Subscriptions

		/// <summary>
		/// Registers the application executable to a custom URI Scheme.
		/// <para>This is required for the Join and Spectate features. Discord will run this custom URI Scheme to launch your application when a user presses either of the buttons.</para>
		/// </summary>
		/// <param name="steamAppID">Optional Steam ID. If supplied, Discord will launch the game through steam instead of directly calling it.</param>
		/// <param name="executable">The path to the executable. If null, the path to the current executable will be used instead.</param>
		/// <returns></returns>
		public bool RegisterUriScheme(string steamAppID = null, string executable = null)
		{
			var urischeme = new UriSchemeRegister(this._logger, this.ApplicationID, steamAppID, executable);
			return this.HasRegisteredUriScheme = urischeme.RegisterUriScheme();
		}

		/// <summary>
		/// Subscribes to an event sent from discord. Used for Join / Spectate feature.
		/// <para>Requires the UriScheme to be registered.</para>
		/// </summary>
		/// <param name="type">The event type to subscribe to</param>
		public void Subscribe(EventType type) => this.SetSubscription(this.Subscription | type);

		/// <summary>
		/// Unsubscribe from the event sent by discord. Used for Join / Spectate feature.
		/// <para>Requires the UriScheme to be registered.</para>
		/// </summary>
		/// <param name="type">The event type to unsubscribe from</param>
		public void Unsubscribe(EventType type) => this.SetSubscription(this.Subscription & ~type);

		/// <summary>
		/// Sets the subscription to the events sent from Discord.
		/// <para>Requires the UriScheme to be registered.</para>
		/// </summary>
		/// <param name="type">The new subscription as a flag. Events selected in the flag will be subscribed too and the other events will be unsubscribed.</param>
		public void SetSubscription(EventType type)
		{
			if (this.IsInitialized)
			{
				//Calculate what needs to be unsubscrinbed
				this.SubscribeToTypes(this.Subscription & ~type, true);
				this.SubscribeToTypes(~this.Subscription & type, false);
			}
			else
			{
				this.Logger.Warning("Client has not yet initialized, but events are being subscribed too. Storing them as state instead.");
			}

			lock (this._sync)
			{
				this.Subscription = type;
			}
		}

		/// <summary>
		/// Simple helper function that will subscribe to the specified types in the flag.
		/// </summary>
		/// <param name="type">The flag to subscribe to</param>
		/// <param name="isUnsubscribe">Represents if the unsubscribe payload should be sent instead.</param>
		private void SubscribeToTypes(EventType type, bool isUnsubscribe)
		{
			//Because of SetSubscription, this can actually be none as there is no differences. 
			//If that is the case, we should just stop here
			if (type == EventType.None) return;

			//We cannot do anything if we are disposed or missing our connection.
			if (this.IsDisposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (!this.IsInitialized)
				throw new UninitializedException();

			if (this._connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			//We dont have the Uri Scheme registered, we should throw a exception to tell the user.
			if (!this.HasRegisteredUriScheme)
				throw new InvalidConfigurationException("Cannot subscribe/unsubscribe to an event as this application has not registered a URI Scheme. Call RegisterUriScheme().");

			//Add the subscribe command to be sent when the connection is able too
			if ((type & EventType.Spectate) == EventType.Spectate)
				this._connection.EnqueueCommand(new SubscribeCommand() { Event = RPC.Payload.ServerEvent.ActivitySpectate, IsUnsubscribe = isUnsubscribe });

			if ((type & EventType.Join) == EventType.Join)
				this._connection.EnqueueCommand(new SubscribeCommand() { Event = RPC.Payload.ServerEvent.ActivityJoin, IsUnsubscribe = isUnsubscribe });

			if ((type & EventType.JoinRequest) == EventType.JoinRequest)
				this._connection.EnqueueCommand(new SubscribeCommand() { Event = RPC.Payload.ServerEvent.ActivityJoinRequest, IsUnsubscribe = isUnsubscribe });
		}

		#endregion

		/// <summary>
		/// Resends the current presence and subscription. This is used when Ready is called to keep the current state within discord.
		/// </summary>
		public void SynchronizeState()
		{
			//Cannot sync over uninitialized connection
			if (!this.IsInitialized)
				throw new UninitializedException();

			//Set the presence and if we have registered the uri scheme, resubscribe.
			this.SetPresence(this.CurrentPresence);
			if (this.HasRegisteredUriScheme)
				this.SubscribeToTypes(this.Subscription, false);
		}

		/// <summary>
		/// Attempts to initalize a connection to the Discord IPC.
		/// </summary>
		/// <returns></returns>
		public bool Initialize()
		{
			return this.IsDisposed
				? throw new ObjectDisposedException("Discord IPC Client")
				: this.IsInitialized
				? throw new UninitializedException("Cannot initialize a client that is already initialized")
				: this._connection == null
				? throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized")
				: (this.IsInitialized = this._connection.AttemptConnection());
		}

		/// <summary>
		/// Attempts to disconnect and deinitialize the IPC connection while retaining the settings.
		/// </summary>
		public void Deinitialize()
		{
			if (!this.IsInitialized)
				throw new UninitializedException("Cannot deinitialize a client that has not been initalized.");

			this._connection.Close();
			this.IsInitialized = false;
		}

		/// <summary>
		/// Terminates the connection to Discord and disposes of the object.
		/// </summary>
		public void Dispose()
		{
			if (this.IsDisposed) return;
			if (this.IsInitialized) this.Deinitialize();
			this.IsDisposed = true;
		}

	}
}
