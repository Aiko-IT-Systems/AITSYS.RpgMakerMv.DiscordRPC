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
using System.Diagnostics;
using System.IO;

using DiscordRPC.Logging;

namespace DiscordRPC.Registry
{
	internal class UnixUriSchemeCreator : IUriSchemeCreator
	{
		private readonly ILogger _logger;
		public UnixUriSchemeCreator(ILogger logger)
		{
			this._logger = logger;
		}

		public bool RegisterUriScheme(UriSchemeRegister register)
		{
			var home = Environment.GetEnvironmentVariable("HOME");
			if (string.IsNullOrEmpty(home))
			{
				this._logger.Error("Failed to register because the HOME variable was not set.");
				return false;
			}

			var exe = register.ExecutablePath;
			if (string.IsNullOrEmpty(exe))
			{
				this._logger.Error("Failed to register because the application was not located.");
				return false;
			}

			//Prepare the command
			string command = null;
			if (register.UsingSteamApp)
			{
				//A steam command isntead
				command = $"xdg-open steam://rungameid/{register.SteamAppID}";
			}
			else
			{
				//Just a regular discord command
				command = exe;
			}


			//Prepare the file
			var desktopFileFormat =
@"[Desktop Entry]
Name=Game {0}
Exec={1} %u
Type=Application
NoDisplay=true
Categories=Discord;Games;
MimeType=x-scheme-handler/discord-{2}";

			var file = string.Format(desktopFileFormat, register.ApplicationID, command, register.ApplicationID);

			//Prepare the path
			var filename = $"/discord-{register.ApplicationID}.desktop";
			var filepath = home + "/.local/share/applications";
			var directory = Directory.CreateDirectory(filepath);
			if (!directory.Exists)
			{
				this._logger.Error("Failed to register because {0} does not exist", filepath);
				return false;
			}

			//Write the file
			File.WriteAllText(filepath + filename, file);

			//Register the Mime type
			if (!this.RegisterMime(register.ApplicationID))
			{
				this._logger.Error("Failed to register because the Mime failed.");
				return false;
			}

			this._logger.Trace("Registered {0}, {1}, {2}", filepath + filename, file, command);
			return true;
		}

		private bool RegisterMime(string appid)
		{
			//Format the arguments
			var format = "default discord-{0}.desktop x-scheme-handler/discord-{0}";
			var arguments = string.Format(format, appid);

			//Run the process and wait for response
			var process = Process.Start("xdg-mime", arguments);
			process.WaitForExit();

			//Return if succesful
			return process.ExitCode >= 0;
		}
	}
}
