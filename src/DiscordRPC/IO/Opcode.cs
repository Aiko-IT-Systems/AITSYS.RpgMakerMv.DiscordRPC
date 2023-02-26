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

namespace DiscordRPC.IO
{
	/// <summary>
	/// The operation code that the <see cref="PipeFrame"/> was sent under. This defines the type of frame and the data to expect.
	/// </summary>
	public enum Opcode : uint
	{
		/// <summary>
		/// Initial handshake frame
		/// </summary>
		Handshake = 0,

		/// <summary>
		/// Generic message frame
		/// </summary>
		Frame = 1,

		/// <summary>
		/// Discord has closed the connection
		/// </summary>
		Close = 2,

		/// <summary>
		/// Ping frame (not used?)
		/// </summary>
		Ping = 3,

		/// <summary>
		/// Pong frame (not used?)
		/// </summary>
		Pong = 4
	}
}
