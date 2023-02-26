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

using DiscordRPC.Logging;

namespace DiscordRPC.IO
{
	/// <summary>
	/// Pipe Client used to communicate with Discord.
	/// </summary>
	public interface INamedPipeClient : IDisposable
	{

		/// <summary>
		/// The logger for the Pipe client to use
		/// </summary>
		ILogger Logger { get; set; }

		/// <summary>
		/// Is the pipe client currently connected?
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// The pipe the client is currently connected too
		/// </summary>
		int ConnectedPipe { get; }

		/// <summary>
		/// Attempts to connect to the pipe. If 0-9 is passed to pipe, it should try to only connect to the specified pipe. If -1 is passed, the pipe will find the first available pipe.
		/// </summary>
		/// <param name="pipe">If -1 is passed, the pipe will find the first available pipe, otherwise it connects to the pipe that was supplied</param>
		/// <returns></returns>
		bool Connect(int pipe);

		/// <summary>
		/// Reads a frame if there is one available. Returns false if there is none. This should be non blocking (aka use a Peek first).
		/// </summary>
		/// <param name="frame">The frame that has been read. Will be <code>default(PipeFrame)</code> if it fails to read</param>
		/// <returns>Returns true if a frame has been read, otherwise false.</returns>
		bool ReadFrame(out PipeFrame frame);

		/// <summary>
		/// Writes the frame to the pipe. Returns false if any errors occur.
		/// </summary>
		/// <param name="frame">The frame to be written</param>
		bool WriteFrame(PipeFrame frame);

		/// <summary>
		/// Closes the connection
		/// </summary>
		void Close();

	}
}
