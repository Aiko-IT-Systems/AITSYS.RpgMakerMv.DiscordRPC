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
using System.Collections.Generic;
using System.Threading;

using DiscordRPC.Events;
using DiscordRPC.Helper;
using DiscordRPC.IO;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using DiscordRPC.RPC.Commands;
using DiscordRPC.RPC.Payload;

using Newtonsoft.Json;

namespace DiscordRPC.RPC
{
	/// <summary>
	/// Communicates between the client and discord through RPC
	/// </summary>
	internal class RpcConnection : IDisposable
	{
		/// <summary>
		/// Version of the RPC Protocol
		/// </summary>
		public static readonly int VERSION = 1;

		/// <summary>
		/// The rate of poll to the discord pipe.
		/// </summary>
		public static readonly int POLL_RATE = 1000;

		/// <summary>
		/// Should we send a null presence on the fairwells?
		/// </summary>
		private static readonly bool s_clear_on_shutdown = true;

		/// <summary>
		/// Should we work in a lock step manner? This option is semi-obsolete and may not work as expected.
		/// </summary>
		private static readonly bool s_lock_step = false;

		/// <summary>
		/// The logger used by the RPC connection
		/// </summary>
		public ILogger Logger
		{
			get => this._logger;
			set
			{
				this._logger = value;
				if (this._namedPipe != null)
					this._namedPipe.Logger = value;
			}
		}
		private ILogger _logger;

		/// <summary>
		/// Called when a message is received from the RPC and is about to be enqueued. This is cross-thread and will execute on the RPC thread.
		/// </summary>
		public event OnRpcMessageEvent OnRpcMessage;

		#region States

		/// <summary>
		/// The current state of the RPC connection
		/// </summary>
		public RpcState State
		{
			get
			{
				lock (this._l_states)
					return this._state;
			}
		}
		private RpcState _state;
		private readonly object _l_states = new();

		/// <summary>
		/// The configuration received by the Ready
		/// </summary>
		public Configuration Configuration { get { Configuration tmp = null; lock (this._l_config) tmp = this._configuration; return tmp; } }
		private Configuration _configuration = null;
		private readonly object _l_config = new();

		private volatile bool _aborting = false;
		private volatile bool _shutdown = false;

		/// <summary>
		/// Indicates if the RPC connection is still running in the background
		/// </summary>
		public bool IsRunning => this._thread != null;

		/// <summary>
		/// Forces the <see cref="Close"/> to call <see cref="Shutdown"/> instead, safely saying goodbye to Discord. 
		/// <para>This option helps prevents ghosting in applications where the Process ID is a host and the game is executed within the host (ie: the Unity3D editor). This will tell Discord that we have no presence and we are closing the connection manually, instead of waiting for the process to terminate.</para>
		/// </summary>
		public bool ShutdownOnly { get; set; }

		#endregion

		#region Privates

		private readonly string _applicationID;                 //ID of the Discord APP
		private readonly int _processID;                            //ID of the process to track

		private long _nonce;                                //Current command index

		private Thread _thread;                         //The current thread
		private readonly INamedPipeClient _namedPipe;

		private readonly int _targetPipe;                               //The pipe to taget. Leave as -1 for any available pipe.

		private readonly object _l_rtqueue = new(); //Lock for the send queue
		private readonly uint _maxRtQueueSize;
		private readonly Queue<ICommand> _rtqueue;                  //The send queue

		private readonly object _l_rxqueue = new(); //Lock for the receive queue
		private readonly uint _maxRxQueueSize;              //The max size of the RX queue
		private readonly Queue<IMessage> _rxqueue;                   //The receive queue

		private readonly AutoResetEvent _queueUpdatedEvent = new(false);
		private readonly BackoffDelay _delay;                     //The backoff delay before reconnecting.
		#endregion

