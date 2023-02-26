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

namespace DiscordRPC.Logging
{
	/// <summary>
	/// Logs the outputs to a file
	/// </summary>
	public class FileLogger : ILogger
	{
		/// <summary>
		/// The level of logging to apply to this logger.
		/// </summary>
		public LogLevel Level { get; set; }

		/// <summary>
		/// Should the output be coloured?
		/// </summary>
		public string File { get; set; }

		private readonly object _filelock;

		/// <summary>
		/// Creates a new instance of the file logger
		/// </summary>
		/// <param name="path">The path of the log file.</param>
		public FileLogger(string path)
			: this(path, LogLevel.Info) { }

		/// <summary>
		/// Creates a new instance of the file logger
		/// </summary>
		/// <param name="path">The path of the log file.</param>
		/// <param name="level">The level to assign to the logger.</param>
		public FileLogger(string path, LogLevel level)
		{
			this.Level = level;
			this.File = path;
			this._filelock = new object();
		}


		/// <summary>
		/// Informative log messages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Trace(string message, params object[] args)
		{
			if (this.Level > LogLevel.Trace) return;
			lock (this._filelock) System.IO.File.AppendAllText(this.File, "\r\nTRCE: " + (args.Length > 0 ? string.Format(message, args) : message));
		}

		/// <summary>
		/// Informative log messages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Info(string message, params object[] args)
		{
			if (this.Level > LogLevel.Info) return;
			lock (this._filelock) System.IO.File.AppendAllText(this.File, "\r\nINFO: " + (args.Length > 0 ? string.Format(message, args) : message));
		}

		/// <summary>
		/// Warning log messages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Warning(string message, params object[] args)
		{
			if (this.Level > LogLevel.Warning) return;
			lock (this._filelock)
				System.IO.File.AppendAllText(this.File, "\r\nWARN: " + (args.Length > 0 ? string.Format(message, args) : message));
		}

		/// <summary>
		/// Error log messsages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Error(string message, params object[] args)
		{
			if (this.Level > LogLevel.Error) return;
			lock (this._filelock)
				System.IO.File.AppendAllText(this.File, "\r\nERR : " + (args.Length > 0 ? string.Format(message, args) : message));
		}

	}
}
