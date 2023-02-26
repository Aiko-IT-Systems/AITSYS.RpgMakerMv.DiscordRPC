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

namespace DiscordRPC.Logging
{
	/// <summary>
	/// Logs the outputs to the console using <see cref="Console.WriteLine()"/>
	/// </summary>
	public class ConsoleLogger : ILogger
	{
		/// <summary>
		/// The level of logging to apply to this logger.
		/// </summary>
		public LogLevel Level { get; set; }

		/// <summary>
		/// Should the output be coloured?
		/// </summary>
		public bool Coloured { get; set; }

		/// <summary>
		/// Creates a new instance of a Console Logger.
		/// </summary>
		public ConsoleLogger()
		{
			this.Level = LogLevel.Info;
			this.Coloured = false;
		}

		/// <summary>
		/// Creates a new instance of a Console Logger
		/// </summary>
		/// <param name="level">The log level</param>
		public ConsoleLogger(LogLevel level)
			: this()
		{
			this.Level = level;
		}

		/// <summary>
		/// Creates a new instance of a Console Logger with a set log level
		/// </summary>
		/// <param name="level">The log level</param>
		/// <param name="coloured">Should the logs be in colour?</param>
		public ConsoleLogger(LogLevel level, bool coloured)
		{
			this.Level = level;
			this.Coloured = coloured;
		}

		/// <summary>
		/// Informative log messages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Trace(string message, params object[] args)
		{
			if (this.Level > LogLevel.Trace) return;

			if (this.Coloured) Console.ForegroundColor = ConsoleColor.Gray;

			var prefixedMessage = "TRACE: " + message;

			if (args.Length > 0)
			{
				Console.WriteLine(prefixedMessage, args);
			}
			else
			{
				Console.WriteLine(prefixedMessage);
			}
		}

		/// <summary>
		/// Informative log messages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Info(string message, params object[] args)
		{
			if (this.Level > LogLevel.Info) return;

			if (this.Coloured) Console.ForegroundColor = ConsoleColor.White;

			var prefixedMessage = "INFO: " + message;

			if (args.Length > 0)
			{
				Console.WriteLine(prefixedMessage, args);
			}
			else
			{
				Console.WriteLine(prefixedMessage);
			}
		}

		/// <summary>
		/// Warning log messages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Warning(string message, params object[] args)
		{
			if (this.Level > LogLevel.Warning) return;

			if (this.Coloured) Console.ForegroundColor = ConsoleColor.Yellow;

			var prefixedMessage = "WARN: " + message;

			if (args.Length > 0)
			{
				Console.WriteLine(prefixedMessage, args);
			}
			else
			{
				Console.WriteLine(prefixedMessage);
			}
		}

		/// <summary>
		/// Error log messsages
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void Error(string message, params object[] args)
		{
			if (this.Level > LogLevel.Error) return;

			if (this.Coloured) Console.ForegroundColor = ConsoleColor.Red;

			var prefixedMessage = "ERR : " + message;

			if (args.Length > 0)
			{
				Console.WriteLine(prefixedMessage, args);
			}
			else
			{
				Console.WriteLine(prefixedMessage);
			}
		}

	}
}