		/// <summary>
		/// Creates a new instance of the RPC.
		/// </summary>
		/// <param name="applicationID">The ID of the Discord App</param>
		/// <param name="processID">The ID of the currently running process</param>
		/// <param name="targetPipe">The target pipe to connect too</param>
		/// <param name="client">The pipe client we shall use.</param>
		/// <param name="maxRxQueueSize">The maximum size of the out queue</param>
		/// <param name="maxRtQueueSize">The maximum size of the in queue</param>
		public RpcConnection(string applicationID, int processID, int targetPipe, INamedPipeClient client, uint maxRxQueueSize = 128, uint maxRtQueueSize = 512)
		{
			this._applicationID = applicationID;
			this._processID = processID;
			this._targetPipe = targetPipe;
			this._namedPipe = client;
			this.ShutdownOnly = true;

			//Assign a default logger
			this.Logger = new ConsoleLogger();

			this._delay = new BackoffDelay(500, 60 * 1000);
			this._maxRtQueueSize = maxRtQueueSize;
			this._rtqueue = new Queue<ICommand>((int)this._maxRtQueueSize + 1);

			this._maxRxQueueSize = maxRxQueueSize;
			this._rxqueue = new Queue<IMessage>((int)this._maxRxQueueSize + 1);

			this._nonce = 0;
		}


		private long GetNextNonce()
		{
			this._nonce += 1;
			return this._nonce;
		}

		#region Queues
		/// <summary>
		/// Enqueues a command
		/// </summary>
		/// <param name="command">The command to enqueue</param>
		internal void EnqueueCommand(ICommand command)
		{
			this.Logger.Trace("Enqueue Command: {0}", command.GetType().FullName);

			//We cannot add anything else if we are aborting or shutting down.
			if (this._aborting || this._shutdown) return;

			//Enqueue the set presence argument
			lock (this._l_rtqueue)
			{
				//If we are too big drop the last element
				if (this._rtqueue.Count == this._maxRtQueueSize)
				{
					this.Logger.Error("Too many enqueued commands, dropping oldest one. Maybe you are pushing new presences to fast?");
					this._rtqueue.Dequeue();
				}

				//Enqueue the message
				this._rtqueue.Enqueue(command);
			}
		}

		/// <summary>
		/// Adds a message to the message queue. Does not copy the message, so besure to copy it yourself or dereference it.
		/// </summary>
		/// <param name="message">The message to add</param>
		private void EnqueueMessage(IMessage message)
		{
			//Invoke the message
			try
			{
				OnRpcMessage?.Invoke(this, message);
			}
			catch (Exception e)
			{
				this.Logger.Error("Unhandled Exception while processing event: {0}", e.GetType().FullName);
				this.Logger.Error(e.Message);
				this.Logger.Error(e.StackTrace);
			}

			//Small queue sizes should just ignore messages
			if (this._maxRxQueueSize <= 0)
			{
				this.Logger.Trace("Enqueued Message, but queue size is 0.");
				return;
			}

			//Large queue sizes should keep the queue in check
			this.Logger.Trace("Enqueue Message: {0}", message.Type);
			lock (this._l_rxqueue)
			{
				//If we are too big drop the last element
				if (this._rxqueue.Count == this._maxRxQueueSize)
				{
					this.Logger.Warning("Too many enqueued messages, dropping oldest one.");
					this._rxqueue.Dequeue();
				}

				//Enqueue the message
				this._rxqueue.Enqueue(message);
			}
		}

		/// <summary>
		/// Dequeues a single message from the event stack. Returns null if none are available.
		/// </summary>
		/// <returns></returns>
		internal IMessage DequeueMessage()
		{
			//Logger.Trace("Deque Message");
			lock (this._l_rxqueue)
			{
				//We have nothing, so just return null.
				if (this._rxqueue.Count == 0) return null;

				//Get the value and remove it from the list at the same time
				return this._rxqueue.Dequeue();
			}
		}

		/// <summary>
		/// Dequeues all messages from the event stack. 
		/// </summary>
		/// <returns></returns>
		internal IMessage[] DequeueMessages()
		{
			//Logger.Trace("Deque Multiple Messages");
			lock (this._l_rxqueue)
			{
				//Copy the messages into an array
				var messages = this._rxqueue.ToArray();

				//Clear the entire queue
				this._rxqueue.Clear();

				//return the array
				return messages;
			}
		}
		#endregion

