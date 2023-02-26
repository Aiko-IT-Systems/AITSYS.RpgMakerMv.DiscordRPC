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

namespace DiscordRPC.Message
{
	/// <summary>
	/// Created when a error occurs within the ipc and it is sent to the client.
	/// </summary>
	public class ErrorMessage : IMessage
	{
		/// <summary>
		/// The type of message received from discord
		/// </summary>
		public override MessageType Type => MessageType.Error;

		/// <summary>
		/// The Discord error code.
		/// </summary>
		[JsonProperty("code")]
		public ErrorCode Code { get; internal set; }

		/// <summary>
		/// The message associated with the error code.
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; internal set; }

	}

	/// <summary>
	/// The error message received by discord. See https://discordapp.com/developers/docs/topics/rpc#rpc-server-payloads-rpc-errors for documentation
	/// </summary>
	public enum ErrorCode
	{
		//Pipe Error Codes
		/// <summary> Pipe was Successful </summary>
		Success = 0,

		///<summary>The pipe had an exception</summary>
		PipeException = 1,

		///<summary>The pipe received corrupted data</summary>
		ReadCorrupt = 2,

		//Custom Error Code
		///<summary>The functionality was not yet implemented</summary>
		NotImplemented = 10,

		//Discord RPC error codes
		///<summary>Unkown Discord error</summary>
		UnkownError = 1000,

		///<summary>Invalid Payload received</summary>
		InvalidPayload = 4000,

		///<summary>Invalid command was sent</summary>
		InvalidCommand = 4002,

		/// <summary>Invalid event was sent </summary>
		InvalidEvent = 4004,

		/*
		InvalidGuild = 4003,
		InvalidChannel = 4005,
		InvalidPermissions = 4006,
		InvalidClientID = 4007,
		InvalidOrigin = 4008,
		InvalidToken = 4009,
		InvalidUser = 4010,
		OAuth2Error = 5000,
		SelectChannelTimeout = 5001,
		GetGuildTimeout = 5002,
		SelectVoiceForceRequired = 5003,
		CaptureShortcutAlreadyListening = 5004
		*/
	}
}
