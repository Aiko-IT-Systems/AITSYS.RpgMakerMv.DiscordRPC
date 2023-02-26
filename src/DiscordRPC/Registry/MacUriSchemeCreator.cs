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

using System.IO;

using DiscordRPC.Logging;

namespace DiscordRPC.Registry
{
	internal class MacUriSchemeCreator : IUriSchemeCreator
	{
		private readonly ILogger _logger;
		public MacUriSchemeCreator(ILogger logger)
		{
			this._logger = logger;
		}

		public bool RegisterUriScheme(UriSchemeRegister register)
		{
			//var home = Environment.GetEnvironmentVariable("HOME");
			//if (string.IsNullOrEmpty(home)) return;     //TODO: Log Error

			var exe = register.ExecutablePath;
			if (string.IsNullOrEmpty(exe))
			{
				this._logger.Error("Failed to register because the application could not be located.");
				return false;
			}

			this._logger.Trace("Registering Steam Command");

			//Prepare the command
			var command = exe;
			if (register.UsingSteamApp) command = $"steam://rungameid/{register.SteamAppID}";
			else this._logger.Warning("This library does not fully support MacOS URI Scheme Registration.");

			//get the folder ready
			var filepath = "~/Library/Application Support/discord/games";
			var directory = Directory.CreateDirectory(filepath);
			if (!directory.Exists)
			{
				this._logger.Error("Failed to register because {0} does not exist", filepath);
				return false;
			}

			//Write the contents to file
			var applicationSchemeFilePath = $"{filepath}/{register.ApplicationID}.json";
			File.WriteAllText(applicationSchemeFilePath, "{ \"command\": \"" + command + "\" }");
			this._logger.Trace("Registered {0}, {1}", applicationSchemeFilePath, command);
			return true;
		}

	}
}
