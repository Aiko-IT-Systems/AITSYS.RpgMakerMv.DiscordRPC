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
using System.Text;

using DiscordRPC.Exceptions;
using DiscordRPC.Helper;

using Newtonsoft.Json;

namespace DiscordRPC
{
	/// <summary>
	/// The base rich presence structure
	/// </summary>
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	[Serializable]
	public class BaseRichPresence
	{
		/// <summary>
		/// The user's current <see cref="Party"/> status. For example, "Playing Solo" or "With Friends".
		/// <para>Max 128 bytes</para>
		/// </summary>
		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public string State
		{
			get => this._state;
			set
			{
				if (!ValidateString(value, out this._state, 128, Encoding.UTF8))
					throw new StringOutOfRangeException("State", 0, 128);
			}
		}

		/// <summary>Inernal inner state string</summary>
		protected internal string _state;

		/// <summary>
		/// What the user is currently doing. For example, "Competitive - Total Mayhem".
		/// <para>Max 128 bytes</para>
		/// </summary>
		[JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
		public string Details
		{
			get => this._details;
			set
			{
				if (!ValidateString(value, out this._details, 128, Encoding.UTF8))
					throw new StringOutOfRangeException(128);
			}
		}
		/// <summary>Inernal inner detail string</summary>
		protected internal string _details;

		/// <summary>
		/// The time elapsed / remaining time data.
		/// </summary>
		[JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
		public Timestamps Timestamps { get; set; }

		/// <summary>
		/// The names of the images to use and the tooltips to give those images.
		/// </summary>
		[JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
		public Assets Assets { get; set; }

		/// <summary>
		/// The party the player is currently in. The <see cref="Party.ID"/> must be set for this to be included in the RichPresence update.
		/// </summary>
		[JsonProperty("party", NullValueHandling = NullValueHandling.Ignore)]
		public Party Party { get; set; }

		/// <summary>
		/// The secrets used for Join / Spectate. Secrets are obfuscated data of your choosing. They could be match ids, player ids, lobby ids, etc. Make this object null if you do not wish too / unable too implement the Join / Request feature.
		/// <para>To keep security on the up and up, Discord requires that you properly hash/encode/encrypt/put-a-padlock-on-and-swallow-the-key-but-wait-then-how-would-you-open-it your secrets.</para>
		/// <para>Visit the <see href="https://discordapp.com/developers/docs/rich-presence/how-to#secrets">Rich Presence How-To</see> for more information.</para>
		/// </summary>
		[JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
		public Secrets Secrets { get; set; }

		#region Has Checks
		/// <summary>
		/// Does the Rich Presence have valid timestamps?
		/// </summary>
		/// <returns></returns>
		public bool HasTimestamps() => this.Timestamps != null && (this.Timestamps.Start != null || this.Timestamps.End != null);

		/// <summary>
		/// Does the Rich Presence have valid assets?
		/// </summary>
		/// <returns></returns>
		public bool HasAssets() => this.Assets != null;

		/// <summary>
		/// Does the Rich Presence have a valid party?
		/// </summary>
		/// <returns></returns>
		public bool HasParty() => this.Party != null && this.Party.ID != null;

		/// <summary>
		/// Does the Rich Presence have valid secrets?
		/// </summary>
		/// <returns></returns>
		public bool HasSecrets() => this.Secrets != null && (this.Secrets.JoinSecret != null || this.Secrets.SpectateSecret != null);

		#endregion


		/// <summary>
		/// Attempts to call <see cref="StringTools.GetNullOrString(string)"/> on the string and return the result, if its within a valid length.
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <param name="result">The formatted string result</param>
		/// <param name="bytes">The maximum number of bytes the string can take up</param>
		/// <param name="encoding">The encoding to count the bytes with</param>
		/// <returns>True if the string fits within the number of bytes</returns>
		internal static bool ValidateString(string str, out string result, int bytes, Encoding encoding)
		{
			result = str;
			if (str == null)
				return true;

			//Trim the string, for the best chance of fitting
			var s = str.Trim();

			//Make sure it fits
			if (!s.WithinLength(bytes, encoding))
				return false;

			//Make sure its not empty
			result = s.GetNullOrString();
			return true;
		}

		/// <summary>
		/// Operator that converts a presence into a boolean for null checks.
		/// </summary>
		/// <param name="presesnce"></param>
		public static implicit operator bool(BaseRichPresence presesnce) => presesnce != null;

		/// <summary>
		/// Checks if the other rich presence differs from the current one
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		internal virtual bool Matches(RichPresence other)
		{
			if (other == null)
				return false;

			if (this.State != other.State || this.Details != other.Details)
				return false;

			//Checks if the timestamps are different
			if (this.Timestamps != null)
			{
				if (other.Timestamps == null ||
					other.Timestamps.StartUnixMilliseconds != this.Timestamps.StartUnixMilliseconds ||
					other.Timestamps.EndUnixMilliseconds != this.Timestamps.EndUnixMilliseconds)
					return false;
			}
			else if (other.Timestamps != null)
			{
				return false;
			}

			//Checks if the secrets are different
			if (this.Secrets != null)
			{
				if (other.Secrets == null ||
					other.Secrets.JoinSecret != this.Secrets.JoinSecret ||
					other.Secrets.SpectateSecret != this.Secrets.SpectateSecret)
					return false;
			}
			else if (other.Secrets != null)
			{
				return false;
			}

			//Checks if the timestamps are different
			if (this.Party != null)
			{
				if (other.Party == null ||
					other.Party.ID != this.Party.ID ||
					other.Party.Max != this.Party.Max ||
					other.Party.Size != this.Party.Size ||
					other.Party.Privacy != this.Party.Privacy)
					return false;
			}
			else if (other.Party != null)
			{
				return false;
			}

			//Checks if the assets are different
			if (this.Assets != null)
			{
				if (other.Assets == null ||
					other.Assets.LargeImageKey != this.Assets.LargeImageKey ||
					other.Assets.LargeImageText != this.Assets.LargeImageText ||
					other.Assets.SmallImageKey != this.Assets.SmallImageKey ||
					other.Assets.SmallImageText != this.Assets.SmallImageText)
					return false;
			}
			else if (other.Assets != null)
			{
				return false;
			}

			return this == other;
		}

		/// <summary>
		/// Converts this BaseRichPresence to RichPresence
		/// </summary>
		/// <returns></returns>
		public RichPresence ToRichPresence()
		{
			var presence = new RichPresence
			{
				State = this.State,
				Details = this.Details,

				Party = !this.HasParty() ? this.Party : null,
				Secrets = !this.HasSecrets() ? this.Secrets : null
			};

			if (this.HasAssets())
			{
				presence.Assets = new Assets()
				{
					SmallImageKey = this.Assets.SmallImageKey,
					SmallImageText = this.Assets.SmallImageText,

					LargeImageKey = this.Assets.LargeImageKey,
					LargeImageText = this.Assets.LargeImageText
				};
			}

			if (this.HasTimestamps())
			{
				presence.Timestamps = new Timestamps();
				if (this.Timestamps.Start.HasValue) presence.Timestamps.Start = this.Timestamps.Start;
				if (this.Timestamps.End.HasValue) presence.Timestamps.End = this.Timestamps.End;
			}

			return presence;
		}
	}

	/// <summary>
	/// The secrets used for Join / Spectate. Secrets are obfuscated data of your choosing. They could be match ids, player ids, lobby ids, etc.
	/// <para>To keep security on the up and up, Discord requires that you properly hash/encode/encrypt/put-a-padlock-on-and-swallow-the-key-but-wait-then-how-would-you-open-it your secrets.</para>
	/// <para>You should send discord data that someone else's game client would need to join or spectate their friend. If you can't or don't want to support those actions, you don't need to send secrets.</para>
	/// <para>Visit the <see href="https://discordapp.com/developers/docs/rich-presence/how-to#secrets">Rich Presence How-To</see> for more information.</para>
	/// </summary>
	[Serializable]
	public class Secrets
	{
		/// <summary>
		/// The secret data that will tell the client how to connect to the game to play. This could be a unique identifier for a fancy match maker or player id, lobby id, etc.
		/// <para>It is recommended to encrypt this information so its hard for people to replicate it. 
		/// Do <b>NOT</b> just use the IP address in this. That is a bad practice and can leave your players vulnerable!
		/// </para>
		/// <para>Max Length of 128 Bytes</para>
		/// </summary>
		[JsonProperty("join", NullValueHandling = NullValueHandling.Ignore)]
		public string JoinSecret
		{
			get => this._joinSecret;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._joinSecret, 128, Encoding.UTF8))
					throw new StringOutOfRangeException(128);
			}
		}
		private string _joinSecret;

		/// <summary>
		/// The secret data that will tell the client how to connect to the game to spectate. This could be a unique identifier for a fancy match maker or player id, lobby id, etc.
		/// <para>It is recommended to encrypt this information so its hard for people to replicate it. 
		/// Do <b>NOT</b> just use the IP address in this. That is a bad practice and can leave your players vulnerable!
		/// </para>
		/// <para>Max Length of 128 Bytes</para>
		/// </summary>
		[JsonProperty("spectate", NullValueHandling = NullValueHandling.Ignore)]
		public string SpectateSecret
		{
			get => this._spectateSecret;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._spectateSecret, 128, Encoding.UTF8))
					throw new StringOutOfRangeException(128);
			}
		}
		private string _spectateSecret;


		#region Statics

		/// <summary>
		/// The encoding the secret generator is using
		/// </summary>
		public static Encoding Encoding => Encoding.UTF8;

		/// <summary>
		/// The length of a secret in bytes.
		/// </summary>
		public static int SecretLength => 128;

		/// <summary>
		/// Creates a new secret. This is NOT a cryptographic function and should NOT be used for sensitive information. This is mainly provided as a way to generate quick IDs.
		/// </summary>
		/// <param name="random">The random to use</param>
		/// <returns>Returns a <see cref="SecretLength"/> sized string with random characters from <see cref="Encoding"/></returns>
		public static string CreateSecret(Random random)
		{
			//Prepare an array and fill it with random bytes
			// THIS IS NOT SECURE! DO NOT USE THIS FOR PASSWORDS!
			var bytes = new byte[SecretLength];
			random.NextBytes(bytes);

			//Return the encoding. Probably should remove invalid characters but cannot be fucked.
			return Encoding.GetString(bytes);
		}


		/// <summary>
		/// Creates a secret word using more readable friendly characters. Useful for debugging purposes. This is not a cryptographic function and should NOT be used for sensitive information.
		/// </summary>
		/// <param name="random">The random used to generate the characters</param>
		/// <returns></returns>
		public static string CreateFriendlySecret(Random random)
		{
			var charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			var builder = new StringBuilder();
			for (var i = 0; i < SecretLength; i++)
				builder.Append(charset[random.Next(charset.Length)]);

			return builder.ToString();
		}
		#endregion
	}

	/// <summary>
	/// Information about the pictures used in the Rich Presence.
	/// </summary>
	[Serializable]
	public class Assets
	{
		/// <summary>
		/// Name of the uploaded image for the large profile artwork.
		/// <para>Max 256 Bytes.</para>
		/// </summary>
		[JsonProperty("large_image", NullValueHandling = NullValueHandling.Ignore)]
		public string LargeImageKey
		{
			get => this._largeimagekey;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._largeimagekey, 256, Encoding.UTF8))
					throw new StringOutOfRangeException(256);

				//Get if this is a external link
				this._islargeimagekeyexternal = this._largeimagekey?.StartsWith("mp:external/") ?? false;

				//Reset the large image ID
				this._largeimageID = null;
			}
		}
		private string _largeimagekey;

		/// <summary>
		/// Gets if the large square image is from an external link
		/// </summary>
		[JsonIgnore]
		public bool IsLargeImageKeyExternal => this._islargeimagekeyexternal;
		private bool _islargeimagekeyexternal;

		/// <summary>
		/// The tooltip for the large square image. For example, "Summoners Rift" or "Horizon Lunar Colony".
		/// <para>Max 128 Bytes.</para>
		/// </summary>
		[JsonProperty("large_text", NullValueHandling = NullValueHandling.Ignore)]
		public string LargeImageText
		{
			get => this._largeimagetext;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._largeimagetext, 128, Encoding.UTF8))
					throw new StringOutOfRangeException(128);
			}
		}
		private string _largeimagetext;


		/// <summary>
		/// Name of the uploaded image for the small profile artwork.
		/// <para>Max 256 Bytes.</para>
		/// </summary>
		[JsonProperty("small_image", NullValueHandling = NullValueHandling.Ignore)]
		public string SmallImageKey
		{
			get => this._smallimagekey;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._smallimagekey, 256, Encoding.UTF8))
					throw new StringOutOfRangeException(256);

				//Get if this is a external link
				this._issmallimagekeyexternal = this._smallimagekey?.StartsWith("mp:external/") ?? false;

				//Reset the small image id
				this._smallimageID = null;
			}
		}
		private string _smallimagekey;

		/// <summary>
		/// Gets if the small profile artwork is from an external link
		/// </summary>
		[JsonIgnore]
		public bool IsSmallImageKeyExternal => this._issmallimagekeyexternal;
		private bool _issmallimagekeyexternal;

		/// <summary>
		/// The tooltip for the small circle image. For example, "LvL 6" or "Ultimate 85%".
		/// <para>Max 128 Bytes.</para>
		/// </summary>
		[JsonProperty("small_text", NullValueHandling = NullValueHandling.Ignore)]
		public string SmallImageText
		{
			get => this._smallimagetext;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._smallimagetext, 128, Encoding.UTF8))
					throw new StringOutOfRangeException(128);
			}
		}
		private string _smallimagetext;

		/// <summary>
		/// The ID of the large image. This is only set after Update Presence and will automatically become null when <see cref="LargeImageKey"/> is changed.
		/// </summary>
		[JsonIgnore]
		public ulong? LargeImageID => this._largeimageID;
		private ulong? _largeimageID;

		/// <summary>
		/// The ID of the small image. This is only set after Update Presence and will automatically become null when <see cref="SmallImageKey"/> is changed.
		/// </summary>
		[JsonIgnore]
		public ulong? SmallImageID => this._smallimageID;
		private ulong? _smallimageID;

		/// <summary>
		/// Merges this asset with the other, taking into account for ID's instead of keys.
		/// </summary>
		/// <param name="other"></param>
		internal void Merge(Assets other)
		{
			//Copy over the names
			this._smallimagetext = other._smallimagetext;
			this._largeimagetext = other._largeimagetext;

			//Convert large ID
			if (ulong.TryParse(other._largeimagekey, out var largeID))
			{
				this._largeimageID = largeID;
			}
			else
			{
				this._largeimagekey = other._largeimagekey;
				this._largeimageID = null;
			}

			//Convert the small ID
			if (ulong.TryParse(other._smallimagekey, out var smallID))
			{
				this._smallimageID = smallID;
			}
			else
			{
				this._smallimagekey = other._smallimagekey;
				this._smallimageID = null;
			}
		}
	}

	/// <summary>
	/// Structure representing the start and endtimes of a match.
	/// </summary>
	[Serializable]
	public class Timestamps
	{
		/// <summary>A new timestamp that starts from the current time.</summary>
		public static Timestamps Now => new(DateTime.UtcNow);

		/// <summary>
		/// Creates a new timestamp starting at the current time and ending in the supplied timespan
		/// </summary>
		/// <param name="seconds">How long the Timestamp will last for in seconds.</param>
		/// <returns>Returns a new timestamp with given duration.</returns>
		public static Timestamps FromTimeSpan(double seconds) => FromTimeSpan(TimeSpan.FromSeconds(seconds));

		/// <summary>
		/// Creates a new timestamp starting at current time and ending in the supplied timespan
		/// </summary>
		/// <param name="timespan">How long the Timestamp will last for.</param>
		/// <returns>Returns a new timestamp with given duration.</returns>
		public static Timestamps FromTimeSpan(TimeSpan timespan)
		{
			return new Timestamps()
			{
				Start = DateTime.UtcNow,
				End = DateTime.UtcNow + timespan
			};
		}

		/// <summary>
		/// The time that match started. When included (not-null), the time in the rich presence will be shown as "00:01 elapsed".
		/// </summary>
		[JsonIgnore]
		public DateTime? Start { get; set; }

		/// <summary>
		/// The time the match will end. When included (not-null), the time in the rich presence will be shown as "00:01 remaining". This will override the "elapsed" to "remaining".
		/// </summary>
		[JsonIgnore]
		public DateTime? End { get; set; }

		/// <summary>
		/// Creates a empty timestamp object
		/// </summary>
		public Timestamps()
		{
			this.Start = null;
			this.End = null;
		}

		/// <summary>
		/// Creates a timestamp with the set start time
		/// </summary>
		/// <param name="start"></param>
		public Timestamps(DateTime start)
		{
			this.Start = start;
			this.End = null;
		}

		/// <summary>
		/// Creates a timestamp with a set duration
		/// </summary>
		/// <param name="start">The start time</param>
		/// <param name="end">The end time</param>
		public Timestamps(DateTime start, DateTime end)
		{
			this.Start = start;
			this.End = end;
		}

		/// <summary>
		/// Converts between DateTime and Milliseconds to give the Unix Epoch Time for the <see cref="Timestamps.Start"/>.
		/// </summary>
		[JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
		public ulong? StartUnixMilliseconds
		{
			get => this.Start.HasValue ? ToUnixMilliseconds(this.Start.Value) : null;

			set => this.Start = value.HasValue ? FromUnixMilliseconds(value.Value) : null;
		}


		/// <summary>
		/// Converts between DateTime and Milliseconds to give the Unix Epoch Time  for the <see cref="Timestamps.End"/>.
		/// <seealso cref="End"/>
		/// </summary>
		[JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
		public ulong? EndUnixMilliseconds
		{
			get => this.End.HasValue ? ToUnixMilliseconds(this.End.Value) : null;

			set => this.End = value.HasValue ? FromUnixMilliseconds(value.Value) : null;
		}

		/// <summary>
		/// Converts a Unix Epoch time into a <see cref="DateTime"/>.
		/// </summary>
		/// <param name="unixTime">The time in milliseconds since 1970 / 01 / 01</param>
		/// <returns></returns>
		public static DateTime FromUnixMilliseconds(ulong unixTime)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddMilliseconds(Convert.ToDouble(unixTime));
		}

		/// <summary>
		/// Converts a <see cref="DateTime"/> into a Unix Epoch time (in milliseconds).
		/// </summary>
		/// <param name="date">The datetime to convert</param>
		/// <returns></returns>
		public static ulong ToUnixMilliseconds(DateTime date)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToUInt64((date - epoch).TotalMilliseconds);
		}

	}

	/// <summary>
	/// Structure representing the part the player is in.
	/// </summary>
	[Serializable]
	public class Party
	{
		/// <summary>
		/// Privacy of the party
		/// </summary>
		public enum PrivacySetting
		{
			/// <summary>
			/// The party is private, invites only.
			/// </summary>
			Private = 0,

			/// <summary>
			/// THe party is public, anyone can join.
			/// </summary>
			Public = 1
		}

		/// <summary>
		/// A unique ID for the player's current party / lobby / group. If this is not supplied, they player will not be in a party and the rest of the information will not be sent. 
		/// <para>Max 128 Bytes</para>
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get => this._partyid; set => this._partyid = value.GetNullOrString(); }
		private string _partyid;

		/// <summary>
		/// The current size of the players party / lobby / group.
		/// </summary>
		[JsonIgnore]
		public int Size { get; set; }

		/// <summary>
		/// The maxium size of the party / lobby / group. This is required to be larger than <see cref="Size"/>. If it is smaller than the current party size, it will automatically be set too <see cref="Size"/> when the presence is sent.
		/// </summary>
		[JsonIgnore]
		public int Max { get; set; }

		/// <summary>
		/// The privacy of the party
		/// </summary>
		[JsonProperty("privacy", NullValueHandling = NullValueHandling.Include, DefaultValueHandling = DefaultValueHandling.Include)]
		public PrivacySetting Privacy { get; set; }


		[JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "No direct rf")]
		private int[] SIZE
		{
			get
			{
				//see issue https://github.com/discordapp/discord-rpc/issues/111
				var size = Math.Max(1, this.Size);
				return new int[] { size, Math.Max(size, this.Max) };
			}

			set
			{
				if (value.Length != 2)
				{
					this.Size = 0;
					this.Max = 0;
				}
				else
				{
					this.Size = value[0];
					this.Max = value[1];
				}
			}

		}
	}

	/// <summary>
	/// A Rich Presence button.
	/// </summary>
	public class Button
	{
		/// <summary>
		/// Text shown on the button
		/// <para>Max 32 bytes.</para>
		/// </summary>
		[JsonProperty("label")]
		public string Label
		{
			get => this._label;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._label, 32, Encoding.UTF8))
					throw new StringOutOfRangeException(32);
			}
		}
		private string _label;

		/// <summary>
		/// The URL opened when clicking the button.
		/// <para>Max 512 bytes.</para>
		/// </summary>
		[JsonProperty("url")]
		public string Url
		{
			get => this._url;
			set
			{
				if (!BaseRichPresence.ValidateString(value, out this._url, 512, Encoding.UTF8))
					throw new StringOutOfRangeException(512);

				if (!Uri.TryCreate(this._url, UriKind.Absolute, out var uriResult)) // || !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
					throw new ArgumentException("Url must be a valid URI");
			}
		}
		private string _url;
	}

	/// <summary>
	/// The Rich Presence structure that will be sent and received by Discord. Use this class to build your presence and update it appropriately.
	/// </summary>
	// This is broken up in this way because the response inherits the BaseRichPresence.
	public sealed class RichPresence : BaseRichPresence
	{
		/// <summary>
		/// The buttons to display in the presence. 
		/// <para>Max of 2</para>
		/// </summary>
		[JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
		public Button[] Buttons { get; set; }

		/// <summary>
		/// Does the Rich Presence have any buttons?
		/// </summary>
		/// <returns></returns>
		public bool HasButtons() => this.Buttons != null && this.Buttons.Length > 0;


		#region Builder
		/// <summary>
		/// Sets the state of the Rich Presence. See also <seealso cref="BaseRichPresence.State"/>.
		/// </summary>
		/// <param name="state">The user's current <see cref="Party"/> status.</param>
		/// <returns>The modified Rich Presence.</returns>
		public RichPresence WithState(string state)
		{
			this.State = state;
			return this;
		}

		/// <summary>
		/// Sets the details of the Rich Presence. See also <seealso cref="BaseRichPresence.Details"/>.
		/// </summary>
		/// <param name="details">What the user is currently doing.</param>
		/// <returns>The modified Rich Presence.</returns>
		public RichPresence WithDetails(string details)
		{
			this.Details = details;
			return this;
		}

		/// <summary>
		/// Sets the timestamp of the Rich Presence. See also <seealso cref="Timestamps"/>.
		/// </summary>
		/// <param name="timestamps">The time elapsed / remaining time data.</param>
		/// <returns>The modified Rich Presence.</returns>
		public RichPresence WithTimestamps(Timestamps timestamps)
		{
			this.Timestamps = timestamps;
			return this;
		}

		/// <summary>
		/// Sets the assets of the Rich Presence. See also <seealso cref="Assets"/>.
		/// </summary>
		/// <param name="assets">The names of the images to use and the tooltips to give those images.</param>
		/// <returns>The modified Rich Presence.</returns>
		public RichPresence WithAssets(Assets assets)
		{
			this.Assets = assets;
			return this;
		}

		/// <summary>
		/// Sets the Rich Presence's party. See also <seealso cref="Party"/>.
		/// </summary>
		/// <param name="party">The party the player is currently in.</param>
		/// <returns>The modified Rich Presence.</returns>
		public RichPresence WithParty(Party party)
		{
			this.Party = party;
			return this;
		}

		/// <summary>
		/// Sets the Rich Presence's secrets. See also <seealso cref="Secrets"/>.
		/// </summary>
		/// <param name="secrets">The secrets used for Join / Spectate.</param>
		/// <returns>The modified Rich Presence.</returns>
		public RichPresence WithSecrets(Secrets secrets)
		{
			this.Secrets = secrets;
			return this;
		}
		#endregion


		#region Cloning and Merging 

		/// <summary>
		/// Clones the presence into a new instance. Used for thread safe writing and reading. This function will ignore properties if they are in a invalid state.
		/// </summary>
		/// <returns></returns>
		public RichPresence Clone()
		{
			return new RichPresence
			{
				State = this._state != null ? this._state.Clone() as string : null,
				Details = this._details != null ? this._details.Clone() as string : null,

				Buttons = !this.HasButtons() ? null : this.Buttons.Clone() as Button[],
				Secrets = !this.HasSecrets() ? null : new Secrets
				{
					//MatchSecret = this.Secrets.MatchSecret?.Clone() as string,
					JoinSecret = this.Secrets.JoinSecret != null ? this.Secrets.JoinSecret.Clone() as string : null,
					SpectateSecret = this.Secrets.SpectateSecret != null ? this.Secrets.SpectateSecret.Clone() as string : null
				},

				Timestamps = !this.HasTimestamps() ? null : new Timestamps
				{
					Start = this.Timestamps.Start,
					End = this.Timestamps.End
				},

				Assets = !this.HasAssets() ? null : new Assets
				{
					LargeImageKey = this.Assets.LargeImageKey != null ? this.Assets.LargeImageKey.Clone() as string : null,
					LargeImageText = this.Assets.LargeImageText != null ? this.Assets.LargeImageText.Clone() as string : null,
					SmallImageKey = this.Assets.SmallImageKey != null ? this.Assets.SmallImageKey.Clone() as string : null,
					SmallImageText = this.Assets.SmallImageText != null ? this.Assets.SmallImageText.Clone() as string : null
				},

				Party = !this.HasParty() ? null : new Party
				{
					ID = this.Party.ID,
					Size = this.Party.Size,
					Max = this.Party.Max,
					Privacy = this.Party.Privacy,
				},

			};
		}

		/// <summary>
		/// Merges the passed presence with this one, taking into account the image key to image id annoyance.
		/// </summary>
		/// <param name="presence"></param>
		/// <returns>self</returns>
		internal RichPresence Merge(BaseRichPresence presence)
		{
			this._state = presence.State;
			this._details = presence.Details;
			this.Party = presence.Party;
			this.Timestamps = presence.Timestamps;
			this.Secrets = presence.Secrets;

			//If they have assets, we should merge them
			if (presence.HasAssets())
			{
				//Make sure we actually have assets too
				if (!this.HasAssets())
				{
					//We dont, so we will just use theirs
					this.Assets = presence.Assets;
				}
				else
				{
					//We do, so we better merge them!
					this.Assets.Merge(presence.Assets);
				}
			}
			else
			{
				//They dont have assets, so we will just set ours to null
				this.Assets = null;
			}

			return this;
		}

		internal override bool Matches(RichPresence other)
		{
			if (!base.Matches(other)) return false;

			//Check buttons
			if (this.Buttons == null ^ other.Buttons == null) return false;
			if (this.Buttons != null)
			{
				if (this.Buttons.Length != other.Buttons.Length) return false;
				for (var i = 0; i < this.Buttons.Length; i++)
				{
					var a = this.Buttons[i];
					var b = other.Buttons[i];
					if (a.Label != b.Label || a.Url != b.Url)
						return false;
				}
			}

			return true;
		}

		#endregion

		/// <summary>
		/// Operator that converts a presence into a boolean for null checks.
		/// </summary>
		/// <param name="presesnce"></param>
		public static implicit operator bool(RichPresence presesnce) => presesnce != null;
	}

	/// <summary>
	/// A rich presence that has been parsed from the pipe as a response.
	/// </summary>
	internal sealed class RichPresenceResponse : BaseRichPresence
	{
		/// <summary>
		/// ID of the client
		/// </summary>
		[JsonProperty("application_id")]
		public string ClientID { get; private set; }

		/// <summary>
		/// Name of the bot
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; private set; }

	}
}