		/// <summary>
		/// Main thread loop
		/// </summary>
		private void MainLoop()
		{
			//initialize the pipe
			this.Logger.Info("RPC Connection Started");
			if (this.Logger.Level <= LogLevel.Trace)
			{
				this.Logger.Trace("============================");
				this.Logger.Trace("Assembly:             " + System.Reflection.Assembly.GetAssembly(typeof(RichPresence)).FullName);
				this.Logger.Trace("Pipe:                 " + this._namedPipe.GetType().FullName);
				this.Logger.Trace("Platform:             " + Environment.OSVersion.ToString());
				this.Logger.Trace("applicationID:        " + this._applicationID);
				this.Logger.Trace("targetPipe:           " + this._targetPipe);
				this.Logger.Trace("POLL_RATE:            " + POLL_RATE);
				this.Logger.Trace("_maxRtQueueSize:      " + this._maxRtQueueSize);
				this.Logger.Trace("_maxRxQueueSize:      " + this._maxRxQueueSize);
				this.Logger.Trace("============================");
			}

			//Forever trying to connect unless the abort signal is sent
			//Keep Alive Loop
			while (!this._aborting && !this._shutdown)
			{
				try
				{
					//Wrap everything up in a try get
					//Dispose of the pipe if we have any (could be broken)
					if (this._namedPipe == null)
					{
						this.Logger.Error("Something bad has happened with our pipe client!");
						this._aborting = true;
						return;
					}

					//Connect to a new pipe
					this.Logger.Trace("Connecting to the pipe through the {0}", this._namedPipe.GetType().FullName);
					if (this._namedPipe.Connect(this._targetPipe))
					{
						#region Connected
						//We connected to a pipe! Reset the delay
						this.Logger.Trace("Connected to the pipe. Attempting to establish handshake...");
						this.EnqueueMessage(new ConnectionEstablishedMessage() { ConnectedPipe = this._namedPipe.ConnectedPipe });

						//Attempt to establish a handshake
						this.EstablishHandshake();
						this.Logger.Trace("Connection Established. Starting reading loop...");

						//Continously iterate, waiting for the frame
						//We want to only stop reading if the inside tells us (mainloop), if we are aborting (abort) or the pipe disconnects
						// We dont want to exit on a shutdown, as we still have information
						var mainloop = true;
						while (mainloop && !this._aborting && !this._shutdown && this._namedPipe.IsConnected)
						{
							#region Read Loop

							//Iterate over every frame we have queued up, processing its contents
							if (this._namedPipe.ReadFrame(out var frame))
							{
								#region Read Payload
								this.Logger.Trace("Read Payload: {0}", frame.Opcode);

								//Do some basic processing on the frame
								switch (frame.Opcode)
								{
									//We have been told by discord to close, so we will consider it an abort
									case Opcode.Close:

										var close = frame.GetObject<ClosePayload>();
										this.Logger.Warning("We have been told to terminate by discord: ({0}) {1}", close.Code, close.Reason);
										this.EnqueueMessage(new CloseMessage() { Code = close.Code, Reason = close.Reason });
										mainloop = false;
										break;

									//We have pinged, so we will flip it and respond back with pong
									case Opcode.Ping:
										this.Logger.Trace("PING");
										frame.Opcode = Opcode.Pong;
										this._namedPipe.WriteFrame(frame);
										break;

									//We have ponged? I have no idea if Discord actually sends ping/pongs.
									case Opcode.Pong:
										this.Logger.Trace("PONG");
										break;

									//A frame has been sent, we should deal with that
									case Opcode.Frame:
										if (this._shutdown)
										{
											//We are shutting down, so skip it
											this.Logger.Warning("Skipping frame because we are shutting down.");
											break;
										}

										if (frame.Data == null)
										{
											//We have invalid data, thats not good.
											this.Logger.Error("We received no data from the frame so we cannot get the event payload!");
											break;
										}

										//We have a frame, so we are going to process the payload and add it to the stack
										EventPayload response = null;
										try { response = frame.GetObject<EventPayload>(); }
										catch (Exception e)
										{
											this.Logger.Error("Failed to parse event! {0}", e.Message);
											this.Logger.Error("Data: {0}", frame.Message);
										}


										try { if (response != null) this.ProcessFrame(response); }
										catch (Exception e)
										{
											this.Logger.Error("Failed to process event! {0}", e.Message);
											this.Logger.Error("Data: {0}", frame.Message);
										}

										break;


									default:
									case Opcode.Handshake:
										//We have a invalid opcode, better terminate to be safe
										this.Logger.Error("Invalid opcode: {0}", frame.Opcode);
										mainloop = false;
										break;
								}

								#endregion
							}

							if (!this._aborting && this._namedPipe.IsConnected)
							{
								//Process the entire command queue we have left
								this.ProcessCommandQueue();

								//Wait for some time, or until a command has been queued up
								this._queueUpdatedEvent.WaitOne(POLL_RATE);
							}

							#endregion
						}
						#endregion

						this.Logger.Trace("Left main read loop for some reason. Aborting: {0}, Shutting Down: {1}", this._aborting, this._shutdown);
					}
					else
					{
						this.Logger.Error("Failed to connect for some reason.");
						this.EnqueueMessage(new ConnectionFailedMessage() { FailedPipe = _targetPipe });
					}

					//If we are not aborting, we have to wait a bit before trying to connect again
					if (!this._aborting && !this._shutdown)
					{
						//We have disconnected for some reason, either a failed pipe or a bad reading,
						// so we are going to wait a bit before doing it again
						long sleep = this._delay.NextDelay();

						this.Logger.Trace("Waiting {0}ms before attempting to connect again", sleep);
						Thread.Sleep(this._delay.NextDelay());
					}
				}
				//catch(InvalidPipeException e)
				//{
				//	Logger.Error("Invalid Pipe Exception: {0}", e.Message);
				//}
				catch (Exception e)
				{
					this.Logger.Error("Unhandled Exception: {0}", e.GetType().FullName);
					this.Logger.Error(e.Message);
					this.Logger.Error(e.StackTrace);
				}
				finally
				{
					//Disconnect from the pipe because something bad has happened. An exception has been thrown or the main read loop has terminated.
					if (this._namedPipe.IsConnected)
					{
						//Terminate the pipe
						this.Logger.Trace("Closing the named pipe.");
						this._namedPipe.Close();
					}

					//Update our state
					this.SetConnectionState(RpcState.Disconnected);
				}
			}

			//We have disconnected, so dispose of the thread and the pipe.
			this.Logger.Trace("Left Main Loop");
			this._namedPipe?.Dispose();

			this.Logger.Info("Thread Terminated, no longer performing RPC connection.");
		}

