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
	/// Representation of the message received by discord when the presence has been updated.
	/// </summary>
	public class PresenceMessage : IMessage
	{
		/// <summary>
		/// The type of message received from discord
		/// </summary>
		public override MessageType Type => MessageType.PresenceUpdate;

		internal PresenceMessage() : this(null) { }
		internal PresenceMessage(RichPresenceResponse rpr)
		{
			if (rpr == null)
			{
				this.Presence = null;
				this.Name = "No Rich Presence";
				this.ApplicationID = "";
			}
			else
			{
				this.Presence = rpr;
				this.Name = rpr.Name;
				this.ApplicationID = rpr.ClientID;
			}
		}

		/// <summary>
		/// The rich presence Discord has set
		/// </summary>
		public BaseRichPresence Presence { get; internal set; }

		/// <summary>
		/// The name of the application Discord has set it for
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// The ID of the application discord has set it for
		/// </summary>
		public string ApplicationID { get; internal set; }
	}
}
