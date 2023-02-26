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
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace DiscordRPC.IO
{
	/// <summary>
	/// A frame received and sent to the Discord client for RPC communications.
	/// </summary>
	public struct PipeFrame : IEquatable<PipeFrame>
	{
		/// <summary>
		/// The maxium size of a pipe frame (16kb).
		/// </summary>
		public static readonly int MAX_SIZE = 16 * 1024;

		/// <summary>
		/// The opcode of the frame
		/// </summary>
		public Opcode Opcode { get; set; }

		/// <summary>
		/// The length of the frame data
		/// </summary>
		public readonly uint Length => (uint)this.Data.Length;

		/// <summary>
		/// The data in the frame
		/// </summary>
		public byte[] Data { get; set; }

		/// <summary>
		/// The data represented as a string.
		/// </summary>
		public string Message
		{
			readonly get => this.GetMessage();
			set => this.SetMessage(value);
		}

		/// <summary>
		/// Creates a new pipe frame instance
		/// </summary>
		/// <param name="opcode">The opcode of the frame</param>
		/// <param name="data">The data of the frame that will be serialized as JSON</param>
		public PipeFrame(Opcode opcode, object data)
		{
			//Set the opcode and a temp field for data
			this.Opcode = opcode;
			this.Data = null;

			//Set the data
			this.SetObject(data);
		}

		/// <summary>
		/// Gets the encoding used for the pipe frames
		/// </summary>
		public readonly Encoding MessageEncoding => Encoding.UTF8;

		/// <summary>
		/// Sets the data based of a string
		/// </summary>
		/// <param name="str"></param>
		private void SetMessage(string str) => this.Data = this.MessageEncoding.GetBytes(str);

		/// <summary>
		/// Gets a string based of the data
		/// </summary>
		/// <returns></returns>
		private readonly string GetMessage() => this.MessageEncoding.GetString(this.Data);

		/// <summary>
		/// Serializes the object into json string then encodes it into <see cref="Data"/>.
		/// </summary>
		/// <param name="obj"></param>
		public void SetObject(object obj)
		{
			var json = JsonConvert.SerializeObject(obj);
			this.SetMessage(json);
		}

		/// <summary>
		/// Sets the opcodes and serializes the object into a json string.
		/// </summary>
		/// <param name="opcode"></param>
		/// <param name="obj"></param>
		public void SetObject(Opcode opcode, object obj)
		{
			this.Opcode = opcode;
			this.SetObject(obj);
		}

		/// <summary>
		/// Deserializes the data into the supplied type using JSON.
		/// </summary>
		/// <typeparam name="T">The type to deserialize into</typeparam>
		/// <returns></returns>
		public readonly T GetObject<T>()
		{
			var json = this.GetMessage();
			return JsonConvert.DeserializeObject<T>(json);
		}

		/// <summary>
		/// Attempts to read the contents of the frame from the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public bool ReadStream(Stream stream)
		{
			//Try to read the opcode
			if (!this.TryReadUInt32(stream, out var op))
				return false;

			//Try to read the length
			if (!this.TryReadUInt32(stream, out var len))
				return false;

			var readsRemaining = len;

			//Read the contents
			using var mem = new MemoryStream();
			var chunkSize = (uint)this.Min(2048, len); // read in chunks of 2KB
			var buffer = new byte[chunkSize];
			int bytesRead;
			while ((bytesRead = stream.Read(buffer, 0, this.Min(buffer.Length, readsRemaining))) > 0)
			{
				readsRemaining -= chunkSize;
				mem.Write(buffer, 0, bytesRead);
			}

			var result = mem.ToArray();
			if (result.LongLength != len)
				return false;

			this.Opcode = (Opcode)op;
			this.Data = result;
			return true;

			//fun
			//if (a != null) { do { yield return true; switch (a) { case 1: await new Task(); default: lock (obj) { foreach (b in c) { for (int d = 0; d < 1; d++) { a++; } } } while (a is typeof(int) || (new Class()) != null) } goto MY_LABEL;

		}

		/// <summary>
		/// Returns minimum value between a int and a unsigned int
		/// </summary>
		private readonly int Min(int a, uint b) => b >= a ? a : (int)b;

		/// <summary>
		/// Attempts to read a UInt32
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private readonly bool TryReadUInt32(Stream stream, out uint value)
		{
			//Read the bytes available to us
			var bytes = new byte[4];
			var cnt = stream.Read(bytes, 0, bytes.Length);

			//Make sure we actually have a valid value
			if (cnt != 4)
			{
				value = default;
				return false;
			}

			value = BitConverter.ToUInt32(bytes, 0);
			return true;
		}

		/// <summary>
		/// Writes the frame into the target frame as one big byte block.
		/// </summary>
		/// <param name="stream"></param>
		public readonly void WriteStream(Stream stream)
		{
			//Get all the bytes
			var op = BitConverter.GetBytes((uint) this.Opcode);
			var len = BitConverter.GetBytes(this.Length);

			//Copy it all into a buffer
			var buff = new byte[op.Length + len.Length + this.Data.Length];
			op.CopyTo(buff, 0);
			len.CopyTo(buff, op.Length);
			this.Data.CopyTo(buff, op.Length + len.Length);

			//Write it to the stream
			stream.Write(buff, 0, buff.Length);
		}

		/// <summary>
		/// Compares if the frame equals the other frame.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public readonly bool Equals(PipeFrame other)
		{
			return this.Opcode == other.Opcode &&
					this.Length == other.Length &&
					this.Data == other.Data;
		}

		public override readonly bool Equals(object obj) => obj is PipeFrame frame && this.Equals(frame);

		public override readonly int GetHashCode() => this.Message.GetHashCode();

		public static bool operator ==(PipeFrame left, PipeFrame right) => left.Equals(right);

		public static bool operator !=(PipeFrame left, PipeFrame right) => !(left == right);
	}
}