		#region Reading

		/// <summary>Handles the response from the pipe and calls appropriate events and changes states.</summary>
		/// <param name="response">The response received by the server.</param>
		private void ProcessFrame(EventPayload response)
		{
			this.Logger.Info("Handling Response. Cmd: {0}, Event: {1}", response.Command, response.Event);

			//Check if it is an error
			if (response.Event.HasValue && response.Event.Value == ServerEvent.Error)
			{
				//We have an error
				this.Logger.Error("Error received from the RPC");

				//Create the event objetc and push it to the queue
				var err = response.GetObject<ErrorMessage>();
				this.Logger.Error("Server responded with an error message: ({0}) {1}", err.Code.ToString(), err.Message);

				//Enqueue the messsage and then end
				this.EnqueueMessage(err);
				return;
			}

			//Check if its a handshake
			if (this.State == RpcState.Connecting)
			{
				if (response.Command == Command.Dispatch && response.Event.HasValue && response.Event.Value == ServerEvent.Ready)
				{
					this.Logger.Info("Connection established with the RPC");
					this.SetConnectionState(RpcState.Connected);
					this._delay.Reset();

					//Prepare the object
					var ready = response.GetObject<ReadyMessage>();
					lock (this._l_config)
					{
						this._configuration = ready.Configuration;
						ready.User.SetConfiguration(this._configuration);
					}

					//Enqueue the message
					this.EnqueueMessage(ready);
					return;
				}
			}

			if (this.State == RpcState.Connected)
			{
				switch (response.Command)
				{
					//We were sent a dispatch, better process it
					case Command.Dispatch:
						this.ProcessDispatch(response);
						break;

					//We were sent a Activity Update, better enqueue it
					case Command.SetActivity:
						if (response.Data == null)
						{
							this.EnqueueMessage(new PresenceMessage());
						}
						else
						{
							var rp = response.GetObject<RichPresenceResponse>();
							this.EnqueueMessage(new PresenceMessage(rp));
						}
						break;

					case Command.Unsubscribe:
					case Command.Subscribe:

						//Prepare a serializer that can account for snake_case enums.
						var serializer = new JsonSerializer();
						serializer.Converters.Add(new Converters.EnumSnakeCaseConverter());

						//Go through the data, looking for the evt property, casting it to a server event
						var evt = response.GetObject<EventPayload>().Event.Value;

						//Enqueue the appropriate message.
						if (response.Command == Command.Subscribe)
							this.EnqueueMessage(new SubscribeMessage(evt));
						else
							this.EnqueueMessage(new UnsubscribeMessage(evt));

						break;


					case Command.SendActivityJoinInvite:
						this.Logger.Trace("Got invite response ack.");
						break;

					case Command.CloseActivityJoinRequest:
						this.Logger.Trace("Got invite response reject ack.");
						break;

					//we have no idea what we were sent
					default:
						this.Logger.Error("Unkown frame was received! {0}", response.Command);
						return;
				}
				return;
			}

			this.Logger.Trace("Received a frame while we are disconnected. Ignoring. Cmd: {0}, Event: {1}", response.Command, response.Event);
		}

		private void ProcessDispatch(EventPayload response)
		{
			if (response.Command != Command.Dispatch) return;
			if (!response.Event.HasValue) return;

			switch (response.Event.Value)
			{
				//We are to join the server
				case ServerEvent.ActivitySpectate:
					var spectate = response.GetObject<SpectateMessage>();
					this.EnqueueMessage(spectate);
					break;

				case ServerEvent.ActivityJoin:
					var join = response.GetObject<JoinMessage>();
					this.EnqueueMessage(join);
					break;

				case ServerEvent.ActivityJoinRequest:
					var request = response.GetObject<JoinRequestMessage>();
					this.EnqueueMessage(request);
					break;

				//Unkown dispatch event received. We should just ignore it.
				default:
					this.Logger.Warning("Ignoring {0}", response.Event.Value);
					break;
			}
		}

		#endregion

		#region Writting

		private void ProcessCommandQueue()
		{
			//Logger.Info("Checking command queue");

			//We are not ready yet, dont even try
			if (this.State != RpcState.Connected)
				return;

			//We are aborting, so we will just log a warning so we know this is probably only going to send the CLOSE
			if (this._aborting)
				this.Logger.Warning("We have been told to write a queue but we have also been aborted.");

			//Prepare some variabels we will clone into with locks
			var needsWriting = true;
			ICommand item = null;

			//Continue looping until we dont need anymore messages
			while (needsWriting && this._namedPipe.IsConnected)
			{
				lock (this._l_rtqueue)
				{
					//Pull the value and update our writing needs
					// If we have nothing to write, exit the loop
					needsWriting = this._rtqueue.Count > 0;
					if (!needsWriting) break;

					//Peek at the item
					item = this._rtqueue.Peek();
				}

				//BReak out of the loop as soon as we send this item
				if (this._shutdown || (!this._aborting && s_lock_step))
					needsWriting = false;

				//Prepare the payload
				var payload = item.PreparePayload(this.GetNextNonce());
				this.Logger.Trace("Attempting to send payload: {0}", payload.Command);

				//Prepare the frame
				var frame = new PipeFrame();
				if (item is CloseCommand)
				{
					//We have been sent a close frame. We better just send a handwave
					//Send it off to the server
					this.SendHandwave();

					//Queue the item
					this.Logger.Trace("Handwave sent, ending queue processing.");
					lock (this._l_rtqueue) this._rtqueue.Dequeue();

					//Stop sending any more messages
					return;
				}
				else
				{
					if (this._aborting)
					{
						//We are aborting, so just dequeue the message and dont bother sending it
						this.Logger.Warning("- skipping frame because of abort.");
						lock (this._l_rtqueue) this._rtqueue.Dequeue();
					}
					else
					{
						//Prepare the frame
						frame.SetObject(Opcode.Frame, payload);

						//Write it and if it wrote perfectly fine, we will dequeue it
						this.Logger.Trace("Sending payload: {0}", payload.Command);
						if (this._namedPipe.WriteFrame(frame))
						{
							//We sent it, so now dequeue it
							this.Logger.Trace("Sent Successfully.");
							lock (this._l_rtqueue) this._rtqueue.Dequeue();
						}
						else
						{
							//Something went wrong, so just giveup and wait for the next time around.
							this.Logger.Warning("Something went wrong during writing!");
							return;
						}
					}
				}
			}
		}

		#endregion

		#region Connection

		/// <summary>
		/// Establishes the handshake with the server. 
		/// </summary>
		/// <returns></returns>
		private void EstablishHandshake()
		{
			this.Logger.Trace("Attempting to establish a handshake...");

			//We are establishing a lock and not releasing it until we sent the handshake message.
			// We need to set the key, and it would not be nice if someone did things between us setting the key.

			//Check its state
			if (this.State != RpcState.Disconnected)
			{
				this.Logger.Error("State must be disconnected in order to start a handshake!");
				return;
			}

			//Send it off to the server
			this.Logger.Trace("Sending Handshake...");
			if (!this._namedPipe.WriteFrame(new PipeFrame(Opcode.Handshake, new Handshake() { Version = VERSION, ClientID = _applicationID })))
			{
				this.Logger.Error("Failed to write a handshake.");
				return;
			}

			//This has to be done outside the lock
			this.SetConnectionState(RpcState.Connecting);
		}

		/// <summary>
		/// Establishes a fairwell with the server by sending a handwave.
		/// </summary>
		private void SendHandwave()
		{
			this.Logger.Info("Attempting to wave goodbye...");

			//Check its state
			if (this.State == RpcState.Disconnected)
			{
				this.Logger.Error("State must NOT be disconnected in order to send a handwave!");
				return;
			}

			//Send the handwave
			if (!this._namedPipe.WriteFrame(new PipeFrame(Opcode.Close, new Handshake() { Version = VERSION, ClientID = _applicationID })))
			{
				this.Logger.Error("failed to write a handwave.");
				return;
			}
		}


		/// <summary>
		/// Attempts to connect to the pipe. Returns true on success
		/// </summary>
		/// <returns></returns>
		public bool AttemptConnection()
		{
			this.Logger.Info("Attempting a new connection");

			//The thread mustn't exist already
			if (this._thread != null)
			{
				this.Logger.Error("Cannot attempt a new connection as the previous connection thread is not null!");
				return false;
			}

			//We have to be in the disconnected state
			if (this.State != RpcState.Disconnected)
			{
				this.Logger.Warning("Cannot attempt a new connection as the previous connection hasn't changed state yet.");
				return false;
			}

			if (this._aborting)
			{
				this.Logger.Error("Cannot attempt a new connection while aborting!");
				return false;
			}

			//Start the thread up
			this._thread = new Thread(this.MainLoop)
			{
				Name = "Discord IPC Thread",
				IsBackground = true
			};
			this._thread.Start();

			return true;
		}

		/// <summary>
		/// Sets the current state of the pipe, locking the l_states object for thread saftey.
		/// </summary>
		/// <param name="state">The state to set it too.</param>
		private void SetConnectionState(RpcState state)
		{
			this.Logger.Trace("Setting the connection state to {0}", state.ToString().ToSnakeCase().ToUpperInvariant());
			lock (this._l_states)
			{
				this._state = state;
			}
		}

		/// <summary>
		/// Closes the connection and disposes of resources. This will not force termination, but instead allow Discord disconnect us after we say goodbye. 
		/// <para>This option helps prevents ghosting in applications where the Process ID is a host and the game is executed within the host (ie: the Unity3D editor). This will tell Discord that we have no presence and we are closing the connection manually, instead of waiting for the process to terminate.</para>
		/// </summary>
		public void Shutdown()
		{
			//Enable the flag
			this.Logger.Trace("Initiated shutdown procedure");
			this._shutdown = true;

			//Clear the commands and enqueue the close
			lock (this._l_rtqueue)
			{
				this._rtqueue.Clear();
				if (s_clear_on_shutdown) this._rtqueue.Enqueue(new PresenceCommand() { PID = _processID, Presence = null });
				this._rtqueue.Enqueue(new CloseCommand());
			}

			//Trigger the event
			this._queueUpdatedEvent.Set();
		}

		/// <summary>
		/// Closes the connection and disposes of resources.
		/// </summary>
		public void Close()
		{
			if (this._thread == null)
			{
				this.Logger.Error("Cannot close as it is not available!");
				return;
			}

			if (this._aborting)
			{
				this.Logger.Error("Cannot abort as it has already been aborted");
				return;
			}

			//Set the abort state
			if (this.ShutdownOnly)
			{
				this.Shutdown();
				return;
			}

			//Terminate
			this.Logger.Trace("Updating Abort State...");
			this._aborting = true;
			this._queueUpdatedEvent.Set();
		}


		/// <summary>
		/// Closes the connection and disposes resources. Identical to <see cref="Close"/> but ignores the "ShutdownOnly" value.
		/// </summary>
		public void Dispose()
		{
			this.ShutdownOnly = false;
			this.Close();
		}
		#endregion

	}

	/// <summary>
	/// State of the RPC connection
	/// </summary>
	internal enum RpcState
	{
		/// <summary>
		/// Disconnected from the discord client
		/// </summary>
		Disconnected,

		/// <summary>
		/// Connecting to the discord client. The handshake has been sent and we are awaiting the ready event
		/// </summary>
		Connecting,

		/// <summary>
		/// We are connect to the client and can send and receive messages.
		/// </summary>
		Connected
	}
}
